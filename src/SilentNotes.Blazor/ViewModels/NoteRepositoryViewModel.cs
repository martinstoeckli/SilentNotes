﻿// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MudBlazor;
//using SilentNotes.Controllers;
using SilentNotes.Crypto;
//using SilentNotes.HtmlView;
using SilentNotes.Models;
using SilentNotes.Services;
//using SilentNotes.StoryBoards;
//using SilentNotes.StoryBoards.SynchronizationStory;
using SilentNotes.Workers;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the main page with the overview of the notes.
    /// </summary>
    public class NoteRepositoryViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        //private readonly IStoryBoardService _storyBoardService;
        //private readonly IRepositoryStorageService _repositoryService;
        private readonly IFeedbackService _feedbackService;
        private readonly IThemeService _themeService;
        private readonly ISettingsService _settingsService;
        private readonly IEnvironmentService _environmentService;
        //private readonly IAutoSynchronizationService _autoSynchronizationService;
        private readonly SearchableHtmlConverter _searchableTextConverter;
        private readonly ICryptor _noteCryptor;
        private readonly KeyValueList<string, string> _specialTagLocalizations;
        private NoteRepositoryModel _model;

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
            ICryptoRandomSource randomSource)
        {
            //    _storyBoardService = storyBoardService;
            //    _repositoryService = repositoryService;
            //    _feedbackService = feedbackService;
            Language = languageService;
            _navigationService = navigationService;
            _feedbackService = feedbackService;
            _themeService = themeService;
            _settingsService = settingsService;
            _environmentService = environmentService;
            //    _autoSynchronizationService = autoSynchronizationService;
            _noteCryptor = new Cryptor(NoteModel.CryptorPackageName, randomSource);
            _searchableTextConverter = new SearchableHtmlConverter();
            AllNotes = new List<NoteViewModel>();
            FilteredNotes = new ObservableCollection<NoteViewModel>();

            _specialTagLocalizations = new KeyValueList<string, string>();
            _specialTagLocalizations[NoteFilter.SpecialTags.AllNotes] = string.Format("«{0}»", Language.LoadText("filter_show_all_notes"));
            _specialTagLocalizations[NoteFilter.SpecialTags.NotesWithoutTags] = string.Format("«{0}»", Language.LoadText("filter_only_without_tags"));

            //    _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);
            Model = model;
            UpdateTags();

            //    // Initialize commands and events
            //    ShowNoteCommand = new RelayCommand<object>(ShowNote);
            //    NewNoteCommand = new RelayCommand(NewNote);
            //    NewChecklistCommand = new RelayCommand(NewChecklist);
            DeleteNoteCommand = new RelayCommand<object>(DeleteNote);
            ClearFilterCommand = new RelayCommand(ClearFilter);
            //    SynchronizeCommand = new RelayCommand(Synchronize);
            //    ShowExportCommand = new RelayCommand(ShowExport);
            CloseSafeCommand = new RelayCommand(CloseSafe);

            Modified = false;

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

        ///// <inheritdoc/>
        //public override void OnStoringUnsavedData()
        //{
        //    // If there was an error reading the existing repository, we do not overwrite it, to
        //    // prevent further damage.
        //    if (Model == null)
        //        return;

        //    if (Modified)
        //    {
        //        _repositoryService.TrySaveRepository(Model);
        //        Modified = false;
        //    }
        //}

        ///// <inheritdoc/>
        //public override void OnGoBackPressed(out bool handled)
        //{
        //    handled = false;
        //}

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
        private List<NoteViewModel> AllNotes { get; set; }

        /// <summary>
        /// Gets a bindable list of the visible notes, which are not filtered out.
        /// </summary>
        public ObservableCollection<NoteViewModel> FilteredNotes { get; private set; }

        /// <summary>
        /// Gets or sets the search filter.
        /// A two-way-binding can end up in an endless loop, when typing very fast.
        /// </summary>
        //[VueDataBinding(VueBindingMode.OneWayToViewmodel)]
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

        //[VueDataBinding(VueBindingMode.OneWayToView)]
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
            foreach (NoteViewModel noteViewModel in AllNotes)
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
        //[VueDataBinding(VueBindingMode.Command)]
        public ICommand ClearFilterCommand { get; private set; }

        private void ClearFilter()
        {
            Filter = null;
            OnPropertyChanged("ClearFilter");
        }

        ///// <summary>
        ///// Gets the command which handles the click event on a note.
        ///// </summary>
        //[VueDataBinding(VueBindingMode.Command)]
        //public ICommand ShowNoteCommand { get; private set; }

        //private void ShowNote(object value)
        //{
        //    Guid noteId = (value is Guid) ? (Guid)value : new Guid(value.ToString());
        //    NoteViewModel note = AllNotes.FirstOrDefault(item => item.Id == noteId);
        //    if (note != null)
        //    {
        //        Navigation navigation = null;
        //        switch (note.Model.NoteType)
        //        {
        //            case NoteType.Text:
        //                navigation = new Navigation(ControllerNames.Note, ControllerParameters.NoteId, noteId.ToString());
        //                break;

        //            case NoteType.Checklist:
        //                navigation = new Navigation(ControllerNames.Checklist, ControllerParameters.NoteId, noteId.ToString());
        //                break;

        //            default:
        //                throw new ArgumentOutOfRangeException(nameof(NoteType));
        //        }

        //        SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
        //        if (!string.IsNullOrEmpty(settings.Filter))
        //            navigation.Variables.AddOrReplace(ControllerParameters.SearchFilter, settings.Filter);
        //        _navigationService.Navigate(navigation);
        //    }
        //}

        ///// <summary>
        ///// Gets the command which handles the creation of a new note.
        ///// </summary>
        //[VueDataBinding(VueBindingMode.Command)]
        //public ICommand NewNoteCommand { get; private set; }

        //private void NewNote()
        //{
        //    NewNote(NoteType.Text);
        //}

        //private void NewNote(NoteType noteType)
        //{
        //    Modified = true;
        //    ClearFilter();

        //    // Create new note and update model list
        //    NoteModel noteModel = new NoteModel();
        //    noteModel.NoteType = noteType;
        //    noteModel.BackgroundColorHex = _settingsService.LoadSettingsOrDefault().DefaultNoteColorHex;

        //    // Update view model list
        //    NoteViewModel noteViewModel = new NoteViewModel(_navigationService, Language, Icon, Theme, _webviewBaseUrl, _searchableTextConverter, _repositoryService, _feedbackService, null, _environmentService, _noteCryptor, _model.Safes, _model.CollectActiveTags(), noteModel);
        //    NoteInsertionMode insertionMode = _settingsService.LoadSettingsOrDefault().DefaultNoteInsertion;
        //    switch (insertionMode)
        //    {
        //        case NoteInsertionMode.AtTop:
        //            var lastPinned = AllNotes.LastOrDefault(x => x.IsPinned == true);
        //            var index = lastPinned == null ? 0 : AllNotes.IndexOf(lastPinned) + 1;
        //            _model.Notes.Insert(index, noteModel);
        //            AllNotes.Insert(index, noteViewModel);
        //            break;

        //        case NoteInsertionMode.AtBottom:
        //            _model.Notes.Add(noteModel);
        //            AllNotes.Add(noteViewModel);
        //            break;

        //        default:
        //            throw new ArgumentOutOfRangeException(nameof(insertionMode));
        //    }

        //    ShowNoteCommand.Execute(noteViewModel.Id);
        //}

        ///// <summary>
        ///// Gets the command which handles the creation of a new checklist note.
        ///// </summary>
        //[VueDataBinding(VueBindingMode.Command)]
        //public ICommand NewChecklistCommand { get; private set; }

        //private void NewChecklist()
        //{
        //    NewNote(NoteType.Checklist);
        //}

        /// <summary>
        /// Gets the command which handles the moving of a note to the recyclebin.
        /// </summary>
        public ICommand DeleteNoteCommand { get; private set; }

        ///// <summary>
        ///// Changes <see cref="NoteViewModel.IsPinned"/> based on position.
        ///// This is usually called after a drag and drop action.
        ///// </summary>
        ///// <remarks>Changes to true if placed in front of a
        ///// pinned note. False if placed behind unpinned one.</remarks>
        //internal void CheckPinStatusAtPosition(int currentIndex)
        //{
        //    var movedNote = FilteredNotes[currentIndex];
        //    var noteBehind = FilteredNotes.ElementAtOrDefault(currentIndex + 1);
        //    var noteInfront = FilteredNotes.ElementAtOrDefault(currentIndex - 1);

        //    if (movedNote.IsPinned == false && noteBehind != null && noteBehind.IsPinned)
        //    {
        //        movedNote.IsPinned = true;
        //        OnPropertyChanged("Notes"); // refreshes the notes
        //    }
        //    else if (movedNote.IsPinned && noteInfront != null && noteInfront.IsPinned == false)
        //    {
        //        movedNote.IsPinned = false;
        //        OnPropertyChanged("Notes");
        //    }
        //}

        private void DeleteNote(object value)
        {
            Guid noteId = (value is Guid) ? (Guid)value : new Guid(value.ToString());
            NoteViewModel selectedNote = AllNotes.Find(item => item.Id == noteId);
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
                if (SetPropertyAndModified(settings.SelectedTag, newValue, (string v) => settings.SelectedTag = v))
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

        ///// <summary>
        ///// Gets the command which handles the synchronization with the web server.
        ///// </summary>
        //[VueDataBinding(VueBindingMode.Command)]
        //public ICommand SynchronizeCommand { get; private set; }

        //private async void Synchronize()
        //{
        //    if (_autoSynchronizationService.IsRunning)
        //        return;

        //    _feedbackService.ShowBusyIndicator(true);
        //    try
        //    {
        //        OnStoringUnsavedData();
        //        _storyBoardService.ActiveStory = new SynchronizationStoryBoard(StoryBoardMode.Gui);
        //        await _storyBoardService.ActiveStory.Start();
        //        _autoSynchronizationService.LastSynchronizationFingerprint = Model.GetModificationFingerprint();
        //    }
        //    catch (Exception)
        //    {
        //        _storyBoardService.ActiveStory = null;
        //        throw;
        //    }
        //    finally
        //    {
        //        _feedbackService.ShowBusyIndicator(false);
        //    }
        //}

        ///// <summary>
        ///// Gets the command which opens export dialog.
        ///// </summary>
        //[VueDataBinding(VueBindingMode.Command)]
        //public ICommand ShowExportCommand { get; private set; }

        //private void ShowExport()
        //{
        //    _navigationService.Navigate(new Navigation(ControllerNames.Export));
        //}

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

        //public void AddNoteToSafe(Guid noteId)
        //{
        //    NoteViewModel note = AllNotes.Find(item => item.Id == noteId);
        //    SafeModel oldestOpenSafe = Model.Safes.FindOldestOpenSafe();
        //    if ((note != null) && (oldestOpenSafe != null))
        //    {
        //        note.Model.SafeId = oldestOpenSafe.Id;
        //        note.Model.HtmlContent = note.Lock(note.UnlockedHtmlContent);
        //        note.Model.RefreshModifiedAt();
        //        Modified = true;
        //    }
        //}

        //public void RemoveNoteFromSafe(Guid noteId)
        //{
        //    NoteViewModel note = AllNotes.Find(item => item.Id == noteId);
        //    if (note != null)
        //    {
        //        note.Model.SafeId = null;
        //        note.Model.HtmlContent = note.UnlockedHtmlContent;
        //        note.Model.RefreshModifiedAt();
        //        Modified = true;
        //    }
        //}

        ///// <summary>
        ///// Command to move a note to a new place in the list. This is usually
        ///// called after a drag and drop action.
        ///// </summary>
        ///// <param name="oldIndex">Index of the note to move.</param>
        ///// <param name="newIndex">New index to place the note in.</param>
        //public void MoveNote(int oldIndex, int newIndex)
        //{
        //    if ((oldIndex == newIndex)
        //        || !IsInRange(oldIndex, 0, FilteredNotes.Count - 1)
        //        || !IsInRange(newIndex, 0, FilteredNotes.Count - 1))
        //        return;
        //    Modified = true;

        //    int oldIndexInUnfilteredList = AllNotes.IndexOf(FilteredNotes[oldIndex]);
        //    int newIndexInUnfilteredList = AllNotes.IndexOf(FilteredNotes[newIndex]);

        //    ListMove(_model.Notes, oldIndexInUnfilteredList, newIndexInUnfilteredList);
        //    ListMove(AllNotes, oldIndexInUnfilteredList, newIndexInUnfilteredList);
        //    FilteredNotes.Move(oldIndex, newIndex);
        //    _model.RefreshOrderModifiedAt();
        //}

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
                    NoteViewModel noteViewModel = new NoteViewModel(
                        note,
                        _searchableTextConverter,
                        Language,
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

        //private bool IsInRange(int candidate, int min, int max)
        //{
        //    return (candidate >= min) && (candidate <= max);
        //}

        //private void ListMove<T>(List<T> list, int oldIndex, int newIndex)
        //{
        //    T item = list[oldIndex];
        //    list.RemoveAt(oldIndex);
        //    list.Insert(newIndex, item);
        //}
    }
}