// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Windows.Input;
using SilentNotes.Controllers;
using SilentNotes.Models;
using SilentNotes.Services;
using VanillaCloudStorageClient;

namespace SilentNotes.ViewModels
{
    public class ChangePasswordViewModel : ViewModelBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly ICryptoRandomService _randomService;
        private readonly ISettingsService _settingsService;
        private readonly IRepositoryStorageService _repositoryService;
        private SecureString _oldPassword;
        private SecureString _password;
        private SecureString _passwordConfirmation;
        private bool _invalidOldPasswordError;
        private bool _invalidPasswordError;
        private bool _invalidPasswordConfirmationError;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSafeViewModel"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public ChangePasswordViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IThemeService themeService,
            IBaseUrlService webviewBaseUrl,
            IFeedbackService feedbackService,
            ICryptoRandomService randomService,
            ISettingsService settingsService,
            IRepositoryStorageService repositoryService)
            : base(navigationService, languageService, svgIconService, themeService, webviewBaseUrl)
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
            _navigationService.Navigate(new Navigation(ControllerNames.NoteRepository));
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
            List<SafeInfo> matchingSafes = TryPasswordOnSafes(OldPassword);
            InvalidOldPasswordError = matchingSafes.Count == 0;
            InvalidPasswordError = !ValidatePassword(Password);
            InvalidPasswordConfirmationError = !ValidatePasswordConfirmation(Password, PasswordConfirmation);
            if (InvalidOldPasswordError || InvalidPasswordError || InvalidPasswordConfirmationError)
                return;

            // Change the encrypted key of each safe which could have been opened with the password.
            string algorithm = _settingsService.LoadSettingsOrDefault().SelectedEncryptionAlgorithm;
            foreach (SafeInfo safeInfo in matchingSafes)
            {
                // No need to open or close the safe, just replace the encrypted key.
                safeInfo.Safe.SerializeableKey = SafeModel.EncryptKey(safeInfo.Key, Password, _randomService, algorithm);
                safeInfo.Safe.RefreshModifiedAt();
            }

            Modified = true;
            _navigationService.Navigate(new Navigation(ControllerNames.NoteRepository));
        }

        public bool InvalidOldPasswordError
        {
            get { return _invalidOldPasswordError; }
            set { ChangeProperty(ref _invalidOldPasswordError, value, false); }
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
        /// Gets or sets the user entered previous password.
        /// </summary>
        public SecureString OldPassword
        {
            get { return _oldPassword; }

            set
            {
                _oldPassword?.Clear();
                _oldPassword = value;
            }
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

        /// <summary>
        /// Tries the password on all safes and returns a list of decrypted safes/keys.
        /// The function does not change the open/close state of the safe.
        /// </summary>
        /// <param name="password">Password to test with.</param>
        /// <returns>List of safes where the password matches.</returns>
        private List<SafeInfo> TryPasswordOnSafes(SecureString password)
        {
            List<SafeInfo> result = new List<SafeInfo>();
            foreach (SafeModel safe in Model.Safes)
            {
                if (SafeModel.TryDecryptKey(safe.SerializeableKey, password, out byte[] key))
                {
                    result.Add(new SafeInfo { Safe = safe, Key = key });
                }
            }
            return result;
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

        private class SafeInfo
        {
            public SafeModel Safe { get; set; }

            public byte[] Key { get; set; }
        }
    }
}
