// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Windows.Input;
using SilentNotes.Controllers;
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
        private readonly IFeedbackService _feedbackService;
        private readonly ICryptoRandomService _randomService;
        private readonly ISettingsService _settingsService;
        private readonly IRepositoryStorageService _repositoryService;
        private SecureString _password;
        private SecureString _passwordConfirmation;
        private bool _invalidPasswordError;
        private bool _invalidPasswordConfirmationError;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSafeViewModel"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public OpenSafeViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IBaseUrlService webviewBaseUrl,
            IFeedbackService feedbackService,
            ICryptoRandomService randomService,
            ISettingsService settingsService,
            IRepositoryStorageService repositoryService)
            : base(navigationService, languageService, svgIconService, webviewBaseUrl)
        {
            _feedbackService = feedbackService ?? throw new ArgumentNullException(nameof(feedbackService));
            _randomService = randomService;
            _settingsService = settingsService;
            _repositoryService = repositoryService;

            _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);
            Model = noteRepository;

            GoBackCommand = new RelayCommand(GoBack);
            CancelCommand = new RelayCommand(Cancel);
            OkCommand = new RelayCommand(Ok);
        }

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
        /// Gets the command to go back to the note overview.
        /// </summary>
        public ICommand GoBackCommand { get; private set; }

        private void GoBack()
        {
            _navigationService.Navigate(ControllerNames.NoteRepository);
        }

        /// <inheritdoc/>
        public override void OnGoBackPressed(out bool handled)
        {
            handled = true;
            GoBack();
        }

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        public ICommand CancelCommand { get; private set; }

        private void Cancel()
        {
            GoBack();
        }

        /// <summary>
        /// Gets the command to create the service.
        /// </summary>
        public ICommand OkCommand { get; private set; }

        private void Ok()
        {
            if (!ValidatePassword(Password))
            {
                InvalidPasswordError = true;
                InvalidPasswordConfirmationError = false;
                return;
            }
            if (!SafeExists && !ValidatePasswordConfirmation(Password, PasswordConfirmation))
            {
                InvalidPasswordError = false;
                InvalidPasswordConfirmationError = true;
                return;
            }

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
                InvalidPasswordError = false;
                InvalidPasswordConfirmationError = false;
                _feedbackService.ShowToast(Language.LoadText("password_wrong_error"));
            }
            else
            {
                if (Model.ReuniteOpenSafes() > 0)
                    Modified = true;
                _navigationService.Navigate(ControllerNames.NoteRepository);
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
        /// Gets a value indicating whether there are already one or more safes in the repository.
        /// </summary>
        public bool SafeExists
        {
            get { return Model.Safes.Count >= 1; }
        }

        public bool InvalidPasswordError
        {
            get { return _invalidPasswordError; }
            set { ChangeProperty(ref _invalidPasswordError, value, false); }
        }

        public bool InvalidPasswordConfirmationError
        {
            get { return _invalidPasswordConfirmationError; }
            set { ChangeProperty(ref _invalidPasswordConfirmationError, value, false); }
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

        private static bool ValidatePassword(SecureString password)
        {
            return (password != null) && (password.Length >= 5);
        }

        private static bool ValidatePasswordConfirmation(SecureString password, SecureString passwordConfirmation)
        {
            return SecureStringExtensions.AreEqual(password, passwordConfirmation);
        }

        /// <summary>
        /// Gets the wrapped model.
        /// </summary>
        internal NoteRepositoryModel Model { get; private set; }
    }
}
