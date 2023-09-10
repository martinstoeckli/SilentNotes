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
        private readonly IStoryBoardService _storyBoardService;
        private readonly IRepositoryStorageService _repositoryService;
        private readonly IFeedbackService _feedbackService;
        private readonly IThemeService _themeService;
        private readonly ISettingsService _settingsService;
        private readonly IEnvironmentService _environmentService;
        private readonly IAutoSynchronizationService _autoSynchronizationService;
        private readonly SearchableHtmlConverter _searchableTextConverter;
        private readonly ICryptor _noteCryptor;
        private readonly KeyValueList<string, string> _specialTagLocalizations;
        private NoteRepositoryModel _model;
        private long? _originalFingerPrint;
        private NoteViewModelReadOnly _selectedOrderNote;

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
            IStoryBoardService storyBoardService,
            IAutoSynchronizationService autoSynchronizationService,
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
            _storyBoardService = storyBoardService;
            _autoSynchronizationService = autoSynchronizationService;
            _noteCryptor = new Cryptor(NoteModel.CryptorPackageName, randomSource);
            _searchableTextConverter = new SearchableHtmlConverter();
            AllNotes = new List<NoteViewModelReadOnly>();
            FilteredNotes = new ObservableCollection<NoteViewModelReadOnly>();

            _specialTagLocalizations = new KeyValueList<string, string>();
            _specialTagLocalizations[NoteFilter.SpecialTags.AllNotes] = string.Format("«{0}»", Language.LoadText("filter_show_all_notes"));
            _specialTagLocalizations[NoteFilter.SpecialTags.NotesWithoutTags] = string.Format("«{0}»", Language.LoadText("filter_only_without_tags"));

            Model = model;
            _originalFingerPrint = Model?.GetModificationFingerprint();
            UpdateTags();

            // Initialize commands and events
            NewNoteCommand = new RelayCommand(NewNote);
            NewChecklistCommand = new RelayCommand(NewChecklist);
            DeleteNoteCommand = new RelayCommand<object>(DeleteNote);
            ClearFilterCommand = new RelayCommand(ClearFilter);
            SynchronizeCommand = new RelayCommand(Synchronize);
            CloseSafeCommand = new RelayCommand(CloseSafe);

            // If a filter was set before e.g. opening a note, set the same filter again.
            SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
            if (!SelectedTagExistsInTags())
                settings.SelectedTag = NoteFilter.SpecialTags.AllNotes;

            if ((settings.SelectedTag != NoteFilter.SpecialTags.AllNotes) || !string.IsNullOrEmpty(settings.Filter))
            {
                OnPropertyChanged(nameof(SelectedTag));
                OnPropertyChanged(nameof(Filter));
                OnPropertyChanged(nameof(IsFiltered));
                ApplyFilter();
                OnPropertyChanged("Notes");
            }
        }

        private ILanguageService Language { get; }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="NoteRepositoryViewModel"/> class.
        ///// </summary>
        //public NoteRepositoryViewModel(
        //    INavigationService navigationService,
        //    ILanguageService languageService,
        //    ISvgIconService svgIconService,
        //    IThemeService themeService,
        //    IBaseUrlService webviewBaseUrl,
        //    IStoryBoardService storyBoardService,
        //    IFeedbackService feedbackService,
        //    ISettingsService settingsService,
        //    IEnvironmentService environmentService,
        //    IAutoSynchronizationService autoSynchronizationService,
        //    ICryptoRandomSource randomSource,
        //    IRepositoryStorageService repositoryService)
        //    : base(navigationService, languageService, svgIconService, themeService, webviewBaseUrl)
        //{
        //    _storyBoardService = storyBoardService;
        //    _repositoryService = repositoryService;
        //    _feedbackService = feedbackService;
        //    _settingsService = settingsService;
        //    _environmentService = environmentService;
        //    _autoSynchronizationService = autoSynchronizationService;
        //    _noteCryptor = new Cryptor(NoteModel.CryptorPackageName, randomSource);
        //    _searchableTextConverter = new SearchableHtmlConverter();
        //    AllNotes = new List<NoteViewModel>();
        //    FilteredNotes = new ObservableCollection<NoteViewModel>();

        //    _specialTagLocalizations = new KeyValueList<string, string>();
        //    _specialTagLocalizations[NoteFilter.SpecialTags.AllNotes] = string.Format("«{0}»", Language.LoadText("filter_show_all_notes"));
        //    _specialTagLocalizations[NoteFilter.SpecialTags.NotesWithoutTags] = string.Format("«{0}»", Language.LoadText("filter_only_without_tags"));

        //    _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);
        //    Model = noteRepository;
        //    UpdateTags();

        //    // Initialize commands and events
        //    ShowNoteCommand = new RelayCommand<object>(ShowNote);
        //    NewNoteCommand = new RelayCommand(NewNote);
        //    NewChecklistCommand = new RelayCommand(NewChecklist);
        //    DeleteNoteCommand = new RelayCommand<object>(DeleteNote);
        //ClearFilterCommand = new RelayCommand(ClearFilter);
        //    SynchronizeCommand = new RelayCommand(Synchronize);
        //    ShowTransferCodeCommand = new RelayCommand(ShowTransferCode);
        //    ShowSettingsCommand = new RelayCommand(ShowSettings);
        //    ShowRecycleBinCommand = new RelayCommand(ShowRecycleBin);
        //    ShowExportCommand = new RelayCommand(ShowExport);
        //    ShowInfoCommand = new RelayCommand(ShowInfo);
        //    OpenSafeCommand = new RelayCommand(OpenSafe);
        //    CloseSafeCommand = new RelayCommand(CloseSafe);
        //    ChangeSafePasswordCommand = new RelayCommand(ChangeSafePassword);

        //    Modified = false;

        //    // If a filter was set before e.g. opening a note, set the same filter again.
        //    SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
        //    if (!SelectedTagExistsInTags())
        //        settings.SelectedTag = NoteFilter.SpecialTags.AllNotes;

        //    if ((settings.SelectedTag != NoteFilter.SpecialTags.AllNotes) || !string.IsNullOrEmpty(settings.Filter))
        //    {
        //        OnPropertyChanged(nameof(SelectedTag));
        //        OnPropertyChanged(nameof(Filter));
        //        OnPropertyChanged(nameof(IsFiltered));
        //        ApplyFilter();
        //        OnPropertyChanged("Notes");
        //    }
        //}

        private bool SelectedTagExistsInTags()
        {
            // An invalid selected tag can exist if the user edited a note (deleted a tag) and
            // returned to the overview, which still remembers this selected tag.
            SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
            if (NoteFilter.SpecialTags.IsSpecialTag(settings.SelectedTag))
                return true;

            NoteFilter noteFilter = new NoteFilter(null, settings.SelectedTag);
            return noteFilter.ContainsTag(Tags.Select(tag => tag.Value));
        }

        /// <inheritdoc/>
        public override void OnStoringUnsavedData()
        {
            // If there was an error reading the existing repository, we do not overwrite it, to
            // prevent further damage.
            if (Model == null)
                return;

            long? fingerPrint = Model?.GetModificationFingerprint();
            if (fingerPrint != _originalFingerPrint)
            {
                _repositoryService.TrySaveRepository(Model);
                _originalFingerPrint = fingerPrint;
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
            NoteFilter noteFilter = new NoteFilter(normalizedFilter, settings.SelectedTag);

            FilteredNotes.Clear();
            foreach (NoteViewModelReadOnly noteViewModel in AllNotes)
            {
                bool hideNote =
                    noteViewModel.InRecyclingBin ||
                    (settings.HideClosedSafeNotes && noteViewModel.IsLocked) ||
                    !noteFilter.ContainsTag(noteViewModel.Tags) ||
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
            Modified = true;
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

            string targetRoute;
            switch (noteType)
            {
                case NoteType.Text:
                    targetRoute = "/note/" + noteViewModel.Id;
                    break;
                case NoteType.Checklist:
                    targetRoute = "/checklist/" + noteViewModel.Id;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(noteType));
            }
            _navigationService.NavigateTo(targetRoute);
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

        private void DeleteNote(object value)
        {
            Guid noteId = (value is Guid) ? (Guid)value : new Guid(value.ToString());
            NoteViewModelReadOnly selectedNote = AllNotes.Find(item => item.Id == noteId);
            if (selectedNote == null)
                return;
            Modified = true;

            // Mark note as deleted
            selectedNote.InRecyclingBin = true;

            // Remove note from filtered list
            int selectedIndex = FilteredNotes.IndexOf(selectedNote);
            FilteredNotes.RemoveAt(selectedIndex);
            UpdateTags();

            // If the note was the last one containing the selected tag, the filter should be cleared
            if (!SelectedTagExistsInTags())
                SelectedTag = null;
            else
                OnPropertyChanged("Notes");

            _feedbackService.ShowToast(Language.LoadText("feedback_note_to_recycle"), Severity.Info);
        }

        /// <summary>
        /// Gets a list of all tags which are used in the notes.
        /// </summary>
        public List<ListItemViewModel<string>> Tags { get; private set; }

        public object SelectedTag
        {
            get
            {
                SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
                ListItemViewModel<string> result = Tags.Find(item => string.Equals(item.Value, settings.SelectedTag, StringComparison.InvariantCultureIgnoreCase));
                return result ?? Tags[0];
            }

            set
            {
                string newValue = ((ListItemViewModel<string>)value).Value;
                SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
                if (SetProperty(settings.SelectedTag, newValue, (string v) => settings.SelectedTag = v))
                {
                    OnPropertyChanged(nameof(SelectedTag));
                    ApplyFilter();
                    OnPropertyChanged("Notes");
                    _settingsService.TrySaveSettingsToLocalDevice(settings);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the notes are filtered by a tag or not.
        /// </summary>
        public bool IsFilteredByTag
        {
            get { return SelectedTag != Tags[0]; } // The first tag is "all notes".
        }

        /// <summary>
        /// Gets or sets a value indicating whether the side drawer displaying the tags is open or not.
        /// </summary>
        public bool IsDrawerOpen { get; set; }

        private void UpdateTags()
        {
            Tags = new List<ListItemViewModel<string>>();
            Tags.Add(new ListItemViewModel<string> 
            { 
                Value = NoteFilter.SpecialTags.AllNotes,
                Text = _specialTagLocalizations[NoteFilter.SpecialTags.AllNotes],
                IconName = IconNames.TagMultiple,
            });
            Tags.Add(new ListItemViewModel<string>
            {
                Value = NoteFilter.SpecialTags.NotesWithoutTags,
                Text = _specialTagLocalizations[NoteFilter.SpecialTags.NotesWithoutTags],
                IconName = IconNames.TagOff,
            });
            Tags.Add(new ListItemViewModel<string>
            {
                IsDivider = true,
            });
            Tags.AddRange(Model.CollectActiveTags().Select(tag => new ListItemViewModel<string>() 
            {
                Text = tag,
                Value = tag,
                IconName = IconNames.TagOutline,
            }));
            OnPropertyChanged(nameof(Tags));
        }

        /// <summary>
        /// Gets the localized tag filter to show all notes.
        /// </summary>
        public string AllNotesTagFilter
        {
            get { return _specialTagLocalizations[NoteFilter.SpecialTags.AllNotes]; }
        }

        /// <summary>
        /// Gets the localized tag filter to show all notes without tags.
        /// </summary>
        public string WithoutTagFilter
        {
            get { return _specialTagLocalizations[NoteFilter.SpecialTags.NotesWithoutTags]; }
        }

        /// <summary>
        /// Gets the command which handles the synchronization with the web server.
        /// </summary>
        public ICommand SynchronizeCommand { get; private set; }

        private async void Synchronize()
        {
            if (_autoSynchronizationService.IsRunning)
                return;

            //_feedbackService.ShowBusyIndicator(true);
            try
            {
                OnStoringUnsavedData();

                _storyBoardService.SynchronizationStory = new SynchronizationStoryModel();
                var synchronizationStory = new IsCloudServiceSetStep();
                await synchronizationStory.RunStory(_storyBoardService.SynchronizationStory, Ioc.Instance, StoryMode.Gui);
                // todo:
                //_autoSynchronizationService.LastSynchronizationFingerprint = Model.GetModificationFingerprint();
            }
            finally
            {
                //_feedbackService.ShowBusyIndicator(false);
            }
        }

        /// <summary>
        /// Gets the command which closes encrypted notes.
        /// </summary>
        public ICommand CloseSafeCommand { get; private set; }

        private void CloseSafe()
        {
            foreach (SafeModel safe in Model.Safes)
                safe.Close();
            _navigationService.Reload();
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
                Modified = true;
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
                Modified = true;
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
            OnPropertyChanged("SelectedOrderNotePosition");
        }
    }
}