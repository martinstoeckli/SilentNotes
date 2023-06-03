// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
//using SilentNotes.Controllers;
using SilentNotes.Crypto;
//using SilentNotes.HtmlView;
using SilentNotes.Models;
using SilentNotes.Services;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the recycle bin to the user.
    /// </summary>
    public class RecycleBinViewModel : ViewModelBase
    {
        private readonly IRepositoryStorageService _repositoryService;
        private readonly IFeedbackService _feedbackService;
        private readonly IThemeService _themeService;
        private readonly ISettingsService _settingsService;
        //private readonly ICryptor _noteCryptor;
        private NoteRepositoryModel _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecycleBinViewModel"/> class.
        /// </summary>
        public RecycleBinViewModel(
            NoteRepositoryModel model,
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IThemeService themeService,
            //IBaseUrlService webviewBaseUrl,
            IFeedbackService feedbackService,
            ISettingsService settingsService,
            //ICryptoRandomSource randomSource,
            IRepositoryStorageService repositoryService)
            //: base(navigationService, languageService, svgIconService, themeService, webviewBaseUrl)
        {
            _feedbackService = feedbackService;
            Language = languageService;
            _themeService = themeService;
            _settingsService = settingsService;
            _repositoryService = repositoryService;
            //_noteCryptor = new Cryptor(NoteModel.CryptorPackageName, randomSource);
            RecycledNotes = new List<NoteViewModel>();

            //_repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);
            Model = model;

            // Initialize commands
            //GoBackCommand = new RelayCommand(GoBack);
            RestoreNoteCommand = new RelayCommand<object>(RestoreNote);
            DeleteNotePermanentlyCommand = new RelayCommand<object>(DeleteNotePermanently);
            EmptyRecycleBinCommand = new RelayCommand(EmptyRecycleBin);
        }

        private ILanguageService Language { get; }

        /// <summary>
        /// Gets a bindable list of the recycled notes.
        /// </summary>
        public List<NoteViewModel> RecycledNotes { get; private set; }

        /// <summary>
        /// Gets or sets the wrapped model.
        /// </summary>
        internal NoteRepositoryModel Model
        {
            get { return _model; }

            set
            {
                RecycledNotes.Clear();
                _model = value;

                // Wrap models in view models
                //IFeedbackService feedbackService = new DummyFeedbackService();
                List<string> tags = _model.CollectActiveTags();
                foreach (NoteModel note in _model.Notes)
                {
                    if (note.InRecyclingBin)
                        RecycledNotes.Add(new NoteViewModel(note, null, Language, _themeService, _settingsService, null, _model.Safes));
                }
            }
        }

        /// <inheritdoc/>
        public override void OnStoringUnsavedData()
        {
            if (Modified)
            {
                _repositoryService.TrySaveRepository(Model);
                Modified = false;
            }
        }

        ///// <summary>
        ///// Gets the command to go back to the note overview.
        ///// </summary>
        //public ICommand GoBackCommand { get; private set; }

        //private void GoBack()
        //{
        //    _navigationService.Navigate(new Navigation(ControllerNames.NoteRepository));
        //}

        ///// <inheritdoc/>
        //public override void OnGoBackPressed(out bool handled)
        //{
        //    handled = true;
        //    GoBack();
        //}

        /// <summary>
        /// Gets the command which handles the clearing of the recyclebin.
        /// </summary>
        public ICommand EmptyRecycleBinCommand { get; private set; }

        private async void EmptyRecycleBin()
        {
            MessageBoxResult dialogResult = await _feedbackService.ShowMessageAsync(Language["empty_recyclebin_confirmation"], Language["empty_recyclebin"], MessageBoxButtons.ContinueCancel, false);
            if (dialogResult != MessageBoxResult.Continue)
                return;

            if (RecycledNotes.Count > 0)
                Modified = true;
            RecycledNotes.Clear();

            // Search for all notes placed in the recycling bin
            for (int index = Model.Notes.Count - 1; index >= 0; index--)
            {
                NoteModel note = Model.Notes[index];
                if (note.InRecyclingBin)
                {
                    // Register the note as deleted and remove the note from the list
                    Model.DeletedNotes.Add(note.Id);
                    Model.Notes.Remove(note);
                }
            }

            // Reload empty page (but only if user confirmed command)
            //_navigationService.Navigate(new Navigation(ControllerNames.RecycleBin));
        }

        /// <summary>
        /// Gets the command which undeletes a note from the recycle bin.
        /// </summary>
        public ICommand RestoreNoteCommand { get; private set; }

        private void RestoreNote(object value)
        {
            Guid noteId = (value is Guid) ? (Guid)value : new Guid(value.ToString());
            NoteViewModel viewModel = RecycledNotes.Find(item => noteId == item.Id);
            if (viewModel != null)
            {
                Modified = true;
                viewModel.InRecyclingBin = false;
                RecycledNotes.Remove(viewModel);
            }
            OnPropertyChanged("Notes");
        }

        /// <summary>
        /// Gets the command which undeletes a note from the recycle bin.
        /// </summary>
        public ICommand DeleteNotePermanentlyCommand { get; private set; }

        private void DeleteNotePermanently(object value)
        {
            Guid noteId = (value is Guid) ? (Guid)value : new Guid(value.ToString());
            NoteViewModel viewModel = RecycledNotes.Find(item => noteId == item.Id);
            if (viewModel != null)
            {
                Modified = true;

                // Register the note as deleted and remove the note from the list
                Model.DeletedNotes.Add(noteId);
                Model.Notes.Remove(viewModel.Model);
                RecycledNotes.Remove(viewModel);
            }
            OnPropertyChanged("Notes");
        }
    }
}
