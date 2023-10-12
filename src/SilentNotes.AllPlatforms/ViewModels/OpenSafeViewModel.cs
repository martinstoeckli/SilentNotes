// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SilentNotes.Models;
using SilentNotes.Services;
using VanillaCloudStorageClient;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the cloud storage settings to the user.
    /// </summary>
    public class OpenSafeViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IFeedbackService _feedbackService;
        private readonly ICryptoRandomService _randomService;
        private readonly ISettingsService _settingsService;
        private readonly IRepositoryStorageService _repositoryService;
        private readonly string _navigationTargetRoute;
        private SecureString _password;
        private SecureString _passwordConfirmation;
        private bool _hasPasswordError;
        private string _passwordErrorText;
        private bool _hasPasswordConfirmationError;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSafeViewModel"/> class.
        /// </summary>
        public OpenSafeViewModel(
            ILanguageService languageService,
            INavigationService navigationService,
            IFeedbackService feedbackService,
            ICryptoRandomService randomService,
            ISettingsService settingsService,
            IRepositoryStorageService repositoryService,
            string navigationTargetRoute)
        {
            Language = languageService;
            _navigationService = navigationService;
            _feedbackService = feedbackService ?? throw new ArgumentNullException(nameof(feedbackService));
            _randomService = randomService;
            _settingsService = settingsService;
            _repositoryService = repositoryService;
            _navigationTargetRoute = navigationTargetRoute;

            _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);
            Model = noteRepository;

            OkCommand = new RelayCommand(Ok);
            ResetSafeCommand = new RelayCommand(ResetSafe);
        }

        private ILanguageService Language { get; }

        /// <inheritdoc />
        public override void OnStoringUnsavedData()
        {
            if (Modified)
            {
                _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);
                _repositoryService.TrySaveRepository(noteRepository);
                Modified = false;
            }
        }

        /// <summary>
        /// Gets the command to create the service.
        /// </summary>
        public ICommand OkCommand { get; private set; }

        private void Ok()
        {
            ValidatePassword(Password);
            ValidatePasswordConfirmation(Password, PasswordConfirmation);
            if (HasPasswordError || HasPasswordConfirmationError)
                return;

            int openedSafes = 0;
            if (SafeExists)
            {
                openedSafes = TryOpenSafes(Password);
            }
            else
            {
                CreateNewSafe(Password);
                openedSafes++;
                Modified = true;
            }

            if (openedSafes == 0)
            {
                HasPasswordError = true;
                PasswordErrorText = Language.LoadText("password_wrong_error");
            }
            else if (string.IsNullOrEmpty(_navigationTargetRoute))
            {
                _navigationService.NavigateHome();
            }
            else
            {
                _navigationService.NavigateTo(_navigationTargetRoute, true);
            }
        }

        private void CreateNewSafe(SecureString password)
        {
            SafeModel safe = new SafeModel();
            string algorithm = _settingsService.LoadSettingsOrDefault().SelectedEncryptionAlgorithm;
            safe.GenerateNewKey(password, _randomService, algorithm);
            Model.Safes.Add(safe);
        }

        private int TryOpenSafes(SecureString password)
        {
            int result = 0;
            foreach (SafeModel safe in Model.Safes)
            {
                safe.Close(); // Actually it shouldn't be possible to have an open safe at this time...
                if (safe.TryOpen(password))
                    result++;
            }
            return result;
        }

        /// <summary>
        /// Gets the command to reset the safe(s). This command can only be called from the safe-open
        /// dialog, so there cannot be any open notes at this time.
        /// </summary>
        public ICommand ResetSafeCommand { get; private set; }

        private async void ResetSafe()
        {
            MessageBoxResult dialogResult = await _feedbackService.ShowMessageAsync(Language.LoadText("safe_reset_warning"), Language.LoadText("safe_reset"), MessageBoxButtons.ContinueCancel, true);
            if (dialogResult == MessageBoxResult.Continue)
            {
                // Move all protected notes to the deleted notes
                List<NoteModel> protectedNotes = Model.Notes.Where(item => item.SafeId != null).ToList();
                foreach (NoteModel protectedNote in protectedNotes)
                {
                    Model.DeletedNotes.Add(protectedNote.Id);
                    Model.Notes.Remove(protectedNote);
                }

                // Remove all safes
                Model.Safes.Clear();
                Modified = true;

                // Continue with the create safe dialog
                _navigationService.NavigateHome();
            }
        }

        /// <summary>
        /// Gets a value indicating whether there are already one or more safes in the repository.
        /// </summary>
        public bool SafeExists
        {
            get { return Model.Safes.Count >= 1; }
        }

        /// <summary>
        /// Gets or sets the user entered password.
        /// </summary>
        public SecureString Password
        {
            get { return _password; }

            set
            {
                _password?.Clear();
                _password = value;
            }
        }

        public bool HasPasswordError
        {
            get { return _hasPasswordError; }
            set { SetProperty(ref _hasPasswordError, value); }
        }

        public string PasswordErrorText
        {
            get { return _passwordErrorText; }
            set { SetProperty(ref _passwordErrorText, value); }
        }

        /// <summary>
        /// Gets or sets the user entered password confirmation.
        /// </summary>
        public SecureString PasswordConfirmation
        {
            get { return _passwordConfirmation; }

            set
            {
                _passwordConfirmation?.Clear();
                _passwordConfirmation = value;
            }
        }

        public bool HasPasswordConfirmationError
        {
            get { return _hasPasswordConfirmationError; }
            set { SetProperty(ref _hasPasswordConfirmationError, value); }
        }

        private void ValidatePassword(SecureString password)
        {
            if ((password == null) || (password.Length < 5))
            {
                HasPasswordError = true;
                PasswordErrorText = Language.LoadText("password_short_error");
            }
            else
            {
                HasPasswordError = false;
                PasswordErrorText = null;
            }
        }

        private void ValidatePasswordConfirmation(SecureString password, SecureString passwordConfirmation)
        {
            if (SafeExists)
                HasPasswordConfirmationError = false; // open dialog, not creation
            else if (SecureStringExtensions.AreEqual(password, passwordConfirmation))
                HasPasswordConfirmationError = false;
            else
                HasPasswordConfirmationError = true;
        }

        /// <summary>
        /// Gets the wrapped model.
        /// </summary>
        internal NoteRepositoryModel Model { get; private set; }
    }
}
