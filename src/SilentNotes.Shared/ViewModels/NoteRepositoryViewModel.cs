// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
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
        private static string _lastFilter;

        private readonly IStoryBoardService _storyBoardService;
        private readonly IRepositoryStorageService _repositoryService;
        private readonly IFeedbackService _feedbackService;
        private readonly ISettingsService _settingsService;
        private readonly IEnvironmentService _environmentService;
        private readonly SearchableHtmlConverter _searchableTextConverter;
        private readonly ICryptor _noteCryptor;
        private NoteRepositoryModel _model;
        private string _filter;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteRepositoryViewModel"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
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
            if (!string.IsNullOrEmpty(_lastFilter))
                Filter = _lastFilter;
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
                double defaultBaseFontSize = 15.0; // Default size for scale 1.0
                switch (_environmentService.Os)
                {
                    case Services.OperatingSystem.Windows:
                        defaultBaseFontSize = 13.0;
                        break;
                }
                SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
                double fontSize = settings != null ? defaultBaseFontSize * settings.FontScale : defaultBaseFontSize;
                return FloatingPointUtils.FormatInvariant(fontSize);
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
            get { return _filter; }

            set
            {
                if (ChangeProperty(ref _filter, value, false))
                {
                    _lastFilter = _filter;
                    OnPropertyChanged(nameof(IsFiltered));
                    ApplyFilter(_filter);
                    OnPropertyChanged("Notes");
                }
            }
        }

        [VueDataBinding(VueBindingMode.OneWayToView)]
        public bool IsFiltered
        {
            get { return !string.IsNullOrEmpty(Filter); }
        }

        private void ApplyFilter(string filter)
        {
            string normalizedFilter = SearchableHtmlConverter.NormalizeWhitespaces(filter);
            NoteFilter noteFilter = new NoteFilter(normalizedFilter);

            FilteredNotes.Clear();
            foreach (NoteViewModel noteViewModel in AllNotes)
            {
                if (!noteViewModel.InRecyclingBin && noteFilter.ContainsPattern(noteViewModel.SearchableContent))
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
                if (!string.IsNullOrEmpty(_filter))
                    navigation.Variables.AddOrReplace(ControllerParameters.SearchFilter, _filter);
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
            NoteViewModel noteViewModel = new NoteViewModel(_navigationService, Language, Icon, Theme, _webviewBaseUrl, _searchableTextConverter, _repositoryService, _feedbackService, null, _noteCryptor, _model.Safes, noteModel);
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
            OnPropertyChanged("Notes");

            _feedbackService.ShowToast(Language.LoadText("feedback_note_to_recycle"));
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
                FilteredNotes.Clear();
                AllNotes.Clear();
                Filter = null;
                _model = value;

                // Wrap models in view models
                foreach (NoteModel note in _model.Notes)
                {
                    NoteViewModel noteViewModel = new ShortenedNoteViewModel(_navigationService, Language, Icon, Theme, _webviewBaseUrl, _searchableTextConverter, _repositoryService, _feedbackService, _settingsService, _noteCryptor, _model.Safes, note);
                    AllNotes.Add(noteViewModel);
                    if (!noteViewModel.InRecyclingBin)
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
