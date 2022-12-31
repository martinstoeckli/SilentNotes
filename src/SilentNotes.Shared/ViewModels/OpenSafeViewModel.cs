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
using SilentNotes.Controllers;
using SilentNotes.HtmlView;
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
        private readonly Navigation _navigationTarget; // Optional target, which should be navigated to after successfully opening the safe.
        private SecureString _password;
        private SecureString _passwordConfirmation;
        private bool _invalidPasswordError;
        private bool _invalidPasswordConfirmationError;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSafeViewModel"/> class.
        /// </summary>
        public OpenSafeViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IThemeService themeService,
            IBaseUrlService webviewBaseUrl,
            IFeedbackService feedbackService,
            ICryptoRandomService randomService,
            ISettingsService settingsService,
            IRepositoryStorageService repositoryService,
            Navigation navigationTarget)
            : base(navigationService, languageService, svgIconService, themeService, webviewBaseUrl)
        {
            _feedbackService = feedbackService ?? throw new ArgumentNullException(nameof(feedbackService));
            _randomService = randomService;
            _settingsService = settingsService;
            _repositoryService = repositoryService;
            _navigationTarget = navigationTarget;

            _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);
            Model = noteRepository;

            GoBackCommand = new RelayCommand(GoBack);
            CancelCommand = new RelayCommand(Cancel);
            OkCommand = new RelayCommand(Ok);
            ResetSafeCommand = new RelayCommand(ResetSafe);
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
        [VueDataBinding(VueBindingMode.Command)]
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
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand CancelCommand { get; private set; }

        private void Cancel()
        {
            GoBack();
        }

        /// <summary>
        /// Gets the command to create the service.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand OkCommand { get; private set; }

        private void Ok()
        {
            InvalidPasswordError = !ValidatePassword(Password);
            InvalidPasswordConfirmationError = !SafeExists && !ValidatePasswordConfirmation(Password, PasswordConfirmation);
            if (InvalidPasswordError || InvalidPasswordConfirmationError)
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
                InvalidPasswordError = false;
                InvalidPasswordConfirmationError = false;
                _feedbackService.ShowToast(Language.LoadText("password_wrong_error"));
            }
            else if (_navigationTarget != null)
            {
                _navigationService.Navigate(_navigationTarget);
            }
            else
            {
                _navigationService.Navigate(new Navigation(ControllerNames.NoteRepository));
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
        [VueDataBinding(VueBindingMode.Command)]
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
                _navigationService.Navigate(new Navigation(ControllerNames.OpenSafe));
            }
        }

        /// <summary>
        /// Gets a value indicating whether there are already one or more safes in the repository.
        /// </summary>
        public bool SafeExists
        {
            get { return Model.Safes.Count >= 1; }
        }

        [VueDataBinding(VueBindingMode.OneWayToView)]
        public bool InvalidPasswordError
        {
            get { return _invalidPasswordError; }
            set { SetProperty(ref _invalidPasswordError, value); }
        }

        [VueDataBinding(VueBindingMode.OneWayToView)]
        public bool InvalidPasswordConfirmationError
        {
            get { return _invalidPasswordConfirmationError; }
            set { SetProperty(ref _invalidPasswordConfirmationError, value); }
        }

        /// <summary>
        /// Gets or sets the user entered password.
        /// </summary>
        [VueDataBinding(VueBindingMode.OneWayToViewmodel)]
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
        [VueDataBinding(VueBindingMode.OneWayToViewmodel)]
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
