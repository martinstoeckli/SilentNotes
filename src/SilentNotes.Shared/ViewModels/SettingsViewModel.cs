﻿// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Windows.Input;
using SilentNotes.Controllers;
using SilentNotes.Crypto.SymmetricEncryption;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.StoryBoards.SynchronizationStory;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the application settings to the user.
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private readonly IThemeService _themeService;
        private readonly IStoryBoardService _storyBoardService;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public SettingsViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IBaseUrlService webviewBaseUrl,
            ISettingsService settingsService,
            IThemeService themeService,
            IStoryBoardService storyBoardService)
            : base(navigationService, languageService, svgIconService, webviewBaseUrl)
        {
            _settingsService = settingsService;
            _themeService = themeService;
            _storyBoardService = storyBoardService;
            Model = _settingsService.LoadSettingsOrDefault();

            EncryptionAlgorithms = new List<DropdownItemViewModel>();
            FillAlgorithmList(EncryptionAlgorithms);

            // Initialize commands
            GoBackCommand = new RelayCommand(GoBack);
            ChangeCloudSettingsCommand = new RelayCommand(ChangeCloudSettings);
            ClearCloudSettingsCommand = new RelayCommand(ClearCloudSettings);
        }

        /// <summary>
        /// Initializes the list of available cloud storage services.
        /// </summary>
        /// <param name="algorithms">List to fill.</param>
        private void FillAlgorithmList(List<DropdownItemViewModel> algorithms)
        {
            algorithms.Add(new DropdownItemViewModel { Value = BouncyCastleAesGcm.CryptoAlgorithmName, Description = Language["encryption_algo_aesgcm"] });
            algorithms.Add(new DropdownItemViewModel { Value = BouncyCastleTwofishGcm.CryptoAlgorithmName, Description = Language["encryption_algo_twofishgcm"] });
        }

        /// <summary>
        /// Gets a list of all available themes.
        /// </summary>
        public List<ThemeModel> Themes
        {
            get { return _themeService.Themes; }
        }

        /// <summary>
        /// Gets or sets the theme selected by the user.
        /// </summary>
        public ThemeModel SelectedTheme
        {
            get { return _themeService.FindThemeOrDefault(Model.SelectedTheme); }
            set { ChangePropertyIndirect(() => Model.SelectedTheme, (string v) => Model.SelectedTheme = v, value.Id, true); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the virtual arrow keys should be displayed.
        /// </summary>
        public bool ShowCursorArrowKeys
        {
            get { return Model.ShowCursorArrowKeys; }
            set { ChangePropertyIndirect<bool>(() => Model.ShowCursorArrowKeys, (v) => Model.ShowCursorArrowKeys = v, value, true); }
        }

        /// <summary>
        /// Gets a list of all available encryption algorithms.
        /// </summary>
        public List<DropdownItemViewModel> EncryptionAlgorithms { get; private set; }

        /// <summary>
        /// Gets or sets the encryption algorithm selected by the user.
        /// </summary>
        public DropdownItemViewModel SelectedEncryptionAlgorithm
        {
            get
            {
                DropdownItemViewModel result = EncryptionAlgorithms.Find(item => item.Value == Model.SelectedEncryptionAlgorithm);

                // Search for the default algorithm, if no matching algorithm could be found.
                if (result == null)
                    result = EncryptionAlgorithms.Find(item => item.Value == SettingsModel.GetDefaultEncryptionAlgorithmName());
                return result;
            }

            set { ChangePropertyIndirect(() => Model.SelectedEncryptionAlgorithm, (string v) => Model.SelectedEncryptionAlgorithm = v, value.Value, true); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the encryption algorithm from the cloud storage
        /// should replace the selected encryption algorithm.
        /// </summary>
        public bool AdoptCloudEncryptionAlgorithm
        {
            get { return Model.AdoptCloudEncryptionAlgorithm; }
            set { ChangePropertyIndirect<bool>(() => Model.AdoptCloudEncryptionAlgorithm, (v) => Model.AdoptCloudEncryptionAlgorithm = v, value, true); }
        }

        /// <summary>
        /// Gets or sets the auto sync mode selected by the user.
        /// </summary>
        public string SelectedAutoSyncMode
        {
            get { return Model.AutoSyncMode.ToString(); }

            set { ChangePropertyIndirect(() => Model.AutoSyncMode.ToString(), (string v) => Model.AutoSyncMode = (AutoSynchronizationMode)Enum.Parse(typeof(AutoSynchronizationMode), value), value, true); }
        }

        /// <inheritdoc/>
        public override void OnStoringUnsavedData()
        {
            if (Modified)
            {
                _settingsService.TrySaveSettingsToLocalDevice(Model);
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
        /// Gets the command to reset the cloud settings.
        /// </summary>
        public ICommand ClearCloudSettingsCommand { get; private set; }

        private void ClearCloudSettings()
        {
            ChangePropertyIndirect(() => Model.CloudStorageAccount, (v) => Model.CloudStorageAccount = v, null, true, nameof(AccountSummary));
        }

        /// <summary>
        /// Gets the command to replace the cloud settings with new ones.
        /// </summary>
        public ICommand ChangeCloudSettingsCommand { get; private set; }

        private async void ChangeCloudSettings()
        {
            try
            {
                _storyBoardService.ActiveStory = new SynchronizationStoryBoard(false);
                await _storyBoardService.ActiveStory.ContinueWith(SynchronizationStoryStepId.ShowCloudStorageChoice.ToInt());
            }
            catch (Exception)
            {
                _storyBoardService.ActiveStory = null;
                throw;
            }
        }

        /// <summary>
        /// Gets a summary of the cloud storage account.
        /// </summary>
        public string AccountSummary
        {
            get
            {
                if (Model.CloudStorageAccount == null)
                {
                    return Language["cloud_service_undefined"];
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(Model.CloudStorageAccount.CloudType.ToString());
                    sb.AppendLine(Model.CloudStorageAccount.Url);
                    sb.AppendLine(Model.CloudStorageAccount.Username);
                    return sb.ToString();
                }
            }
        }

        /// <summary>
        /// Gets the wrapped model.
        /// </summary>
        internal SettingsModel Model { get; private set; }
    }
}
