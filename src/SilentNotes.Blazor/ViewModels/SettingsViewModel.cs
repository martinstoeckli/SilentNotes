using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using System.Windows.Input;
using MudBlazor;
using SilentNotes.Models;
using VanillaCloudStorageClient;
//using Windows.Globalization;
using SilentNotes.Services;
using MudBlazor.Interfaces;

namespace SilentNotes.ViewModels
{
    internal class SettingsViewModel : ViewModelBase
    {
        public const double ReferenceFontSize = 15.0; // Used as reference to calculate the font scale
        public const int ReferenceNoteMaxSize = 160;
        public const int ReferenceNoteMinSize = 72;
        //private readonly ISettingsService _settingsService;
        private readonly IEnvironmentService _environmentService;
        private readonly IThemeService _themeService;
        //private readonly IStoryBoardService _storyBoardService;
        //private readonly IFeedbackService _feedbackService;
        //private readonly IFilePickerService _filePickerService;
        //private readonly ICloudStorageClientFactory _cloudStorageClientFactory;
        private readonly SliderStepConverter _fontSizeConverter;
        private readonly SliderStepConverter _noteMaxHeightConverter;

        public SettingsViewModel(
            SettingsModel model,
            IEnvironmentService environmentService,
            IThemeService themeService)
        {
            Model = model;
            _environmentService = environmentService;
            _themeService = themeService;

            _fontSizeConverter = new SliderStepConverter(ReferenceFontSize, 1.0);
            _noteMaxHeightConverter = new SliderStepConverter(ReferenceNoteMaxSize, 20.0);
        }

        //    /// <summary>
        //    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        //    /// </summary>
        //    public SettingsViewModel(
        //        INavigationService navigationService,
        //        ILanguageService languageService,
        //        ISvgIconService svgIconService,
        //        IThemeService themeService,
        //        IBaseUrlService webviewBaseUrl,
        //        ISettingsService settingsService,
        //        IEnvironmentService environmentService,
        //        IStoryBoardService storyBoardService,
        //        IFeedbackService feedbackService,
        //        ICloudStorageClientFactory cloudStorageClientFactory,
        //        IFilePickerService filePickerService)
        //        : base(navigationService, languageService, svgIconService, themeService, webviewBaseUrl)
        //    {
        //        _settingsService = settingsService;
        //        _environmentService = environmentService;
        //        _storyBoardService = storyBoardService;
        //        _feedbackService = feedbackService;
        //        _cloudStorageClientFactory = cloudStorageClientFactory;
        //        _filePickerService = filePickerService;
        //        _fontSizeConverter = new SliderStepConverter(ReferenceFontSize, 1.0);
        //        _noteMaxHeightConverter = new SliderStepConverter(ReferenceNoteMaxSize, 20.0);
        //        Model = _settingsService.LoadSettingsOrDefault();

        //        EncryptionAlgorithms = new List<DropdownItemViewModel>();
        //        FillAlgorithmList(EncryptionAlgorithms);

        //        // Initialize commands
        //        GoBackCommand = new RelayCommand(GoBack);
        //        ChangeCloudSettingsCommand = new RelayCommand(ChangeCloudSettings);
        //        ClearCloudSettingsCommand = new RelayCommand(ClearCloudSettings);
        //        TestNewLocalizationCommand = new RelayCommand(TestNewLocalization);
        //    }

        //    /// <summary>
        //    /// Initializes the list of available cloud storage services.
        //    /// </summary>
        //    /// <param name="algorithms">List to fill.</param>
        //    private void FillAlgorithmList(List<DropdownItemViewModel> algorithms)
        //    {
        //        algorithms.Add(new DropdownItemViewModel { Value = BouncyCastleXChaCha20.CryptoAlgorithmName, Description = Language["encryption_algo_xchacha20"] });
        //        algorithms.Add(new DropdownItemViewModel { Value = BouncyCastleAesGcm.CryptoAlgorithmName, Description = Language["encryption_algo_aesgcm"] });
        //        algorithms.Add(new DropdownItemViewModel { Value = BouncyCastleTwofishGcm.CryptoAlgorithmName, Description = Language["encryption_algo_twofishgcm"] });
        //    }

        /// <summary>
        /// Gets or sets the <see cref="FontScale"/> expressed for the -3...+3 slider.
        /// </summary>
        //[VueDataBinding(VueBindingMode.TwoWay)]
        public int FontSizeStep
        {
            get { return _fontSizeConverter.ModelFactorToSliderStep(Model.FontScale); }

            set
            {
                SetPropertyAndModified(
                    _fontSizeConverter.ModelFactorToSliderStep(Model.FontScale),
                    value,
                    (v) => Model.FontScale = _fontSizeConverter.SliderStepToModelFactor(v));
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="NoteMaxHeight"/> expressed for the -4...+4 slider.
        /// </summary>
        //[VueDataBinding(VueBindingMode.TwoWay)]
        public int NoteMaxHeightStep
        {
            get { return _noteMaxHeightConverter.ModelFactorToSliderStep(Model.NoteMaxHeightScale); }

            set
            {
                SetPropertyAndModified(
                    _noteMaxHeightConverter.ModelFactorToSliderStep(Model.NoteMaxHeightScale),
                    value,
                    (v) => Model.NoteMaxHeightScale = _noteMaxHeightConverter.SliderStepToModelFactor(v));
            }
        }

        //    /// <summary>
        //    /// Gets or sets the <see cref="NoteMaxHeight"/> expressed for the -4...+4 slider.
        //    /// </summary>
        //    [VueDataBinding(VueBindingMode.TwoWay)]
        //    public int KeepScreenOnDuration
        //    {
        //        get { return Model.KeepScreenUpDuration; }

        //        set
        //        {
        //            if (SetPropertyAndModified(Model.KeepScreenUpDuration, value, (v) => Model.KeepScreenUpDuration = v))
        //            {
        //                OnPropertyChanged(nameof(KeepScreenOnDurationTitle));
        //            }
        //        }
        //    }

        //    public bool CanKeepScreenOn
        //    {
        //        get { return _environmentService?.KeepScreenOn != null; }
        //    }

        //    [VueDataBinding(VueBindingMode.OneWayToView)]
        //    public string KeepScreenOnDurationTitle
        //    {
        //        get { return Language.LoadTextFmt("keep_screen_on_duration", KeepScreenOnDuration); }
        //    }

        //    /// <summary>
        //    /// Gets or sets the theme selected by the user.
        //    /// </summary>
        //    [VueDataBinding(VueBindingMode.TwoWay)]
        //    public string SelectedTheme
        //    {
        //        get { return Theme.FindThemeOrDefault(Model.SelectedTheme).Id; }

        //        set
        //        {
        //            if (SetPropertyAndModified(Model.SelectedTheme, value, (string v) => Model.SelectedTheme = v))
        //            {
        //                OnPropertyChanged(nameof(SelectedThemeImage));
        //            }
        //        }
        //    }

        //    [VueDataBinding(VueBindingMode.OneWayToView)]
        //    public string SelectedThemeImage
        //    {
        //        get { return Theme.FindThemeOrDefault(Model.SelectedTheme).Image; }
        //    }

        /// <summary>
        /// Gets or sets a value indicating whether the a solid color should be used instead of a
        /// background image.
        /// </summary>
        //[VueDataBinding(VueBindingMode.TwoWay)]
        public bool UseSolidColorTheme
        {
            get { return Model.UseSolidColorTheme; }
            set { SetPropertyAndModified(Model.UseSolidColorTheme, value, (bool v) => Model.UseSolidColorTheme = v); }
        }

        /// <summary>
        /// Gets or sets the solid background color for the theme background. It depends on
        /// <see cref="UseSolidColorTheme"/> whether this value is respected.
        /// </summary>
        //[VueDataBinding(VueBindingMode.TwoWay)]
        public string ColorForSolidThemeHex
        {
            get { return Model.ColorForSolidTheme; }
            set { SetPropertyAndModified(Model.ColorForSolidTheme, value, (string v) => Model.ColorForSolidTheme = v); }
        }

        /// <summary>
        /// Gets or sets the theme mode selected by the user.
        /// </summary>
        //[VueDataBinding(VueBindingMode.TwoWay)]
        public string SelectedThemeMode
        {
            get { return Model.ThemeMode.ToString(); }

            set
            {
                if (SetPropertyAndModified(Model.ThemeMode.ToString(), value, (string v) => Model.ThemeMode = (ThemeMode)Enum.Parse(typeof(ThemeMode), value)))
                {
                    _themeService.RefreshGui();
                }
            }
        }

        //    /// <summary>
        //    /// Gets or sets a value indicating whether the note color should be the same for all notes
        //    /// in dark mode.
        //    /// </summary>
        //    [VueDataBinding(VueBindingMode.TwoWay)]
        //    public bool UseColorForAllNotesInDarkMode
        //    {
        //        get { return Model.UseColorForAllNotesInDarkMode; }
        //        set { SetPropertyAndModified(Model.UseColorForAllNotesInDarkMode, value, (bool v) => Model.UseColorForAllNotesInDarkMode = v); }
        //    }

        //    /// <summary>
        //    /// Gets or sets the background color for new notes. It depends on <see cref="UseColorForAllNotesInDarkMode"/>
        //    /// whether this value is respected.
        //    /// </summary>
        //    [VueDataBinding(VueBindingMode.TwoWay)]
        //    public string ColorForAllNotesInDarkModeHex
        //    {
        //        get { return Model.ColorForAllNotesInDarkModeHex; }
        //        set { SetPropertyAndModified(Model.ColorForAllNotesInDarkModeHex, value, (string v) => Model.ColorForAllNotesInDarkModeHex = v); }
        //    }

        //    /// <summary>
        //    /// Gets a list of available background colors.
        //    /// </summary>
        //    public List<string> NoteColorsHex
        //    {
        //        get { return Model.NoteColorsHex; }
        //    }

        //    /// <summary>
        //    /// Gets or sets the default background color as hex string, e.g. #ff0000
        //    /// </summary>
        //    [VueDataBinding(VueBindingMode.TwoWay)]
        //    public string DefaultNoteColorHex
        //    {
        //        get { return Model.DefaultNoteColorHex; }
        //        set { SetPropertyAndModified(Model.DefaultNoteColorHex, value, (string v) => Model.DefaultNoteColorHex = v); }
        //    }

        //    /// <summary>
        //    /// Gets or sets the note insertion mode selected by the user.
        //    /// </summary>
        //    [VueDataBinding(VueBindingMode.TwoWay)]
        //    public string SelectedNoteInsertionMode
        //    {
        //        get { return Model.DefaultNoteInsertion.ToString(); }
        //        set { SetPropertyAndModified(Model.DefaultNoteInsertion.ToString(), value, (string v) => Model.DefaultNoteInsertion = (NoteInsertionMode)Enum.Parse(typeof(NoteInsertionMode), value)); }
        //    }

        /// <summary>
        /// Gets or sets a value indicating whether the last selected tag to filter the notes
        /// should be remembered across startups.
        /// </summary>
        public bool RememberLastTagFilter
        {
            get { return Model.RememberLastTagFilter; }
            set { SetPropertyAndModified(Model.RememberLastTagFilter, value, (bool v) => Model.RememberLastTagFilter = v); }
        }

        // todo:
        public bool StartWithTagsOpen
        {
            get { return Model.StartWithTagsOpen; }
            set { SetPropertyAndModified(Model.StartWithTagsOpen, value, (bool v) => Model.StartWithTagsOpen = v); }
        }

        //    /// <summary>
        //    /// Gets or sets a value indicating whether notes should be hidden in the overview, if they
        //    /// are part of a closed safe.
        //    /// </summary>
        //    [VueDataBinding(VueBindingMode.TwoWay)]
        //    public bool HideClosedSafeNotes
        //    {
        //        get { return Model.HideClosedSafeNotes; }
        //        set { SetPropertyAndModified(Model.HideClosedSafeNotes, value, (bool v) => Model.HideClosedSafeNotes = v); }
        //    }

        //    /// <summary>
        //    /// Gets the dark class for a given background color, depending of whether the background
        //    /// color is a light or a dark color.
        //    /// </summary>
        //    /// <param name="backgroundColorHex">Background color of the note.</param>
        //    /// <returns>Html class "dark" if the background color is dark, otherwise an empty string.</returns>
        //    public string GetDarkClass(string backgroundColorHex)
        //    {
        //        Color backgroundColor = ColorExtensions.HexToColor(backgroundColorHex);
        //        if (backgroundColor.IsDark())
        //            return "dark";
        //        else
        //            return string.Empty;
        //    }

        //    /// <summary>
        //    /// Gets a list of all available encryption algorithms.
        //    /// </summary>
        //    public List<DropdownItemViewModel> EncryptionAlgorithms { get; private set; }

        //    /// <summary>
        //    /// Gets or sets the encryption algorithm selected by the user.
        //    /// </summary>
        //    [VueDataBinding(VueBindingMode.TwoWay)]
        //    public string SelectedEncryptionAlgorithm
        //    {
        //        get
        //        {
        //            DropdownItemViewModel result = EncryptionAlgorithms.Find(item => item.Value == Model.SelectedEncryptionAlgorithm);

        //            // Search for the default algorithm, if no matching algorithm could be found.
        //            if (result == null)
        //                result = EncryptionAlgorithms.Find(item => item.Value == SettingsModel.GetDefaultEncryptionAlgorithmName());
        //            return result.Value;
        //        }

        //        set { SetPropertyAndModified(Model.SelectedEncryptionAlgorithm, value, (string v) => Model.SelectedEncryptionAlgorithm = v); }
        //    }

        //    /// <summary>
        //    /// Gets or sets a value indicating whether taking screenshots should be forbidden or allowed.
        //    /// </summary>
        //    [VueDataBinding(VueBindingMode.TwoWay)]
        //    public bool PreventScreenshots
        //    {
        //        get { return Model.PreventScreenshots; }
        //        set
        //        {
        //            if (SetPropertyAndModified(Model.PreventScreenshots, value, (bool v) => Model.PreventScreenshots = v))
        //            {
        //                if (_environmentService.Screenshots != null)
        //                    _environmentService.Screenshots.PreventScreenshots = value;
        //            }
        //        }
        //    }

        //    /// <summary>
        //    /// Gets a value indicating whether the operating system is able to prevent screenshots.
        //    /// </summary>
        //    public bool CanPreventScreenshots
        //    {
        //        get { return _environmentService.Screenshots != null; }
        //    }

        //    /// <summary>
        //    /// Gets or sets the auto sync mode selected by the user.
        //    /// </summary>
        //    [VueDataBinding(VueBindingMode.TwoWay)]
        //    public string SelectedAutoSyncMode
        //    {
        //        get { return Model.AutoSyncMode.ToString(); }
        //        set { SetPropertyAndModified(Model.AutoSyncMode.ToString(), value, (string v) => Model.AutoSyncMode = (AutoSynchronizationMode)Enum.Parse(typeof(AutoSynchronizationMode), value)); }
        //    }

        //    /// <inheritdoc/>
        //    public override void OnStoringUnsavedData()
        //    {
        //        if (Modified)
        //        {
        //            _settingsService.TrySaveSettingsToLocalDevice(Model);
        //            Modified = false;
        //        }
        //    }

        //    /// <summary>
        //    /// Gets the command to go back to the note overview.
        //    /// </summary>
        //    [VueDataBinding(VueBindingMode.Command)]
        //    public ICommand GoBackCommand { get; private set; }

        //    private void GoBack()
        //    {
        //        _navigationService.Navigate(new Navigation(ControllerNames.NoteRepository));
        //    }

        //    /// <inheritdoc/>
        //    public override void OnGoBackPressed(out bool handled)
        //    {
        //        handled = true;
        //        GoBack();
        //    }

        //    /// <summary>
        //    /// Gets the command to reset the cloud settings.
        //    /// </summary>
        //    [VueDataBinding(VueBindingMode.Command)]
        //    public ICommand ClearCloudSettingsCommand { get; private set; }

        //    private async void ClearCloudSettings()
        //    {
        //        string title = Language.LoadText("cloud_clear_settings_desc");
        //        string explanation = Language.LoadText("cloud_clear_settings_expl");
        //        MessageBoxResult dialogResult = await _feedbackService.ShowMessageAsync(explanation, title, MessageBoxButtons.YesNoCancel, false);

        //        // Remove repository from online storage
        //        if (dialogResult == MessageBoxResult.No)
        //        {
        //            await TryDeleteCloudNoteRepository();
        //        }

        //        // Remove account from settings
        //        if ((dialogResult == MessageBoxResult.Yes) || (dialogResult == MessageBoxResult.No))
        //        {
        //            SetPropertyAndModified(Model.Credentials, null, (v) => Model.Credentials = v, nameof(AccountSummary));
        //            OnPropertyChanged(nameof(ClearCloudSettingsDisabled));
        //        }
        //    }

        //    [VueDataBinding(VueBindingMode.OneWayToView)]
        //    public bool ClearCloudSettingsDisabled
        //    {
        //        get { return Model.Credentials == null; }
        //    }

        //    private async Task<bool> TryDeleteCloudNoteRepository()
        //    {
        //        var credentials = Model.Credentials;
        //        ICloudStorageClient cloudStorageClient = _cloudStorageClientFactory.GetByKey(credentials.CloudStorageId);
        //        try
        //        {
        //            await cloudStorageClient.DeleteFileAsync(Config.RepositoryFileName, credentials);
        //            return true;
        //        }
        //        catch (Exception)
        //        {
        //            return false;
        //        }
        //    }

        //    /// <summary>
        //    /// Gets the command to replace the cloud settings with new ones.
        //    /// </summary>
        //    [VueDataBinding(VueBindingMode.Command)]
        //    public ICommand ChangeCloudSettingsCommand { get; private set; }

        //    private async void ChangeCloudSettings()
        //    {
        //        try
        //        {
        //            _storyBoardService.ActiveStory = new SynchronizationStoryBoard(StoryBoardMode.Gui);
        //            await _storyBoardService.ActiveStory.ContinueWith(SynchronizationStoryStepId.ShowCloudStorageChoice);
        //        }
        //        catch (Exception)
        //        {
        //            _storyBoardService.ActiveStory = null;
        //            throw;
        //        }
        //    }

        //    /// <summary>
        //    /// Gets a summary of the cloud storage account.
        //    /// </summary>
        //    [VueDataBinding(VueBindingMode.OneWayToView)]
        //    public string AccountSummary
        //    {
        //        get
        //        {
        //            if (Model.Credentials == null)
        //            {
        //                return Language["cloud_service_undefined"];
        //            }
        //            else
        //            {
        //                StringBuilder sb = new StringBuilder();
        //                sb.AppendLine(Model.Credentials.CloudStorageId);
        //                sb.AppendLine(Model.Credentials.Url);
        //                return sb.ToString();
        //            }
        //        }
        //    }

        //    /// <summary>
        //    /// Gets the command to reset the cloud settings.
        //    /// </summary>
        //    [VueDataBinding(VueBindingMode.Command)]
        //    public ICommand TestNewLocalizationCommand { get; private set; }

        //    private async void TestNewLocalization()
        //    {
        //        try
        //        {
        //            if (await _filePickerService.PickFile())
        //            {
        //                byte[] languageFile = await _filePickerService.ReadPickedFile();
        //                (Language as ILanguageTestService).OverrideWithTestResourceFile(languageFile);
        //                _navigationService.Navigate(new Navigation(ControllerNames.Settings));
        //            }
        //        }
        //        catch (Exception)
        //        {
        //        }
        //    }

        /// <summary>
        /// Gets the wrapped model.
        /// </summary>
        internal SettingsModel Model { get; }
    }
}
