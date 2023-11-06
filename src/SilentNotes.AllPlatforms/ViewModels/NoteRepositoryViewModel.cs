// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MudBlazor;
using SilentNotes.Crypto;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Stories;
using SilentNotes.Stories.SynchronizationStory;
using SilentNotes.Workers;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the main page with the overview of the notes.
    /// </summary>
    public class NoteRepositoryViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly ISynchronizationService _synchronizationService;
        private readonly IRepositoryStorageService _repositoryService;
        private readonly IFeedbackService _feedbackService;
        private readonly IThemeService _themeService;
        private readonly ISettingsService _settingsService;
        private readonly IEnvironmentService _environmentService;
        private readonly SearchableHtmlConverter _searchableTextConverter;
        private readonly ICryptor _noteCryptor;
        private readonly List<string> _filterTags;
        private NoteRepositoryModel _model;
        private long? _originalFingerPrint;
        private NoteViewModelReadOnly _selectedOrderNote;
        private ITreeItemViewModel _selectedTagNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteRepositoryViewModel"/> class.
        /// </summary>
        public NoteRepositoryViewModel(
            NoteRepositoryModel model,
            ILanguageService languageService,
            INavigationService navigationService,
            IFeedbackService feedbackService,
            IThemeService themeService,
            ISettingsService settingsService,
            IEnvironmentService environmentService,
            ISynchronizationService synchronizationService,
            ICryptoRandomSource randomSource,
            IRepositoryStorageService repositoryService)
        {
            Language = languageService;
            _navigationService = navigationService;
            _feedbackService = feedbackService;
            _themeService = themeService;
            _settingsService = settingsService;
            _environmentService = environmentService;
            _repositoryService = repositoryService;
            _synchronizationService = synchronizationService;
            _noteCryptor = new Cryptor(NoteModel.CryptorPackageName, randomSource);
            _searchableTextConverter = new SearchableHtmlConverter();
            _filterTags = new List<string>();
            AllNotes = new List<NoteViewModelReadOnly>();
            FilteredNotes = new ObservableCollection<NoteViewModelReadOnly>();
            TagsRootNode = new TagTreeItemViewModel(null, null, AllNotes);

            Model = model;
            ResetIsModified();

            // Initialize commands and events
            NewNoteCommand = new RelayCommand(NewNote);
            NewChecklistCommand = new RelayCommand(NewChecklist);
            DeleteNoteCommand = new RelayCommand<object>(DeleteNote);
            ClearFilterCommand = new RelayCommand(ClearFilter);
            SynchronizeCommand = new RelayCommand(Synchronize);
            CloseSafeCommand = new RelayCommand(CloseSafe);
            SelectTagNodeCommand = new AsyncRelayCommand<ITreeItemViewModel>(SelectTagNode);
            ClearTagFilterCommand = new RelayCommand(ClearTagFilter);
            ToggleNotesWithoutTagsFilterCommand = new RelayCommand(ToggleNotesWithoutTagsFilter);

            SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
            IsDrawerOpen = settings.StartWithTagsOpen;
        }

        /// <summary>
        /// Adds all root nodes to the tag tree.
        /// </summary>
        /// <returns>Task for async call.</returns>
        public async Task InitializeTagTree()
        {
            TagsRootNode.ResetChildren();
            await TagsRootNode.LazyLoadChildren();

            SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
            FilterNotesWithoutTags = settings.FilterNotesWithoutTags;

            // Try to reapply the selected tags
            ITreeItemViewModel parent = TagsRootNode;
            ITreeItemViewModel child = null;
            foreach (string filterTag in settings.FilterTags)
            {
                child = parent.Children.FirstOrDefault(child => child.Title == filterTag);
                if (child == null)
                    break;

                await child.Expand();
                parent = child;
            }
            if (child != null)
                SelectedTagNode = child;

            ApplyFilter();
        }

        private ILanguageService Language { get; }

        /// <summary>
        /// Gets a value indicating whether the repository was modifed.
        /// </summary>
        internal bool IsModified
        {
            get { return _originalFingerPrint != Model.GetModificationFingerprint(); }
        }

        /// <summary>
        /// Sets the <see cref="IsModified"/> to false by recalculating the fingerprint.
        /// </summary>
        internal void ResetIsModified()
        {
            _originalFingerPrint = Model?.GetModificationFingerprint();
        }

        /// <inheritdoc/>
        public override void OnStoringUnsavedData()
        {
            // If there was an error reading the existing repository, we do not overwrite it, to
            // prevent further damage.
            if (Model == null)
                return;

            if (IsModified)
            {
                _repositoryService.TrySaveRepository(Model);
                ResetIsModified();
            }

            SettingsModel settings = _settingsService.LoadSettingsOrDefault();
            if (settings.StartWithTagsOpen != IsDrawerOpen)
            {
                settings.StartWithTagsOpen = IsDrawerOpen;
                _settingsService.TrySaveSettingsToLocalDevice(settings);
            }
        }

        public int NoteMinHeight
        {
            get
            {
                // The minimum must not be bigger than the maximum.
                return Math.Min(SettingsViewModel.ReferenceNoteMinSize, NoteMaxHeight);
            }
        }

        public int NoteMaxHeight
        {
            get
            {
                SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
                SliderStepConverter noteMaxHeightConverter = new SliderStepConverter(SettingsViewModel.ReferenceNoteMaxSize, 20.0);
                return noteMaxHeightConverter.ModelFactorToValueAsInt(settings.NoteMaxHeightScale);
            }
        }

        /// <summary>
        /// Gets or sets a list of viewmodels for all notes of the repository.
        /// </summary>
        private List<NoteViewModelReadOnly> AllNotes { get; set; }

        /// <summary>
        /// Gets a bindable list of the visible notes, which are not filtered out.
        /// </summary>
        public ObservableCollection<NoteViewModelReadOnly> FilteredNotes { get; private set; }

        /// <summary>
        /// Gets the root node of the tag tree, its <see cref="ITreeItemViewModel.Children"/>
        /// list can be used for data binding.
        /// </summary>
        public TagTreeItemViewModel TagsRootNode { get; }

        /// <summary>
        /// This command can be used when the user clicks a node in the tag filter tree.
        /// </summary>
        public ICommand SelectTagNodeCommand { get; }

        private async Task SelectTagNode(ITreeItemViewModel treeItem)
        {
            FilterNotesWithoutTags = false;

            if (treeItem == SelectedTagNode)
                SelectedTagNode = null; // Unselect node
            else
                SelectedTagNode = treeItem; // Select node

            if (SelectedTagNode != null)
                await SelectedTagNode.Expand();

            ApplyFilter();
        }

        /// <summary>
        /// Gets the command which handles the moving of a note to the recyclebin.
        /// </summary>
        public ICommand ClearTagFilterCommand { get; }

        private void ClearTagFilter()
        {
            FilterNotesWithoutTags = false;
            SelectedTagNode = null;
            ApplyFilter();
        }

        /// <summary>
        /// Gets the command which toggles the <see cref="FilterNotesWithoutTags"/> state.
        /// </summary>
        public ICommand ToggleNotesWithoutTagsFilterCommand { get; }

        private void ToggleNotesWithoutTagsFilter()
        {
            FilterNotesWithoutTags = !FilterNotesWithoutTags;
            SelectedTagNode = null;
            ApplyFilter();
        }

        /// <summary>
        /// Gets or sets a value indicating whether the special filtering for notes without tags
        /// is active or not.
        /// </summary>
        public bool FilterNotesWithoutTags
        {
            get { return _settingsService.LoadSettingsOrDefault().FilterNotesWithoutTags; }
            set { SetProperty(FilterNotesWithoutTags, value, (bool v) => _settingsService.LoadSettingsOrDefault().FilterNotesWithoutTags = v); }
        }

        /// <summary>
        /// Gets or sets the currently selected tag node and marks its anchestors as IsSelected.
        /// </summary>
        public ITreeItemViewModel SelectedTagNode
        {
            get { return _selectedTagNode; }

            set
            {
                _selectedTagNode = value;
                foreach (var node in TagsRootNode.EnumerateSiblingsRecursive(true))
                    node.IsSelected = false;

                // Mark all parent items as selected
                if (_selectedTagNode != null)
                {
                    foreach (var node in _selectedTagNode.EnumerateAnchestorsRecursive(true))
                        node.IsSelected = true;
                }

                SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
                settings.FilterTags = GetFilterTags();
            }
        }

        /// <summary>
        /// Gets a list of selected tags, starting from the root node and ending at the selected node.
        /// </summary>
        /// <returns>List of tags.</returns>
        public List<string> GetFilterTags()
        {
            _filterTags.Clear();
            if (SelectedTagNode != null)
            {
                _filterTags.AddRange(SelectedTagNode.EnumerateAnchestorsRecursive(true)
                    .Select(node => node.Title)
                    .Where(tag => !string.IsNullOrEmpty(tag))
                    .Reverse());
            }
            return _filterTags;
        }

        /// <summary>
        /// Gets or sets the search filter.
        /// A two-way-binding can end up in an endless loop, when typing very fast.
        /// </summary>
        public string Filter
        {
            get { return _settingsService?.LoadSettingsOrDefault().Filter; }

            set
            {
                SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
                if (SetProperty(settings.Filter, value, (string v) => settings.Filter = v))
                {
                    OnPropertyChanged(nameof(Filter));
                    OnPropertyChanged(nameof(IsFiltered));
                    ApplyFilter();
                    OnPropertyChanged("Notes");
                }
            }
        }

        public bool IsFiltered
        {
            get { return !string.IsNullOrEmpty(Filter); }
        }

        private void ApplyFilter()
        {
            SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
            string normalizedFilter = SearchableHtmlConverter.NormalizeWhitespaces(settings.Filter);
            var options = FilterNotesWithoutTags ? NoteFilter.FilterOptions.NotesWithoutTags : NoteFilter.FilterOptions.FilterByTagList;
            NoteFilter noteFilter = new NoteFilter(normalizedFilter, GetFilterTags(), options);

            FilteredNotes.Clear();
            foreach (NoteViewModelReadOnly noteViewModel in AllNotes)
            {
                bool hideNote =
                    noteViewModel.InRecyclingBin ||
                    (settings.HideClosedSafeNotes && noteViewModel.IsLocked) ||
                    !noteFilter.MatchTags(noteViewModel.Tags) ||
                    !noteFilter.ContainsPattern(noteViewModel.SearchableContent);

                if (!hideNote)
                    FilteredNotes.Add(noteViewModel);
            }
        }

        /// <summary>
        /// Gets the command which clears the search filter.
        /// </summary>
        public ICommand ClearFilterCommand { get; private set; }

        private void ClearFilter()
        {
            Filter = null;
            OnPropertyChanged("ClearFilter");
        }

        /// <summary>
        /// Gets the command which handles the creation of a new note.
        /// </summary>
        public ICommand NewNoteCommand { get; private set; }

        private void NewNote()
        {
            NewNote(NoteType.Text);
        }

        private void NewNote(NoteType noteType)
        {
            ClearFilter();

            // Create new note and update model list
            NoteModel noteModel = new NoteModel();
            noteModel.NoteType = noteType;
            noteModel.BackgroundColorHex = _settingsService.LoadSettingsOrDefault().DefaultNoteColorHex;

            // Update view model list
            var noteViewModel = new NoteViewModelReadOnly(
                noteModel,
                _searchableTextConverter,
                _themeService,
                _settingsService,
                _noteCryptor,
                _model.Safes);
            NoteInsertionMode insertionMode = _settingsService.LoadSettingsOrDefault().DefaultNoteInsertion;
            switch (insertionMode)
            {
                case NoteInsertionMode.AtTop:
                    var lastPinned = AllNotes.LastOrDefault(x => x.IsPinned == true);
                    var index = lastPinned == null ? 0 : AllNotes.IndexOf(lastPinned) + 1;
                    _model.Notes.Insert(index, noteModel);
                    AllNotes.Insert(index, noteViewModel);
                    break;

                case NoteInsertionMode.AtBottom:
                    _model.Notes.Add(noteModel);
                    AllNotes.Add(noteViewModel);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(insertionMode));
            }

            _navigationService.NavigateTo(noteViewModel.Route);
        }

        /// <summary>
        /// Gets the command which handles the creation of a new checklist note.
        /// </summary>
        public ICommand NewChecklistCommand { get; private set; }

        private void NewChecklist()
        {
            NewNote(NoteType.Checklist);
        }

        /// <summary>
        /// Gets the command which handles the moving of a note to the recyclebin.
        /// </summary>
        public ICommand DeleteNoteCommand { get; private set; }

        private async void DeleteNote(object value)
        {
            Guid noteId = (value is Guid) ? (Guid)value : new Guid(value.ToString());
            NoteViewModelReadOnly selectedNote = AllNotes.Find(item => item.Id == noteId);
            if (selectedNote == null)
                return;

            // Mark note as deleted
            selectedNote.InRecyclingBin = true;

            // Remove note from filtered list
            int selectedIndex = FilteredNotes.IndexOf(selectedNote);
            FilteredNotes.RemoveAt(selectedIndex);
            await InitializeTagTree();

            _feedbackService.ShowToast(Language.LoadText("feedback_note_to_recycle"), Severity.Info);
        }

        /// <summary>
        /// Gets a value indicating whether the notes are filtered by a tag or not.
        /// </summary>
        public bool IsFilteredByTag
        {
            get { return (SelectedTagNode != null) || FilterNotesWithoutTags; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the side drawer displaying the tags is open or not.
        /// </summary>
        public bool IsDrawerOpen { get; set; }

        /// <summary>
        /// Gets the command which handles the synchronization with the web server.
        /// </summary>
        public ICommand SynchronizeCommand { get; private set; }

        private async void Synchronize()
        {
            OnStoringUnsavedData();
            await _synchronizationService.SynchronizeManually(Ioc.Instance);
        }

        /// <summary>
        /// Gets the command which closes encrypted notes.
        /// </summary>
        public ICommand CloseSafeCommand { get; private set; }

        private void CloseSafe()
        {
            foreach (SafeModel safe in Model.Safes)
                safe.Close();
            _navigationService.NavigateHome();
        }

        /// <summary>
        /// Gets a value indicating whether as leat one safe exists.
        /// </summary>
        public bool SafeExists
        {
            get { return Model.Safes.Count > 0; }
        }

        /// <summary>
        /// Gets a value indicating whether at least one safe is open.
        /// </summary>
        public bool IsAnySafeOpen
        {
            get { return Model.Safes.Any(safe => safe.IsOpen); }
        }

        public void AddNoteToSafe(Guid noteId)
        {
            NoteViewModelReadOnly note = AllNotes.Find(item => item.Id == noteId);
            SafeModel oldestOpenSafe = Model.Safes.FindOldestOpenSafe();
            if ((note != null) && (oldestOpenSafe != null))
            {
                note.Model.SafeId = oldestOpenSafe.Id;
                note.Model.HtmlContent = note.Lock(note.UnlockedHtmlContent);
                note.Model.RefreshModifiedAt();
            }
        }

        public void RemoveNoteFromSafe(Guid noteId)
        {
            NoteViewModelReadOnly note = AllNotes.Find(item => item.Id == noteId);
            if (note != null)
            {
                note.Model.SafeId = null;
                note.Model.HtmlContent = note.UnlockedHtmlContent;
                note.Model.RefreshModifiedAt();
            }
        }

        /// <summary>
        /// Gets or sets the wrapped model.
        /// </summary>
        internal NoteRepositoryModel Model
        {
            get { return _model; }

            set
            {
                SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
                FilteredNotes.Clear();
                AllNotes.Clear();
                _model = value;

                // Wrap models in view models
                List<string> allTags = _model.CollectActiveTags();
                foreach (NoteModel note in _model.Notes)
                {
                    var noteViewModel = new NoteViewModelReadOnly(
                        note,
                        _searchableTextConverter,
                        _themeService,
                        _settingsService,
                        _noteCryptor,
                        _model.Safes);
                    AllNotes.Add(noteViewModel);

                    bool hideNote =
                        noteViewModel.InRecyclingBin ||
                        (settings.HideClosedSafeNotes && noteViewModel.IsLocked);

                    if (!hideNote)
                        FilteredNotes.Add(noteViewModel);
                }
            }
        }

        /// <summary>
        /// Handles the user selection of the ordering note.
        /// </summary>
        /// <param name="note">The note which was selected ba the user.</param>
        public void SelectOrderNote(NoteViewModelReadOnly note)
        {
            if (note == SelectedOrderNote)
                SelectedOrderNote = null; // clicking twice removes selection
            else
                SelectedOrderNote = note;
        }

        /// <summary>
        /// Gets the user selected note for ordering, or null when no note is selected.
        /// </summary>
        public NoteViewModelReadOnly SelectedOrderNote 
        {
            get { return _selectedOrderNote; }
            
            private set
            {
                if (SetProperty(ref _selectedOrderNote, value))
                    OnPropertyChanging(nameof(OrderToolbarVisible));
            }
        }

        /// <summary>
        /// Gets a value indicating whether the ordering toolbar shoulc be visible or not.
        /// </summary>
        public bool OrderToolbarVisible
        {
            get { return SelectedOrderNote != null; }
        }

        public void MoveSelectedOrderNote(bool upwards, bool singleStep)
        {
            if (SelectedOrderNote == null)
                return;

            var notePositions = NoteMover.GetNotePositions(AllNotes, FilteredNotes, SelectedOrderNote, upwards, singleStep);
            if (notePositions == null)
                return;

            // Move the notes in all lists of model and viewmodel
            NoteMover.ListMove(_model.Notes, notePositions.OldAllNotesPos, notePositions.NewAllNotesPos);
            NoteMover.ListMove(AllNotes, notePositions.OldAllNotesPos, notePositions.NewAllNotesPos);
            FilteredNotes.Move(notePositions.OldFilteredNotesPos, notePositions.NewFilteredNotesPos);

            NoteMover.AdjustPinStatusAfterMoving(AllNotes, notePositions.NewAllNotesPos);
            _model.RefreshOrderModifiedAt();

            WeakReferenceMessenger.Default.Send(new BringSelectedNoteIntoViewMessage());
        }

        /// <summary>
        /// This message can be used to signal that the currently selected note should be brought
        /// into view.
        /// </summary>
        internal class BringSelectedNoteIntoViewMessage
        {
        }
    }
}