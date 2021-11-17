// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Input;
using SilentNotes.Controllers;
using SilentNotes.Crypto.SymmetricEncryption;
using SilentNotes.HtmlView;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.StoryBoards;
using SilentNotes.StoryBoards.SynchronizationStory;
using SilentNotes.Workers;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the application settings to the user.
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        public const double ReferenceFontSize = 15.0; // Used as reference to calculate the font scale
        public const int ReferenceNoteMaxSize = 160;
        public const int ReferenceNoteMinSize = 115;
        private readonly ISettingsService _settingsService;
        private readonly IStoryBoardService _storyBoardService;
        private readonly IFeedbackService _feedbackService;
        private readonly IFilePickerService _filePickerService;
        private readonly SliderStepConverter _fontSizeConverter;
        private readonly SliderStepConverter _noteMaxHeightConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        public SettingsViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IThemeService themeService,
            IBaseUrlService webviewBaseUrl,
            ISettingsService settingsService,
            IStoryBoardService storyBoardService,
            IFeedbackService feedbackService,
            IFilePickerService filePickerService)
            : base(navigationService, languageService, svgIconService, themeService, webviewBaseUrl)
        {
            _settingsService = settingsService;
            _storyBoardService = storyBoardService;
            _feedbackService = feedbackService;
            _filePickerService = filePickerService;
            _fontSizeConverter = new SliderStepConverter(ReferenceFontSize, 1.0);
            _noteMaxHeightConverter = new SliderStepConverter(ReferenceNoteMaxSize, 20.0);
            Model = _settingsService.LoadSettingsOrDefault();

            EncryptionAlgorithms = new List<DropdownItemViewModel>();
            FillAlgorithmList(EncryptionAlgorithms);

            // Initialize commands
            GoBackCommand = new RelayCommand(GoBack);
            ChangeCloudSettingsCommand = new RelayCommand(ChangeCloudSettings);
            ClearCloudSettingsCommand = new RelayCommand(ClearCloudSettings);
            TestNewLocalizationCommand = new RelayCommand(TestNewLocalization);
        }

        /// <summary>
        /// Initializes the list of available cloud storage services.
        /// </summary>
        /// <param name="algorithms">List to fill.</param>
        private void FillAlgorithmList(List<DropdownItemViewModel> algorithms)
        {
            algorithms.Add(new DropdownItemViewModel { Value = BouncyCastleXChaCha20.CryptoAlgorithmName, Description = Language["encryption_algo_xchacha20"] });
            algorithms.Add(new DropdownItemViewModel { Value = BouncyCastleAesGcm.CryptoAlgorithmName, Description = Language["encryption_algo_aesgcm"] });
            algorithms.Add(new DropdownItemViewModel { Value = BouncyCastleTwofishGcm.CryptoAlgorithmName, Description = Language["encryption_algo_twofishgcm"] });
        }

        /// <summary>
        /// Gets or sets the <see cref="FontScale"/> expressed for the -3...+3 slider.
        /// </summary>
        [VueDataBinding(VueBindingMode.TwoWay)]
        public int FontSizeStep
        {
            get
            {
                return _fontSizeConverter.ModelFactorToSliderStep(Model.FontScale);
            }

            set
            {
                ChangePropertyIndirect<int>(
                    () => _fontSizeConverter.ModelFactorToSliderStep(Model.FontScale),
                    (v) => Model.FontScale = _fontSizeConverter.SliderStepToModelFactor(v),
                    value,
                    true);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="NoteMaxHeight"/> expressed for the -4...+4 slider.
        /// </summary>
        [VueDataBinding(VueBindingMode.TwoWay)]
        public int NoteMaxHeightStep
        {
            get
            {
                return _noteMaxHeightConverter.ModelFactorToSliderStep(Model.NoteMaxHeightScale);
            }

            set
            {
                ChangePropertyIndirect<int>(
                    () => _noteMaxHeightConverter.ModelFactorToSliderStep(Model.NoteMaxHeightScale),
                    (v) => Model.NoteMaxHeightScale = _noteMaxHeightConverter.SliderStepToModelFactor(v),
                    value,
                    true);
            }
        }

        /// <summary>
        /// Gets or sets the theme selected by the user.
        /// </summary>
        [VueDataBinding(VueBindingMode.TwoWay)]
        public string SelectedTheme
        {
            get { return Theme.FindThemeOrDefault(Model.SelectedTheme).Id; }

            set
            {
                if (ChangePropertyIndirect(() => Model.SelectedTheme, (string v) => Model.SelectedTheme = v, value, true))
                    OnPropertyChanged(nameof(SelectedThemeImage));
            }
        }

        [VueDataBinding(VueBindingMode.OneWayToView)]
        public string SelectedThemeImage
        {
            get { return Theme.FindThemeOrDefault(Model.SelectedTheme).Image; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the a solid color should be used instead of a
        /// background image.
        /// </summary>
        [VueDataBinding(VueBindingMode.TwoWay)]
        public bool UseSolidColorTheme
        {
            get { return Model.UseSolidColorTheme; }
            set { ChangePropertyIndirect(() => Model.UseSolidColorTheme, (bool v) => Model.UseSolidColorTheme = v, value, true); }
        }

        /// <summary>
        /// Gets or sets the solid background color for the theme background. It depends on
        /// <see cref="UseSolidColorTheme"/> whether this value is respected.
        /// </summary>
        [VueDataBinding(VueBindingMode.TwoWay)]
        public string ColorForSolidThemeHex
        {
            get { return Model.ColorForSolidTheme; }
            set { ChangePropertyIndirect(() => Model.ColorForSolidTheme, (string v) => Model.ColorForSolidTheme = v, value, true); }
        }

        /// <summary>
        /// Gets or sets the theme mode selected by the user.
        /// </summary>
        [VueDataBinding(VueBindingMode.TwoWay)]
        public string SelectedThemeMode
        {
            get { return Model.ThemeMode.ToString(); }

            set
            {
                ChangePropertyIndirect(() => Model.ThemeMode.ToString(), (string v) => Model.ThemeMode = (ThemeMode)Enum.Parse(typeof(ThemeMode), value), value, true);
                _navigationService.Navigate(new Navigation(ControllerNames.Settings));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the note color should be the same for all notes
        /// in dark mode.
        /// </summary>
        [VueDataBinding(VueBindingMode.TwoWay)]
        public bool UseColorForAllNotesInDarkMode
        {
            get { return Model.UseColorForAllNotesInDarkMode; }
            set { ChangePropertyIndirect(() => Model.UseColorForAllNotesInDarkMode, (bool v) => Model.UseColorForAllNotesInDarkMode = v, value, true); }
        }

        /// <summary>
        /// Gets or sets the background color for new notes. It depends on <see cref="UseColorForAllNotesInDarkMode"/>
        /// whether this value is respected.
        /// </summary>
        [VueDataBinding(VueBindingMode.TwoWay)]
        public string ColorForAllNotesInDarkModeHex
        {
            get { return Model.ColorForAllNotesInDarkModeHex; }
            set { ChangePropertyIndirect(() => Model.ColorForAllNotesInDarkModeHex, (string v) => Model.ColorForAllNotesInDarkModeHex = v, value, true); }
        }

        /// <summary>
        /// Gets a list of available background colors.
        /// </summary>
        public List<string> NoteColorsHex
        {
            get { return Model.NoteColorsHex; }
        }

        /// <summary>
        /// Gets or sets the default background color as hex string, e.g. #ff0000
        /// </summary>
        [VueDataBinding(VueBindingMode.TwoWay)]
        public string DefaultNoteColorHex
        {
            get { return Model.DefaultNoteColorHex; }

            set { ChangePropertyIndirect(() => Model.DefaultNoteColorHex, (string v) => Model.DefaultNoteColorHex = v, value, true); }
        }

        /// <summary>
        /// Gets or sets the note insertion mode selected by the user.
        /// </summary>
        [VueDataBinding(VueBindingMode.TwoWay)]
        public string SelectedNoteInsertionMode
        {
            get { return Model.DefaultNoteInsertion.ToString(); }

            set { ChangePropertyIndirect(() => Model.DefaultNoteInsertion.ToString(), (string v) => Model.DefaultNoteInsertion = (NoteInsertionMode)Enum.Parse(typeof(NoteInsertionMode), value), value, true); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether notes should be hidden in the overview, if they
        /// are part of a closed safe.
        /// </summary>
        [VueDataBinding(VueBindingMode.TwoWay)]
        public bool HideClosedSafeNotes
        {
            get { return Model.HideClosedSafeNotes; }
            set { ChangePropertyIndirect(() => Model.HideClosedSafeNotes, (bool v) => Model.HideClosedSafeNotes = v, value, true); }
        }

        /// <summary>
        /// Gets the dark class for a given background color, depending of whether the background
        /// color is a light or a dark color.
        /// </summary>
        /// <param name="backgroundColorHex">Background color of the note.</param>
        /// <returns>Html class "dark" if the background color is dark, otherwise an empty string.</returns>
        public string GetDarkClass(string backgroundColorHex)
        {
            Color backgroundColor = ColorExtensions.HexToColor(backgroundColorHex);
            if (backgroundColor.IsDark())
                return "dark";
            else
                return string.Empty;
        }

        /// <summary>
        /// Gets a list of all available encryption algorithms.
        /// </summary>
        public List<DropdownItemViewModel> EncryptionAlgorithms { get; private set; }

        /// <summary>
        /// Gets or sets the encryption algorithm selected by the user.
        /// </summary>
        [VueDataBinding(VueBindingMode.TwoWay)]
        public string SelectedEncryptionAlgorithm
        {
            get
            {
                DropdownItemViewModel result = EncryptionAlgorithms.Find(item => item.Value == Model.SelectedEncryptionAlgorithm);

                // Search for the default algorithm, if no matching algorithm could be found.
                if (result == null)
                    result = EncryptionAlgorithms.Find(item => item.Value == SettingsModel.GetDefaultEncryptionAlgorithmName());
                return result.Value;
            }

            set { ChangePropertyIndirect(() => Model.SelectedEncryptionAlgorithm, (string v) => Model.SelectedEncryptionAlgorithm = v, value, true); }
        }

        /// <summary>
        /// Gets or sets the auto sync mode selected by the user.
        /// </summary>
        [VueDataBinding(VueBindingMode.TwoWay)]
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
        /// Gets the command to reset the cloud settings.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand ClearCloudSettingsCommand { get; private set; }

        private async void ClearCloudSettings()
        {
            string title = Language.LoadText("cloud_clear_settings_desc");
            string explanation = Language.LoadText("cloud_clear_settings_expl");
            MessageBoxResult dialogResult = await _feedbackService.ShowMessageAsync(explanation, title, MessageBoxButtons.YesNoCancel, false);

            // Remove repository from online storage
            if (dialogResult == MessageBoxResult.Yes)
            {

            }

            // Remove account from settings
            if ((dialogResult == MessageBoxResult.Yes) || (dialogResult == MessageBoxResult.No))
            {
                ChangePropertyIndirect(() => Model.Credentials, (v) => Model.Credentials = v, null, true, nameof(AccountSummary));
            }
        }

        /// <summary>
        /// Gets the command to replace the cloud settings with new ones.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand ChangeCloudSettingsCommand { get; private set; }

        private async void ChangeCloudSettings()
        {
            try
            {
                _storyBoardService.ActiveStory = new SynchronizationStoryBoard(StoryBoardMode.GuiAndToasts);
                await _storyBoardService.ActiveStory.ContinueWith(SynchronizationStoryStepId.ShowCloudStorageChoice);
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
        [VueDataBinding(VueBindingMode.OneWayToView)]
        public string AccountSummary
        {
            get
            {
                if (Model.Credentials == null)
                {
                    return Language["cloud_service_undefined"];
                }
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(Model.Credentials.CloudStorageId);
                    sb.AppendLine(Model.Credentials.Url);
                    return sb.ToString();
                }
            }
        }

        /// <summary>
        /// Gets the command to reset the cloud settings.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand TestNewLocalizationCommand { get; private set; }

        private async void TestNewLocalization()
        {
            try
            {
                if (await _filePickerService.PickFile())
                {
                    byte[] languageFile = await _filePickerService.ReadPickedFile();
                    (Language as ILanguageTestService).OverrideWithTestResourceFile(languageFile);
                    _navigationService.Navigate(new Navigation(ControllerNames.Settings));
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Gets the wrapped model.
        /// </summary>
        internal SettingsModel Model { get; private set; }
    }
}
