// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Security;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;
using VanillaCloudStorageClient;

namespace SilentNotes.ViewModels
{
    public class ChangePasswordViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly ICryptoRandomService _randomService;
        private readonly ISettingsService _settingsService;
        private readonly IRepositoryStorageService _repositoryService;
        private SecureString _oldPassword;
        private SecureString _password;
        private SecureString _passwordConfirmation;
        private bool _hasOldPasswordError;
        private bool _hasPasswordError;
        private bool _hasPasswordConfirmationError;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSafeViewModel"/> class.
        /// </summary>
        public ChangePasswordViewModel(
            INavigationService navigationService,
            ICryptoRandomService randomService,
            ISettingsService settingsService,
            IRepositoryStorageService repositoryService)
        {
            _navigationService = navigationService;
            _randomService = randomService;
            _settingsService = settingsService;
            _repositoryService = repositoryService;

            _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);
            Model = noteRepository;
            Modifications = new ModificationDetector(() => Model?.GetModificationFingerprint());

            OkCommand = new RelayCommand(Ok);
        }

        /// <summary>
        /// Gets a modification detector.
        /// </summary>
        internal ModificationDetector Modifications { get; }

        /// <inheritdoc />
        public void OnStoringUnsavedData()
        {
            if (Modifications.IsModified())
            {
                _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);
                _repositoryService.TrySaveRepository(noteRepository);
                Modifications.MemorizeCurrentState();
            }
        }

        /// <summary>
        /// Gets the command to create the service.
        /// </summary>
        public ICommand OkCommand { get; private set; }

        private void Ok()
        {
            List<SafeInfo> matchingSafes = TryPasswordOnSafes(OldPassword);
            HasOldPasswordError = matchingSafes.Count == 0;
            HasPasswordError = !ValidatePassword(Password);
            HasPasswordConfirmationError = !ValidatePasswordConfirmation(Password, PasswordConfirmation);
            if (HasOldPasswordError || HasPasswordError || HasPasswordConfirmationError)
                return;

            // Change the encrypted key of each safe which could have been opened with the password.
            string algorithm = _settingsService.LoadSettingsOrDefault().SelectedEncryptionAlgorithm;
            foreach (SafeInfo safeInfo in matchingSafes)
            {
                // No need to open or close the safe, just replace the encrypted key.
                safeInfo.Safe.SerializeableKey = SafeModel.EncryptKey(safeInfo.Key, Password, _randomService, algorithm);
                safeInfo.Safe.RefreshModifiedAt();
            }

            _navigationService.NavigateTo(RouteNames.Home);
        }

        public bool HasOldPasswordError
        {
            get { return _hasOldPasswordError; }
            set { SetProperty(ref _hasOldPasswordError, value); }
        }

        public bool HasPasswordError
        {
            get { return _hasPasswordError; }
            set { SetProperty(ref _hasPasswordError, value); }
        }

        public bool HasPasswordConfirmationError
        {
            get { return _hasPasswordConfirmationError; }
            set { SetProperty(ref _hasPasswordConfirmationError, value); }
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
