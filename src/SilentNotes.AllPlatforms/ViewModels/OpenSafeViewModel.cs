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
using SilentNotes.Workers;
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
        private readonly ISafeKeyService _keyService;
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
            ISafeKeyService safeKeyService,
            string navigationTargetRoute)
        {
            Language = languageService;
            _navigationService = navigationService;
            _feedbackService = feedbackService;
            _randomService = randomService;
            _settingsService = settingsService;
            _repositoryService = repositoryService;
            _navigationTargetRoute = navigationTargetRoute;
            _keyService = safeKeyService;

            _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);
            Model = noteRepository;
            Modifications = new ModificationDetector(() => Model?.GetModificationFingerprint());

            OkCommand = new RelayCommand(Ok);
            ResetSafeCommand = new RelayCommand(ResetSafe);
        }

        private ILanguageService Language { get; }

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
            ValidatePassword(Password);
            ValidatePasswordConfirmation(Password, PasswordConfirmation);
            if (HasPasswordError || HasPasswordConfirmationError)
                return;

            int openedSafeCount = 0;
            if (SafeExists)
            {
                List<SafeOpenResult> openedSafes = TryOpenSafes(Password);
                openedSafeCount = openedSafes.Count;
                ReEncryptSafesIfNecessary(openedSafes, Password);
            }
            else
            {
                CreateNewSafe(Password);
                openedSafeCount++;
            }

            if (openedSafeCount == 0)
            {
                HasPasswordError = true;
                PasswordErrorText = Language.LoadText("password_wrong_error");
            }
            else if (string.IsNullOrEmpty(_navigationTargetRoute))
            {
                _navigationService.NavigateTo(RouteNames.NoteRepository);
            }
            else
            {
                _navigationService.NavigateTo(_navigationTargetRoute);
            }
        }

        private void ReEncryptSafesIfNecessary(List<SafeOpenResult> openedSafes, SecureString password)
        {
            var safesNeedingReEncryption = openedSafes.Where(item => item.NeedsReEncryption).ToArray();
            if (safesNeedingReEncryption.Length > 0)
            {
                string algorithm = _settingsService.LoadSettingsOrDefault().SelectedEncryptionAlgorithm;
                string kdfAlgorithm = _settingsService.LoadSettingsOrDefault().SelectedKdfAlgorithm;

                foreach (var safeNeedingReEncryption in safesNeedingReEncryption)
                {
                    // No need to open or close the safe, just replace the encrypted key.
                    safeNeedingReEncryption.Safe.SerializeableKey = SafeModel.EncryptKey(safeNeedingReEncryption.Key, password, _randomService, algorithm, kdfAlgorithm);
                    safeNeedingReEncryption.Safe.RefreshModifiedAt();
                }
            }
        }

        private void CreateNewSafe(SecureString password)
        {
            SafeModel safe = new SafeModel();
            string algorithm = _settingsService.LoadSettingsOrDefault().SelectedEncryptionAlgorithm;
            string kdfAlgorithm = _settingsService.LoadSettingsOrDefault().SelectedKdfAlgorithm;

            // We generate a 256 bit key, this is required by all available symmetric encryption
            // algorithms and is more than big enough even for future algorithms.
            var key = _randomService.GetRandomBytes(32);
            safe.SerializeableKey = SafeModel.EncryptKey(key, password, _randomService, algorithm, kdfAlgorithm);

            // Double check that key can be decrypted
            if (!_keyService.TryOpenSafe(safe, password, out _))
                throw new Exception("Safe could not be opened!");

            Model.Safes.Add(safe);
        }

        private List<SafeOpenResult> TryOpenSafes(SecureString password)
        {
            var result = new List<SafeOpenResult>();
            foreach (SafeModel safe in Model.Safes)
            {
                _keyService.CloseSafe(safe.Id); // Actually it shouldn't be possible to have an open safe at this time...
                if (_keyService.TryOpenSafe(safe, password, out bool needsReEncryption))
                {
                    _keyService.TryGetKey(safe.Id, out byte[] key); // Doesn't copy the key in memory
                    result.Add(new SafeOpenResult(safe, key, needsReEncryption));
                }
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
                    Model.DeletedNotes.AddIdOrRefreshDeletedAt(protectedNote.Id);
                    Model.Notes.Remove(protectedNote);
                }

                // Remove all safes
                Model.Safes.Clear();
                _keyService.CloseAllSafes();

                // Continue with the create safe dialog.
                OnStoringUnsavedData();
                _navigationService.NavigateReload();
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
