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
        private readonly IStoryBoardService _storyBoardService;
        private readonly IRepositoryStorageService _repositoryService;
        private readonly IFeedbackService _feedbackService;
        private readonly ISettingsService _settingsService;
        private readonly IThemeService _themeService;
        private readonly IEnvironmentService _environmentService;
        private readonly SearchableHtmlConverter _searchableTextConverter;
        private readonly ICryptor _noteCryptor;
        private NoteRepositoryModel _model;
        private NoteViewModel _selectedNote;
        private string _filter;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteRepositoryViewModel"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public NoteRepositoryViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IBaseUrlService webviewBaseUrl,
            IStoryBoardService storyBoardService,
            IFeedbackService feedbackService,
            ISettingsService settingsService,
            IThemeService themeService,
            IEnvironmentService environmentService,
            ICryptoRandomSource randomSource,
            IRepositoryStorageService repositoryService)
            : base(navigationService, languageService, svgIconService, webviewBaseUrl)
        {
            _storyBoardService = storyBoardService;
            _repositoryService = repositoryService;
            _feedbackService = feedbackService;
            _settingsService = settingsService;
            _themeService = themeService;
            _environmentService = environmentService;
            _noteCryptor = new Cryptor(NoteModel.CryptorPackageName, randomSource);
            _searchableTextConverter = new SearchableHtmlConverter();
            AllNotes = new List<NoteViewModel>();
            FilteredNotes = new ObservableCollection<NoteViewModel>();

            _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);
            Model = noteRepository;

            // Initialize commands and events
            ShowNoteCommand = new RelayCommand<Guid?>(ShowNote);
            AddNoteCommand = new RelayCommand(AddNote);
            DeleteNoteCommand = new RelayCommand(DeleteNote);
            ClearFilterCommand = new RelayCommand(ClearFilter);
            SynchronizeCommand = new RelayCommand(Synchronize);
            ShowTransferCodeCommand = new RelayCommand(ShowTransferCode);
            ShowSettingsCommand = new RelayCommand(ShowSettings);
            ShowRecycleBinCommand = new RelayCommand(ShowRecycleBin);
            ShowInfoCommand = new RelayCommand(ShowInfo);
            OpenSafeCommand = new RelayCommand(OpenSafe);
            CloseSafeCommand = new RelayCommand(CloseSafe);
            ChangeSafePasswordCommand = new RelayCommand(ChangeSafePassword);

            OnPropertyChanged(nameof(FilterButtonMagnifierVisible));
            OnPropertyChanged(nameof(FilterButtonCancelVisible));
            Modified = false;
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
        /// Gets the active theme.
        /// </summary>
        public ThemeModel Theme
        {
            get { return _themeService.SelectedTheme; }
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
        /// Gets or sets the selected Note.
        /// </summary>
        public NoteViewModel SelectedNote
        {
            get { return _selectedNote; }
            set { ChangeProperty(ref _selectedNote, value, false); }
        }

        /// <summary>
        /// Gets or sets the search filter.
        /// </summary>
        public string Filter
        {
            get { return _filter; }

            set
            {
                if (ChangeProperty(ref _filter, value, false))
                {
                    OnPropertyChanged(nameof(FilterButtonMagnifierVisible));
                    OnPropertyChanged(nameof(FilterButtonCancelVisible));
                    ApplyFilter(_filter);
                    OnPropertyChanged("Notes");
                }
            }
        }

        private void ApplyFilter(string filter)
        {
            string normalizedFilter = SearchableHtmlConverter.NormalizeWhitespaces(filter);
            NoteFilter noteFilter = new NoteFilter(normalizedFilter);
            NoteViewModel selectedNote = SelectedNote;

            FilteredNotes.Clear();
            foreach (NoteViewModel noteViewModel in AllNotes)
            {
                if (!noteViewModel.InRecyclingBin && noteFilter.ContainsPattern(noteViewModel.SearchableContent))
                    FilteredNotes.Add(noteViewModel);
            }

            if (FilteredNotes.Contains(selectedNote))
                SelectedNote = selectedNote;
        }

        /// <summary>
        /// Gets a value indicating whether the magnifier button in the filter box is visible.
        /// </summary>
        public bool FilterButtonMagnifierVisible
        {
            get { return string.IsNullOrEmpty(Filter); }
        }

        /// <summary>
        /// Gets a value indicating whether the cancel button in the filter box is visible.
        /// </summary>
        public bool FilterButtonCancelVisible
        {
            get { return !string.IsNullOrEmpty(Filter); }
        }

        /// <summary>
        /// Gets the command which clears the search filter.
        /// </summary>
        public ICommand ClearFilterCommand { get; private set; }

        private void ClearFilter()
        {
            Filter = null;
        }

        /// <summary>
        /// Gets the command which handles the click event on a note.
        /// </summary>
        public ICommand ShowNoteCommand { get; private set; }

        private void ShowNote(Guid? noteId)
        {
            NoteViewModel note;
            if (noteId != null)
                note = FilteredNotes.FirstOrDefault(item => item.Id == noteId);
            else
                note = SelectedNote;

            if (note != null)
            {
                _navigationService.Navigate(ControllerNames.Note, ControllerParameters.NoteId, note.Id.ToString());
            }
        }

        /// <summary>
        /// Gets the command which handles the creation of a new note.
        /// </summary>
        public ICommand AddNoteCommand { get; private set; }

        private void AddNote()
        {
            Modified = true;
            ClearFilter();

            // Create new note and update model list
            NoteModel noteModel = new NoteModel();
            noteModel.BackgroundColorHex = _settingsService.LoadSettingsOrDefault().DefaultNoteColorHex;
            _model.Notes.Insert(0, noteModel);

            // Update view model list
            NoteViewModel noteViewModel = new NoteViewModel(_navigationService, Language, Icon, _webviewBaseUrl, _searchableTextConverter, _repositoryService, _feedbackService, null, _noteCryptor, _model.Safes, noteModel);
            AllNotes.Insert(0, noteViewModel);
            FilteredNotes.Insert(0, noteViewModel);

            SelectedNote = noteViewModel;
            ShowNote(noteViewModel.Id);
        }

        /// <summary>
        /// Gets the command which handles the moving of a note to the recyclebin.
        /// </summary>
        public ICommand DeleteNoteCommand { get; private set; }

        private void DeleteNote()
        {
            if (SelectedNote == null)
                return;
            Modified = true;

            // Mark note as deleted
            SelectedNote.InRecyclingBin = true;

            // Remove note from filtered list
            int selectedIndex = FilteredNotes.IndexOf(SelectedNote);
            FilteredNotes.RemoveAt(selectedIndex);
            OnPropertyChanged("Notes");

            // Set new selection
            selectedIndex = Math.Min(selectedIndex, FilteredNotes.Count - 1);
            if (selectedIndex >= 0)
                SelectedNote = FilteredNotes[selectedIndex];
            else
                SelectedNote = null;
        }

        /// <summary>
        /// Gets the command which handles the synchronization with the web server.
        /// </summary>
        public ICommand SynchronizeCommand { get; private set; }

        private async void Synchronize()
        {
            _feedbackService.ShowBusyIndicator(true);
            try
            {
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
        public ICommand ShowRecycleBinCommand { get; private set; }

        private void ShowRecycleBin()
        {
            _navigationService.Navigate(ControllerNames.RecycleBin);
        }

        /// <summary>
        /// Gets the command which opens the settings dialog.
        /// </summary>
        public ICommand ShowSettingsCommand { get; private set; }

        private void ShowSettings()
        {
            _navigationService.Navigate(ControllerNames.Settings);
        }

        /// <summary>
        /// Gets the command which shows the current transfer code.
        /// </summary>
        public ICommand ShowTransferCodeCommand { get; private set; }

        private void ShowTransferCode()
        {
            _navigationService.Navigate(ControllerNames.TransferCodeHistory);
        }

        /// <summary>
        /// Gets the command which shows the info dialog.
        /// </summary>
        public ICommand ShowInfoCommand { get; private set; }

        private void ShowInfo()
        {
            _navigationService.Navigate(ControllerNames.Info);
        }

        /// <summary>
        /// Gets the command which opens encrypted notes.
        /// </summary>
        public ICommand OpenSafeCommand { get; private set; }

        private void OpenSafe()
        {
            _navigationService.Navigate(ControllerNames.OpenSafe);
        }

        /// <summary>
        /// Gets the command which closes encrypted notes.
        /// </summary>
        public ICommand CloseSafeCommand { get; private set; }

        private void CloseSafe()
        {
            foreach (SafeModel safe in Model.Safes)
                safe.Close();
            _navigationService.Navigate(ControllerNames.NoteRepository);
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
        public ICommand ChangeSafePasswordCommand { get; private set; }

        private void ChangeSafePassword()
        {
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
                SelectedNote = null;
                Filter = null;
                _model = value;

                // Wrap models in view models
                foreach (NoteModel note in _model.Notes)
                {
                    NoteViewModel noteViewModel = new NoteViewModel(_navigationService, Language, Icon, _webviewBaseUrl, _searchableTextConverter, _repositoryService, _feedbackService, _settingsService, _noteCryptor, _model.Safes, note);
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
