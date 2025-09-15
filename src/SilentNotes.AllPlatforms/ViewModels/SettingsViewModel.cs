using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Crypto.KeyDerivation;
using SilentNotes.Crypto.SymmetricEncryption;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Stories;
using SilentNotes.Stories.SynchronizationStory;
using SilentNotes.Workers;
using VanillaCloudStorageClient;

using Color = System.Drawing.Color;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the settings page.
    /// </summary>
    /// <remarks>
    /// Update <see cref="SettingsViewModel.GetModificationFingerprint(SettingsModel)"/> when adding
    /// new properties to this class!
    /// </remarks>
    internal class SettingsViewModel : ViewModelBase
    {
        public const double ReferenceFontSize = 15.0; // Used as reference to calculate the font scale
        public const int ReferenceNoteMaxSize = 160;
        public const int ReferenceNoteMinSize = 72;
        private readonly ISettingsService _settingsService;
        private readonly IEnvironmentService _environmentService;
        private readonly IThemeService _themeService;
        private readonly ISynchronizationService _synchronizationService;
        private readonly IFeedbackService _feedbackService;
        private readonly IFilePickerService _filePickerService;
        private readonly IMessengerService _messengerService;
        private readonly ICloudStorageClientFactory _cloudStorageClientFactory;
        private readonly SliderStepConverter _fontSizeConverter;
        private readonly SliderStepConverter _noteMaxHeightConverter;
        private string _defaultFontFamilyLabel;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
        /// </summary>
        public SettingsViewModel(
            ISettingsService settingsService,
            ILanguageService languageService,
            IEnvironmentService environmentService,
            ISynchronizationService synchronizationService,
            IThemeService themeService,
            IFeedbackService feedbackService,
            IMessengerService messengerService,
            IFontService fontService,
            ICloudStorageClientFactory cloudStorageClientFactory,
            IFilePickerService filePickerService)
        {
            Language = languageService;
            _settingsService = settingsService;
            _environmentService = environmentService;
            _synchronizationService = synchronizationService;
            _themeService = themeService;
            _feedbackService = feedbackService;
            _messengerService = messengerService;
            _cloudStorageClientFactory = cloudStorageClientFactory;
            _filePickerService = filePickerService;
            Model = _settingsService.LoadSettingsOrDefault();
            Modifications = new ModificationDetector(() => GetModificationFingerprint(Model));

            _fontSizeConverter = new SliderStepConverter(ReferenceFontSize, 1.0);
            _noteMaxHeightConverter = new SliderStepConverter(ReferenceNoteMaxSize, 20.0);

            _defaultFontFamilyLabel = "‹" + Language["gui_font_family-default"] + "›";
            FontFamilies = fontService.ListFontFamilies();
            FontFamilies.Insert(0, _defaultFontFamilyLabel);

            EncryptionAlgorithms = new List<DropdownItemViewModel>();
            KdfAlgorithms = new List<DropdownItemViewModel>();
            FillAlgorithmList(EncryptionAlgorithms, KdfAlgorithms);

            // Initialize commands
            ChangeCloudSettingsCommand = new RelayCommand(ChangeCloudSettings);
            ClearCloudSettingsCommand = new RelayCommand(ClearCloudSettings);
            TestNewLocalizationCommand = new RelayCommand(TestNewLocalization);
        }

        /// <summary>
        /// Gets a modification detector for the note repository.
        /// </summary>
        internal ModificationDetector Modifications { get; }

        /// <summary>
        /// Gets a fingerprint of all properties which can be modified on this page. Getting the
        /// same fingerprint means that no changes where made, or that they where undone.
        /// </summary>
        /// <param name="settings">The settings model from which to create the fingerprint.</param>
        /// <returns>The fingerprint representing the current state.</returns>
        public static long GetModificationFingerprint(SettingsModel settings)
        {
            if (settings == null)
                return 0;

            return ModificationDetector.CombineHashCodes(new long[]
            {
                settings.Revision.GetHashCode(),
                string.GetHashCode(settings.FontFamily),
                settings.FontScale.GetHashCode(),
                string.GetHashCode(settings.SelectedWallpaper),
                settings.UseWallpaper.GetHashCode(),
                settings.UseSolidColorTheme.GetHashCode(),
                string.GetHashCode(settings.ColorForSolidTheme),
                settings.ThemeMode.GetHashCode(),
                settings.UseColorForAllNotesInDarkMode.GetHashCode(),
                string.GetHashCode(settings.ColorForAllNotesInDarkModeHex),
                string.GetHashCode(settings.DefaultNoteColorHex),
                settings.NoteMaxHeightScale.GetHashCode(),
                settings.KeepScreenUpDuration.GetHashCode(),
                settings.DefaultNoteInsertion.GetHashCode(),
                settings.HideClosedSafeNotes.GetHashCode(),
                settings.RememberLastTagFilter.GetHashCode(),
                string.GetHashCode(settings.SelectedEncryptionAlgorithm),
                string.GetHashCode(settings.SelectedKdfAlgorithm),
                settings.AutoSyncMode.GetHashCode(),
                settings.PreventScreenshots.GetHashCode(),
                (settings.Credentials == null).GetHashCode(),
            });
        }

        /// <inheritdoc/>
        public void OnStoringUnsavedData()
        {
            if (Modifications.IsModified())
            {
                _settingsService.TrySaveSettingsToLocalDevice(Model);
                Modifications.MemorizeCurrentState();
            }
        }

        private ILanguageService Language { get; }

        /// <summary>
        /// Initializes the list of available cloud storage services.
        /// </summary>
        /// <param name="algorithms">List to fill.</param>
        private void FillAlgorithmList(List<DropdownItemViewModel> algorithms, List<DropdownItemViewModel> kdfAlgorithms)
        {
            string recommended = string.Format(" [{0}]", Language["encryption_algorithm_recommended"]);

            algorithms.Add(new DropdownItemViewModel { Value = BouncyCastleXChaCha20.CryptoAlgorithmName, Description = "XChaCha20-Poly1305" + recommended });
            algorithms.Add(new DropdownItemViewModel { Value = BouncyCastleAesGcm.CryptoAlgorithmName, Description = "AES-256-GCM" });
            algorithms.Add(new DropdownItemViewModel { Value = BouncyCastleTwofishGcm.CryptoAlgorithmName, Description = "Twofish-256-GCM" });

            kdfAlgorithms.Add(new DropdownItemViewModel { Value = BouncyCastleArgon2.CryptoKdfName, Description = "Argon2id" + recommended });
            kdfAlgorithms.Add(new DropdownItemViewModel { Value = Pbkdf2.CryptoKdfName, Description = "PBKDF2" });
        }

        /// <summary>
        /// Gets a list of all available font family names.
        /// </summary>
        public List<string> FontFamilies { get; private set; }

        /// <summary>
        /// Gets or sets the 
        /// </summary>
        public string SelectedFontFamily
        {
            get { return Model.FontFamily ?? _defaultFontFamilyLabel; }

            set
            {
                if (value == _defaultFontFamilyLabel)
                    value = null;
                SetProperty(SelectedFontFamily, value, (v) => Model.FontFamily = v);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="SettingsModel.FontScale"/> expressed for the -3...+3 slider.
        /// </summary>
        public int FontSizeStep
        {
            get { return _fontSizeConverter.ModelFactorToSliderStep(Model.FontScale); }

            set
            {
                SetProperty(
                    _fontSizeConverter.ModelFactorToSliderStep(Model.FontScale),
                    value,
                    (v) => Model.FontScale = _fontSizeConverter.SliderStepToModelFactor(v));
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="SettingsModel.NoteMaxHeightScale"/> expressed for the -4...+4 slider.
        /// </summary>
        public int NoteMaxHeightStep
        {
            get { return _noteMaxHeightConverter.ModelFactorToSliderStep(Model.NoteMaxHeightScale); }

            set
            {
                SetProperty(
                    _noteMaxHeightConverter.ModelFactorToSliderStep(Model.NoteMaxHeightScale),
                    value,
                    (v) => Model.NoteMaxHeightScale = _noteMaxHeightConverter.SliderStepToModelFactor(v));
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="SettingsModel.KeepScreenUpDuration"/> property.
        /// </summary>
        public int KeepScreenOnDuration
        {
            get { return Model.KeepScreenUpDuration; }

            set
            {
                if (SetProperty(Model.KeepScreenUpDuration, value, (v) => Model.KeepScreenUpDuration = v))
                {
                    OnPropertyChanged(nameof(KeepScreenOnDurationTitle));
                }
            }
        }

        public bool CanKeepScreenOn
        {
            get { return _environmentService?.KeepScreenOn != null; }
        }

        public string KeepScreenOnDurationTitle
        {
            get { return Language.LoadTextFmt("keep_screen_on_duration", KeepScreenOnDuration); }
        }

        /// <summary>
        /// Gets or sets the index of the wallpaper selected by the user.
        /// </summary>
        public int SelectedWallpaperIndex 
        {
            get { return _themeService.FindWallpaperIndexOrDefault(Model.SelectedWallpaper); }
            set
            {
                string themeId = _themeService.Wallpapers[value].Id;
                SetProperty(Model.SelectedWallpaper, themeId, (string v) => Model.SelectedWallpaper = v);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the wallpaper should be used as background image.
        /// </summary>
        public bool UseWallpaper
        {
            get { return Model.UseWallpaper; }
            set { SetProperty(Model.UseWallpaper, value, (bool v) => Model.UseWallpaper = v); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the a solid color should be used instead of a
        /// background image.
        /// </summary>
        public bool UseSolidColorTheme
        {
            get { return Model.UseSolidColorTheme; }
            set { SetProperty(Model.UseSolidColorTheme, value, (bool v) => Model.UseSolidColorTheme = v); }
        }

        /// <summary>
        /// Gets or sets the solid background color for the theme background. It depends on
        /// <see cref="UseSolidColorTheme"/> whether this value is respected.
        /// </summary>
        public string ColorForSolidThemeHex
        {
            get { return Model.ColorForSolidTheme; }
            set { SetProperty(Model.ColorForSolidTheme, value, (string v) => Model.ColorForSolidTheme = v); }
        }

        /// <summary>
        /// Gets or sets the theme mode selected by the user.
        /// </summary>
        public string SelectedThemeMode
        {
            get { return Model.ThemeMode.ToString(); }

            set
            {
                if (SetProperty(Model.ThemeMode.ToString(), value, (string v) => Model.ThemeMode = (ThemeMode)Enum.Parse(typeof(ThemeMode), value)))
                {
                    _messengerService.Send(new RedrawMainPageMessage());
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the note color should be the same for all notes
        /// in dark mode.
        /// </summary>
        public bool UseColorForAllNotesInDarkMode
        {
            get { return Model.UseColorForAllNotesInDarkMode; }
            set { SetProperty(Model.UseColorForAllNotesInDarkMode, value, (bool v) => Model.UseColorForAllNotesInDarkMode = v); }
        }

        /// <summary>
        /// Gets or sets the background color for new notes. It depends on <see cref="UseColorForAllNotesInDarkMode"/>
        /// whether this value is respected.
        /// </summary>
        public string ColorForAllNotesInDarkModeHex
        {
            get { return Model.ColorForAllNotesInDarkModeHex; }
            set { SetProperty(Model.ColorForAllNotesInDarkModeHex, value, (string v) => Model.ColorForAllNotesInDarkModeHex = v); }
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
        public string DefaultNoteColorHex
        {
            get { return Model.DefaultNoteColorHex; }
            set { SetProperty(Model.DefaultNoteColorHex, value, (string v) => Model.DefaultNoteColorHex = v); }
        }

        /// <summary>
        /// Gets or sets the note insertion mode selected by the user.
        /// </summary>
        public string SelectedNoteInsertionMode
        {
            get { return Model.DefaultNoteInsertion.ToString(); }
            set { SetProperty(Model.DefaultNoteInsertion.ToString(), value, (string v) => Model.DefaultNoteInsertion = (NoteInsertionMode)Enum.Parse(typeof(NoteInsertionMode), value)); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the drawer containing the tags should be opened
        /// when showing the note repository page.
        /// </summary>
        public bool StartWithTagsOpen
        {
            get { return Model.StartWithTagsOpen; }
            set { SetProperty(Model.StartWithTagsOpen, value, (bool v) => Model.StartWithTagsOpen = v); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the last selected tag to filter the notes
        /// should be remembered across startups.
        /// </summary>
        public bool RememberLastTagFilter
        {
            get { return Model.RememberLastTagFilter; }
            set { SetProperty(Model.RememberLastTagFilter, value, (bool v) => Model.RememberLastTagFilter = v); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether notes should be hidden in the overview, if they
        /// are part of a closed safe.
        /// </summary>
        public bool HideClosedSafeNotes
        {
            get { return Model.HideClosedSafeNotes; }
            set { SetProperty(Model.HideClosedSafeNotes, value, (bool v) => Model.HideClosedSafeNotes = v); }
        }

        /// <summary>
        /// Gets a class attribute for a given background color, "note-light" for bright background
        /// colors, "note-dark" for dark background colors.
        /// </summary>
        /// <param name="backgroundColorHex">Background color of the note.</param>
        /// <returns>Html class "note-dark" if the background color is dark, otherwise "note-light".</returns>
        public string GetDarkClass(string backgroundColorHex)
        {
            Color backgroundColor = ColorExtensions.HexToColor(backgroundColorHex);
            if (backgroundColor.IsDark())
                return "note-dark";
            else
                return "note-light";
        }

        /// <summary>
        /// Gets a list of all available encryption algorithms.
        /// </summary>
        public List<DropdownItemViewModel> EncryptionAlgorithms { get; private set; }

        /// <summary>
        /// Gets or sets the encryption algorithm selected by the user.
        /// </summary>
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

            set { SetProperty(Model.SelectedEncryptionAlgorithm, value, (string v) => Model.SelectedEncryptionAlgorithm = v); }
        }

        /// <summary>
        /// Gets a list of all available kdf algorithms.
        /// </summary>
        public List<DropdownItemViewModel> KdfAlgorithms {  get; private set; }

        /// <summary>
        /// Gets or sets the encryption algorithm selected by the user.
        /// </summary>
        public string SelectedKdfAlgorithm
        {
            get
            {
                DropdownItemViewModel result = KdfAlgorithms.Find(item => item.Value == Model.SelectedKdfAlgorithm);

                // Search for the default algorithm, if no matching algorithm could be found.
                if (result == null)
                    result = KdfAlgorithms.Find(item => item.Value == SettingsModel.GetDefaultKdfAlgorithmName());
                return result.Value;
            }

            set { SetProperty(Model.SelectedKdfAlgorithm, value, (string v) => Model.SelectedKdfAlgorithm = v); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether taking screenshots should be forbidden or allowed.
        /// </summary>
        public bool PreventScreenshots
        {
            get { return Model.PreventScreenshots; }
            set
            {
                if (SetProperty(Model.PreventScreenshots, value, (bool v) => Model.PreventScreenshots = v))
                {
                    if (_environmentService.Screenshots != null)
                        _environmentService.Screenshots.PreventScreenshots = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the operating system is able to prevent screenshots.
        /// </summary>
        public bool CanPreventScreenshots
        {
            get { return _environmentService.Screenshots != null; }
        }

        /// <summary>
        /// Gets or sets the auto sync mode selected by the user.
        /// </summary>
        public string SelectedAutoSyncMode
        {
            get { return Model.AutoSyncMode.ToString(); }
            set { SetProperty(Model.AutoSyncMode.ToString(), value, (string v) => Model.AutoSyncMode = (AutoSynchronizationMode)Enum.Parse(typeof(AutoSynchronizationMode), value)); }
        }

        /// <summary>
        /// Gets the command to reset the cloud settings.
        /// </summary>
        public ICommand ClearCloudSettingsCommand { get; private set; }

        private async void ClearCloudSettings()
        {
            string title = Language.LoadText("cloud_clear_settings_desc");
            string explanation = Language.LoadText("cloud_clear_settings_expl");
            MessageBoxResult dialogResult = await _feedbackService.ShowMessageAsync(explanation, title, MessageBoxButtons.YesNoCancel, false);

            // Remove repository from online storage
            if (dialogResult == MessageBoxResult.No)
            {
                await TryDeleteCloudNoteRepository();
            }

            // Remove account from settings
            if ((dialogResult == MessageBoxResult.Yes) || (dialogResult == MessageBoxResult.No))
            {
                SetProperty(Model.Credentials, null, (v) => Model.Credentials = v, nameof(AccountSummary));
                OnPropertyChanged(nameof(ClearCloudSettingsDisabled));
                _messengerService.Send(new RedrawCurrentPageMessage());
            }
        }

        public bool ClearCloudSettingsDisabled
        {
            get { return Model.Credentials == null; }
        }

        private async Task<bool> TryDeleteCloudNoteRepository()
        {
            var credentials = Model.Credentials;
            ICloudStorageClient cloudStorageClient = _cloudStorageClientFactory.GetByKey(credentials.CloudStorageId);
            try
            {
                await cloudStorageClient.DeleteFileAsync(NoteRepositoryModel.RepositoryFileName, credentials);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the command to replace the cloud settings with new ones.
        /// </summary>
        public ICommand ChangeCloudSettingsCommand { get; private set; }

        private async void ChangeCloudSettings()
        {
            await _synchronizationService.SynchronizeManuallyChangeCloudStorage(Ioc.Instance);
        }

        public string AccountIcon
        {
            get
            {
                if (Model.Credentials != null)
                {
                    var cloudMetadata = _cloudStorageClientFactory.GetCloudStorageMetadata(Model.Credentials.CloudStorageId);
                    return cloudMetadata.AssetImageName;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets a summary of the cloud storage account.
        /// </summary>
        public MarkupString AccountSummary
        {
            get
            {
                if (Model.Credentials == null)
                {
                    return (MarkupString)Language["cloud_service_undefined"];
                }
                else
                {
                    var cloudMetadata = _cloudStorageClientFactory.GetCloudStorageMetadata(Model.Credentials.CloudStorageId);
                    List<string> lines = new List<string>();
                    lines.Add(cloudMetadata.Title);
                    lines.Add(WebUtility.HtmlEncode(Model.Credentials.Url));
                    return (MarkupString)string.Join("<br>", lines);
                }
            }
        }

        /// <summary>
        /// Gets the command to reset the cloud settings.
        /// </summary>
        public ICommand TestNewLocalizationCommand { get; private set; }

        private async void TestNewLocalization()
        {
            try
            {
                if (await _filePickerService.PickFile())
                {
                    byte[] languageFile = await _filePickerService.ReadPickedFile();
                    (Language as ILanguageTestService).OverrideWithTestResourceFile(languageFile);
                    _messengerService.Send(new RedrawCurrentPageMessage());
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Gets the wrapped model.
        /// </summary>
        internal SettingsModel Model { get; }
    }
}
