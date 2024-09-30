// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Crypto;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;

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
        private readonly IMessengerService _messengerService;
        private readonly ISafeKeyService _keyService;
        private readonly ICryptor _noteCryptor;
        private NoteRepositoryModel _model;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecycleBinViewModel"/> class.
        /// </summary>
        /// <param name="model">The model to wrap.</param>
        public RecycleBinViewModel(
            NoteRepositoryModel model,
            ILanguageService languageService,
            IThemeService themeService,
            IFeedbackService feedbackService,
            ISettingsService settingsService,
            ISafeKeyService keyService,
            ICryptoRandomSource randomSource,
            IRepositoryStorageService repositoryService,
            IMessengerService messengerService)
        {
            _feedbackService = feedbackService;
            Language = languageService;
            _themeService = themeService;
            _settingsService = settingsService;
            _keyService = keyService;
            _repositoryService = repositoryService;
            _messengerService = messengerService;
            _noteCryptor = new Cryptor(NoteModel.CryptorPackageName, randomSource);
            RecycledNotes = new List<NoteViewModelReadOnly>();

            Model = model;
            Modifications = new ModificationDetector(() => Model?.GetModificationFingerprint());

            // Initialize commands
            RestoreNoteCommand = new RelayCommand<object>(RestoreNote);
            DeleteNotePermanentlyCommand = new RelayCommand<object>(DeleteNotePermanently);
            EmptyRecycleBinCommand = new RelayCommand(EmptyRecycleBin);
        }

        private ILanguageService Language { get; }

        /// <summary>
        /// Gets a modification detector.
        /// </summary>
        internal ModificationDetector Modifications { get; }

        /// <summary>
        /// Gets a bindable list of the recycled notes.
        /// </summary>
        public List<NoteViewModelReadOnly> RecycledNotes { get; private set; }

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
                foreach (NoteModel note in _model.Notes)
                {
                    if (note.InRecyclingBin)
                    {
                        RecycledNotes.Add(new NoteViewModelReadOnly(
                            note, null, _themeService, _settingsService, _keyService, _noteCryptor, _model.Safes));
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void OnStoringUnsavedData()
        {
            if (Modifications.IsModified())
            {
                _repositoryService.TrySaveRepository(Model);
                Modifications.MemorizeCurrentState();
            }
        }

        /// <summary>
        /// Gets the command which handles the clearing of the recyclebin.
        /// </summary>
        public ICommand EmptyRecycleBinCommand { get; private set; }

        private async void EmptyRecycleBin()
        {
            MessageBoxResult dialogResult = await _feedbackService.ShowMessageAsync(Language["empty_recyclebin_confirmation"], Language["empty_recyclebin"], MessageBoxButtons.ContinueCancel, false);
            if (dialogResult != MessageBoxResult.Continue)
                return;

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
            _messengerService.Send(new RedrawCurrentPageMessage());
        }

        /// <summary>
        /// Gets the command which undeletes a note from the recycle bin.
        /// </summary>
        public ICommand RestoreNoteCommand { get; private set; }

        private void RestoreNote(object value)
        {
            Guid noteId = (value is Guid) ? (Guid)value : new Guid(value.ToString());
            NoteViewModelReadOnly viewModel = RecycledNotes.Find(item => noteId == item.Id);
            if (viewModel != null)
            {
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
            NoteViewModelReadOnly viewModel = RecycledNotes.Find(item => noteId == item.Id);
            if (viewModel != null)
            {
                // Register the note as deleted and remove the note from the list
                Model.DeletedNotes.Add(noteId);
                Model.Notes.Remove(viewModel.Model);
                RecycledNotes.Remove(viewModel);
            }
            OnPropertyChanged("Notes");
        }
    }
}
