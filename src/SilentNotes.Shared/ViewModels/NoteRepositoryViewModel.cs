// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using SilentNotes.Controllers;
using SilentNotes.Crypto;
using SilentNotes.HtmlView;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.StoryBoards;
using SilentNotes.StoryBoards.SynchronizationStory;
using SilentNotes.Workers;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the main page with the overview of the notes.
    /// </summary>
    public class NoteRepositoryViewModel : ViewModelBase
    {
        private const string EmptyTagFilter = "...";
        private readonly IStoryBoardService _storyBoardService;
        private readonly IRepositoryStorageService _repositoryService;
        private readonly IFeedbackService _feedbackService;
        private readonly ISettingsService _settingsService;
        private readonly IEnvironmentService _environmentService;
        private readonly SearchableHtmlConverter _searchableTextConverter;
        private readonly ICryptor _noteCryptor;
        private NoteRepositoryModel _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteRepositoryViewModel"/> class.
        /// </summary>
        public NoteRepositoryViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IThemeService themeService,
            IBaseUrlService webviewBaseUrl,
            IStoryBoardService storyBoardService,
            IFeedbackService feedbackService,
            ISettingsService settingsService,
            IEnvironmentService environmentService,
            ICryptoRandomSource randomSource,
            IRepositoryStorageService repositoryService)
            : base(navigationService, languageService, svgIconService, themeService, webviewBaseUrl)
        {
            _storyBoardService = storyBoardService;
            _repositoryService = repositoryService;
            _feedbackService = feedbackService;
            _settingsService = settingsService;
            _environmentService = environmentService;
            _noteCryptor = new Cryptor(NoteModel.CryptorPackageName, randomSource);
            _searchableTextConverter = new SearchableHtmlConverter();
            AllNotes = new List<NoteViewModel>();
            FilteredNotes = new ObservableCollection<NoteViewModel>();

            _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);
            Model = noteRepository;

            Tags = Model.CollectActiveTags();
            Tags.Insert(0, EmptyTagFilter);

            // Initialize commands and events
            ShowNoteCommand = new RelayCommand<object>(ShowNote);
            NewNoteCommand = new RelayCommand(NewNote);
            NewChecklistCommand = new RelayCommand(NewChecklist);
            DeleteNoteCommand = new RelayCommand<object>(DeleteNote);
            ClearFilterCommand = new RelayCommand(ClearFilter);
            SynchronizeCommand = new RelayCommand(Synchronize);
            ShowTransferCodeCommand = new RelayCommand(ShowTransferCode);
            ShowSettingsCommand = new RelayCommand(ShowSettings);
            ShowRecycleBinCommand = new RelayCommand(ShowRecycleBin);
            ShowExportCommand = new RelayCommand(ShowExport);
            ShowInfoCommand = new RelayCommand(ShowInfo);
            OpenSafeCommand = new RelayCommand(OpenSafe);
            CloseSafeCommand = new RelayCommand(CloseSafe);
            ChangeSafePasswordCommand = new RelayCommand(ChangeSafePassword);

            Modified = false;

            // If a filter was set before e.g. opening a note, set the same filter again.
            SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
            if (!SelectedTagExistsInTags())
                settings.SelectedTag = null;

            //TODO: refactor
            var tmpVariables = _navigationService.CurrentNavigation.Variables;
            if (tmpVariables.Exists(x => x.Key == "noteid"))
            {
                /*
                 * If the variable exists, it means that we navigated from a NoteViewModel
                 * and the pin property might have changed.
                 * Could be also done by iterating throught the whole list and check for the property.
                 * TODO: Consider PropertyChanged event.
                */
                var changedNote = AllNotes
                    .Where(x => x.Id.ToString().Equals(tmpVariables["noteid"]))
                    .FirstOrDefault();

                CheckChangedNoteForPin(changedNote);
            }

            if (!string.IsNullOrEmpty(settings.SelectedTag) || !string.IsNullOrEmpty(settings.Filter))
            {
                OnPropertyChanged(nameof(SelectedTag));
                OnPropertyChanged(nameof(Filter));
                OnPropertyChanged(nameof(IsFiltered));
                ApplyFilter();
                OnPropertyChanged("Notes");
            }
        }

        /// <summary>
        /// Checks whether the <see cref="NoteViewModel.PinnedChanged"/> changed
        /// and moves the note based on <see cref="NoteViewModel.IsPinned"/>.
        /// </summary>
        /// <param name="noteId"></param>
        private void CheckChangedNoteForPin(NoteViewModel changedNote)
        {
            if (changedNote.PinnedChanged)
            {
                if (changedNote.IsPinned)
                {//since the note got pinned, move it to the top
                    MoveNote(FilteredNotes.IndexOf(changedNote), 0);
                    //TODO: highlight note as pinned (border/icon/whatever)
                }
                else
                {//the note got unpinned, move it to the end of pinned notes
                    int firstUnpinnedNoteIndex = FilteredNotes.IndexOf(
                        FilteredNotes.First(x => x.IsPinned == false)
                    );

                    MoveNote(FilteredNotes.IndexOf(changedNote), firstUnpinnedNoteIndex);
                    //TODO: change highlight to normal
                }
                changedNote.PinnedChanged = false; //handled
            }
        }

        private static void ClearSelectedTagIfNotContainedInNotes(SettingsModel settings, List<string> tags)
        {
            // An invalid selected tag can exist if the user edited a note (deleted a tag) and
            // returned to the overview, which still remembers this selected tag.
            NoteFilter noteFilter = new NoteFilter(null, SelectedTag);
            return noteFilter.ContainsTag(Tags);
        }

        /// <inheritdoc/>
        public override void OnStoringUnsavedData()
        {
            // If there was an error reading the existing repository, we do not overwrite it, to
            // prevent further damage.
            if (Model == null)
                return;

            if (Modified)
            {
                _repositoryService.TrySaveRepository(Model);
                Modified = false;
            }
        }

        /// <inheritdoc/>
        public override void OnGoBackPressed(out bool handled)
        {
            handled = false;
        }

        /// <summary>
        /// Gets the base font size [px] of the notes, from which the relative sizes are derrived.
        /// </summary>
        public string NoteBaseFontSize
        {
            get
            {
                double defaultBaseFontSize = SettingsViewModel.ReferenceFontSize;
                switch (_environmentService.Os)
                {
                    case Services.OperatingSystem.Windows:
                        defaultBaseFontSize = SettingsViewModel.ReferenceFontSize - 2;
                        break;
                }
                SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
                SliderStepConverter converter = new SliderStepConverter(defaultBaseFontSize, 1.0);
                double fontSize = settings != null
                    ? converter.ModelFactorToValue(settings.FontScale)
                    : defaultBaseFontSize;
                return FloatingPointUtils.FormatInvariant(fontSize);
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
        private List<NoteViewModel> AllNotes { get; set; }

        /// <summary>
        /// Gets a bindable list of the visible notes, which are not filtered out.
        /// </summary>
        public ObservableCollection<NoteViewModel> FilteredNotes { get; private set; }

        /// <summary>
        /// Gets or sets the search filter.
        /// A two-way-binding can end up in an endless loop, when typing very fast.
        /// </summary>
        [VueDataBinding(VueBindingMode.OneWayToViewmodel)]
        public string Filter
        {
            get { return _settingsService?.LoadSettingsOrDefault().Filter; }

            set
            {
                SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
                if (ChangePropertyIndirect(() => settings.Filter, (string v) => settings.Filter = v, value, false))
                {
                    OnPropertyChanged(nameof(Filter));
                    OnPropertyChanged(nameof(IsFiltered));
                    ApplyFilter();
                    OnPropertyChanged("Notes");
                }
            }
        }

        [VueDataBinding(VueBindingMode.OneWayToView)]
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
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand ClearFilterCommand { get; private set; }

        private void ClearFilter()
        {
            Filter = null;
            OnPropertyChanged("ClearFilter");
        }

        /// <summary>
        /// Gets the command which handles the click event on a note.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand ShowNoteCommand { get; private set; }

        private void ShowNote(object value)
        {
            Guid noteId = (value is Guid) ? (Guid)value : new Guid(value.ToString());
            NoteViewModel note = FilteredNotes.FirstOrDefault(item => item.Id == noteId);
            if (note != null)
            {
                Navigation navigation = null;
                switch (note.Model.NoteType)
                {
                    case NoteType.Text:
                        navigation = new Navigation(ControllerNames.Note, ControllerParameters.NoteId, noteId.ToString());
                        break;

                    case NoteType.Checklist:
                        navigation = new Navigation(ControllerNames.Checklist, ControllerParameters.NoteId, noteId.ToString());
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(NoteType));
                }

                SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
                if (!string.IsNullOrEmpty(settings.Filter))
                    navigation.Variables.AddOrReplace(ControllerParameters.SearchFilter, settings.Filter);
                _navigationService.Navigate(navigation);
            }
        }

        /// <summary>
        /// Gets the command which handles the creation of a new note.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
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
            NoteViewModel noteViewModel = new NoteViewModel(_navigationService, Language, Icon, Theme, _webviewBaseUrl, _searchableTextConverter, _repositoryService, _feedbackService, null, _noteCryptor, _model.Safes, _model.CollectActiveTags(), noteModel); ;
            NoteInsertionMode insertionMode = _settingsService.LoadSettingsOrDefault().DefaultNoteInsertion;
            switch (insertionMode)
            {
                case NoteInsertionMode.AtTop:
                    _model.Notes.Insert(0, noteModel);
                    AllNotes.Insert(0, noteViewModel);
                    FilteredNotes.Insert(0, noteViewModel);
                    break;

                case NoteInsertionMode.AtBottom:
                    _model.Notes.Add(noteModel);
                    AllNotes.Add(noteViewModel);
                    FilteredNotes.Add(noteViewModel);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(insertionMode));
            }

            ShowNoteCommand.Execute(noteViewModel.Id);
        }

        /// <summary>
        /// Gets the command which handles the creation of a new checklist note.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand NewChecklistCommand { get; private set; }

        private void NewChecklist()
        {
            NewNote(NoteType.Checklist);
        }

        /// <summary>
        /// Gets the command which handles the moving of a note to the recyclebin.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand DeleteNoteCommand { get; private set; }

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

            Tags = Model.CollectActiveTags();
            Tags.Insert(0, EmptyTagFilter);
            OnPropertyChanged("Tags");

            // If the note was the last one containing the selected tag, the filter should be cleared
            if (!SelectedTagExistsInTags())
                SelectedTag = null;
            else
                OnPropertyChanged("Notes");

            _feedbackService.ShowToast(Language.LoadText("feedback_note_to_recycle"));
        }

        /// <summary>
        /// Gets a list of all tags which are used in the notes, plus the <see cref="EmptyTagFilter"/>.
        /// </summary>
        [VueDataBinding(VueBindingMode.OneWayToView)]
        public List<string> Tags { get; private set; }

        /// <summary>
        /// Gets or sets the selected tag string, or the <see cref="EmptyTagFilter"/> in case that no
        /// tag is selected.
        /// </summary>
        [VueDataBinding(VueBindingMode.TwoWay)]
        public string SelectedTag
        {
            get
            {
                SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
                return string.IsNullOrEmpty(settings.SelectedTag) ? EmptyTagFilter : settings.SelectedTag;
            }

            set
            {
                string newValue = (value == EmptyTagFilter ? null : value);
                SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
                if (ChangePropertyIndirect(() => settings.SelectedTag, (string v) => settings.SelectedTag = v, newValue, true))
                {
                    OnPropertyChanged(nameof(SelectedTag));
                    ApplyFilter();
                    OnPropertyChanged("Notes");
                    _settingsService.TrySaveSettingsToLocalDevice(settings);
                }
            }
        }

        /// <summary>
        /// Gets the command which handles the synchronization with the web server.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand SynchronizeCommand { get; private set; }

        private async void Synchronize()
        {
            _feedbackService.ShowBusyIndicator(true);
            try
            {
                OnStoringUnsavedData();
                _storyBoardService.ActiveStory = new SynchronizationStoryBoard(StoryBoardMode.GuiAndToasts);
                await _storyBoardService.ActiveStory.Start();
            }
            catch (Exception)
            {
                _storyBoardService.ActiveStory = null;
                throw;
            }
            finally
            {
                _feedbackService.ShowBusyIndicator(false);
            }
        }

        /// <summary>
        /// Gets the command which opens the recycling bin dialog.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand ShowRecycleBinCommand { get; private set; }

        private void ShowRecycleBin()
        {
            _navigationService.Navigate(new Navigation(ControllerNames.RecycleBin));
        }

        /// <summary>
        /// Gets the command which opens export dialog.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand ShowExportCommand { get; private set; }

        private void ShowExport()
        {
            _navigationService.Navigate(new Navigation(ControllerNames.Export));
        }

        /// <summary>
        /// Gets the command which opens the settings dialog.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand ShowSettingsCommand { get; private set; }

        private void ShowSettings()
        {
            _navigationService.Navigate(new Navigation(ControllerNames.Settings));
        }

        /// <summary>
        /// Gets the command which shows the current transfer code.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand ShowTransferCodeCommand { get; private set; }

        private void ShowTransferCode()
        {
            _navigationService.Navigate(new Navigation(ControllerNames.TransferCodeHistory));
        }

        /// <summary>
        /// Gets the command which shows the info dialog.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand ShowInfoCommand { get; private set; }

        private void ShowInfo()
        {
            _navigationService.Navigate(new Navigation(ControllerNames.Info));
        }

        /// <summary>
        /// Gets the command which opens encrypted notes.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand OpenSafeCommand { get; private set; }

        private void OpenSafe()
        {
            _navigationService.Navigate(new Navigation(ControllerNames.OpenSafe));
        }

        /// <summary>
        /// Gets the command which closes encrypted notes.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand CloseSafeCommand { get; private set; }

        private void CloseSafe()
        {
            foreach (SafeModel safe in Model.Safes)
                safe.Close();
            _navigationService.Navigate(new Navigation(ControllerNames.NoteRepository));
        }

        /// <summary>
        /// Gets a value indicating whether at least one safe is open.
        /// </summary>
        public bool IsAnySafeOpen
        {
            get { return Model.Safes.Any(safe => safe.IsOpen); }
        }

        /// <summary>
        /// Gets the command which allows to change the password to encrypt the notes.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand ChangeSafePasswordCommand { get; private set; }

        private void ChangeSafePassword()
        {
            if (Model.Safes.Count > 0)
                _navigationService.Navigate(new Navigation(ControllerNames.ChangePassword));
            else
                // First create a safe before the password can be changed.
                _navigationService.Navigate(new Navigation(ControllerNames.OpenSafe));
        }

        public void AddNoteToSafe(Guid noteId)
        {
            NoteViewModel note = AllNotes.Find(item => item.Id == noteId);
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
            NoteViewModel note = AllNotes.Find(item => item.Id == noteId);
            if (note != null)
            {
                note.Model.SafeId = null;
                note.Model.HtmlContent = note.UnlockedHtmlContent;
                note.Model.RefreshModifiedAt();
                Modified = true;
            }
        }

        /// <summary>
        /// Command to move a note to a new place in the list. This is usually
        /// called after a drag and drop action.
        /// </summary>
        /// <param name="oldIndex">Index of the note to move.</param>
        /// <param name="newIndex">New index to place the note in.</param>
        public void MoveNote(int oldIndex, int newIndex)
        {
            if ((oldIndex == newIndex)
                || !IsInRange(oldIndex, 0, FilteredNotes.Count - 1)
                || !IsInRange(newIndex, 0, FilteredNotes.Count - 1))
                return;
            Modified = true;

            /*
             * Check for moving a normal note before a pinned one.
             * If the user tries that, the normal note gets appended behind the pinned notes,
             * by setting the newIndex as the index of the first unpinned note.
             *
             * Apparently this doen't affect UI, so the note is displayed in the incorrect order
             * until reload.
             */

            if (FilteredNotes[oldIndex].IsPinned == false && FilteredNotes[newIndex].IsPinned)
            {
                newIndex = FilteredNotes.IndexOf(
                    FilteredNotes.First(x => x.IsPinned == false)
                );
            }

            int oldIndexInUnfilteredList = AllNotes.IndexOf(FilteredNotes[oldIndex]);
            int newIndexInUnfilteredList = AllNotes.IndexOf(FilteredNotes[newIndex]);

            ListMove(_model.Notes, oldIndexInUnfilteredList, newIndexInUnfilteredList);
            ListMove(AllNotes, oldIndexInUnfilteredList, newIndexInUnfilteredList);
            FilteredNotes.Move(oldIndex, newIndex);
            _model.RefreshOrderModifiedAt();
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
                    NoteViewModel noteViewModel = new NoteViewModel(_navigationService, Language, Icon, Theme, _webviewBaseUrl, _searchableTextConverter, _repositoryService, _feedbackService, _settingsService, _noteCryptor, _model.Safes, allTags, note);
                    AllNotes.Add(noteViewModel);

                    bool hideNote =
                        noteViewModel.InRecyclingBin ||
                        (settings.HideClosedSafeNotes && noteViewModel.IsLocked);

                    if (!hideNote)
                        FilteredNotes.Add(noteViewModel);
                }
            }
        }

        private bool IsInRange(int candidate, int min, int max)
        {
            return (candidate >= min) && (candidate <= max);
        }

        private void ListMove<T>(List<T> list, int oldIndex, int newIndex)
        {
            T item = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, item);
        }
    }
}