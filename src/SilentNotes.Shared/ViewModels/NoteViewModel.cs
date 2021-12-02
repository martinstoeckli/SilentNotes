// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Input;
using SilentNotes.Controllers;
using SilentNotes.Crypto;
using SilentNotes.HtmlView;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.StoryBoards.PullPushStory;
using SilentNotes.Workers;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present a single note.
    /// </summary>
    public class NoteViewModel : ViewModelBase
    {
        private static TimeAgo _timeAgo;
        private readonly IRepositoryStorageService _repositoryService;
        private readonly IFeedbackService _feedbackService;
        private readonly ISettingsService _settingsService;
        private readonly ICryptor _cryptor;
        private readonly SafeListModel _safes;
        private readonly IList<string> _allDistinctAndSortedTags;
        private SearchableHtmlConverter _searchableTextConverter;
        protected string _unlockedContent;
        private string _searchableContent;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteViewModel"/> class.
        /// </summary>
        public NoteViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IThemeService themeService,
            IBaseUrlService webviewBaseUrl,
            SearchableHtmlConverter searchableTextConverter,
            IRepositoryStorageService repositoryService,
            IFeedbackService feedbackService,
            ISettingsService settingsService,
            ICryptor cryptor,
            SafeListModel safes,
            IList<string> allDistinctAndSortedTags,
            NoteModel noteFromRepository)
            : base(navigationService, languageService, svgIconService, themeService, webviewBaseUrl)
        {
            _repositoryService = repositoryService;
            _feedbackService = feedbackService;
            _settingsService = settingsService;
            _searchableTextConverter = searchableTextConverter;
            _cryptor = cryptor;
            _safes = safes;
            _allDistinctAndSortedTags = allDistinctAndSortedTags;
            MarkSearchableContentAsDirty();
            PushNoteToOnlineStorageCommand = new RelayCommand(PushNoteToOnlineStorage);
            PullNoteFromOnlineStorageCommand = new RelayCommand(PullNoteFromOnlineStorage);
            ToggleShoppingModeCommand = new RelayCommand(ToggleShoppingMode);
            GoBackCommand = new RelayCommand(GoBack);
            AddTagCommand = new RelayCommand<string>(AddTag);
            DeleteTagCommand = new RelayCommand<string>(DeleteTag);

            Model = noteFromRepository;
            _unlockedContent = IsInSafe ? UnlockIfSafeOpen(Model.HtmlContent) : Model.HtmlContent;
        }

        /// <summary>
        /// Gets the unique id of the note.
        /// </summary>
        public Guid Id
        {
            get { return Model.Id; }
        }

        /// <summary>
        /// Gets the type of the note as css class.
        /// </summary>
        public string CssClassNoteType
        {
            get { return Model.NoteType.ToString().ToLowerInvariant(); }
        }

        /// <summary>
        /// Gets a searchable representation of the <see cref="UnlockedHtmlContent"/>. This searchable
        /// text is generated on demand only, to mark it as dirty use <see cref="MarkSearchableContentAsDirty"/>.
        /// </summary>
        public string SearchableContent
        {
            get
            {
                bool searchableContentIsDirty = _searchableContent == null;
                if (searchableContentIsDirty && UnlockedHtmlContent != null)
                {
                    SearchableHtmlConverter converter = _searchableTextConverter ?? (_searchableTextConverter = new SearchableHtmlConverter());
                    converter.TryConvertHtml(UnlockedHtmlContent, out _searchableContent);
                }
                return _searchableContent;
            }
        }

        /// <summary>
        /// Marks the <see cref="SearchableContent"/> as dirty, so it can be recreated the next
        /// time it is used.
        /// </summary>
        private void MarkSearchableContentAsDirty()
        {
            _searchableContent = null;
        }

        /// <summary>
        /// Gets or sets the Html content of the note.
        /// </summary>
        public string UnlockedHtmlContent
        {
            get { return _unlockedContent; }

            set
            {
                if (value == null)
                    value = string.Empty;
                if (ChangeProperty(ref _unlockedContent, value, true))
                {
                    MarkSearchableContentAsDirty();
                    Model.RefreshModifiedAt();
                    OnPropertyChanged(nameof(PrettyTimeAgo));
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="UnlockedHtmlContent"/>, but very long notes are truncated, so they
        /// can be processed faster by the HTML view in an overview of notes.
        /// </summary>
        /// <returns>Truncated html content of the note.</returns>
        public string GetShortenedUnlockedHtmlContent()
        {
            string result = _unlockedContent;
            if (result != null)
            {
                // Create a short version for large notes, with only the first part of the note.
                // This is a performance improvement if there are large notes in the repository.
                HtmlShortener shortener = new HtmlShortener();
                shortener.WantedLength = 600; // Should be enough even for settings with
                shortener.WantedTagNumber = 20; // small font and very height notes.

                result = shortener.Shorten(result);
            }
            return result;
        }

        [VueDataBinding(VueBindingMode.OneWayToView)]
        public List<string> Tags
        {
            get { return Model.Tags; }
        }

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand AddTagCommand { get; private set; }

        /// <inheritdoc/>
        private void AddTag(string value)
        {
            if (Model.Tags.Contains(value, StringComparer.InvariantCultureIgnoreCase))
                return;

            Model.Tags.Add(value);
            Model.Tags.Sort(StringComparer.InvariantCultureIgnoreCase);
            Model.RefreshModifiedAt();
            Modified = true;
            OnPropertyChanged(nameof(Tags));
            OnPropertyChanged(nameof(TagSuggestions));
        }

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand DeleteTagCommand { get; private set; }

        /// <inheritdoc/>
        private void DeleteTag(string value)
        {
            int tagIndex = Model.Tags.FindIndex(tag => string.Equals(value, tag, StringComparison.InvariantCultureIgnoreCase));
            if (tagIndex == -1)
                return;

            Model.Tags.RemoveAt(tagIndex);
            Model.RefreshModifiedAt();
            Modified = true;
            OnPropertyChanged(nameof(Tags));
            OnPropertyChanged(nameof(TagSuggestions));
        }

        /// <summary>
        /// Gets a list of tags, which are used in other notes, but not in this note. They can be
        /// used to make suggestions about already used tags.
        /// </summary>
        [VueDataBinding(VueBindingMode.OneWayToView)]
        public IEnumerable<string> TagSuggestions
        {
            get { return _allDistinctAndSortedTags.Where(tag => !Tags.Contains(tag, StringComparer.InvariantCultureIgnoreCase)); }
        }

        /// <summary>
        /// Gets or sets the background color as hex string, e.g. #ff0000
        /// </summary>
        [VueDataBinding(VueBindingMode.TwoWay)]
        public string BackgroundColorHex
        {
            get
            {
                string result = Model.BackgroundColorHex;
                if (Theme.DarkMode)
                {
                    SettingsModel settings = _settingsService.LoadSettingsOrDefault();
                    if (settings.UseColorForAllNotesInDarkMode)
                        result = settings.ColorForAllNotesInDarkModeHex;
                }
                return result;
            }

            set
            {
                if (Theme.DarkMode)
                {
                    SettingsModel settings = _settingsService.LoadSettingsOrDefault();
                    if (settings.UseColorForAllNotesInDarkMode)
                    {
                        OnPropertyChanged(nameof(BackgroundColorHex)); // Redraw unchanged color (Vue binding)
                        _feedbackService.ShowToast(Language.LoadText("gui_theme_color_cannot_change"));
                        return;
                    }
                }

                if (ChangePropertyIndirect(() => Model.BackgroundColorHex, (string v) => Model.BackgroundColorHex = v, value, true))
                {
                    Model.RefreshModifiedAt();
                    OnPropertyChanged(nameof(IsDark));
                }
            }
        }

        /// <summary>
        /// Gets or sets the border as CSS string, e.g. '1px solid black'
        /// </summary>
        [VueDataBinding(VueBindingMode.TwoWay)]
        public string Border
        {
            get
            {
                return Model.Border;
            }
            set
            {
                ChangePropertyIndirect(() => Model.Border, (string v) => Model.Border = v, value, true);
            }
        }

        /// <summary>
        /// Gets a list of available background colors.
        /// </summary>
        public List<string> BackgroundColorsHex
        {
            get { return _settingsService.LoadSettingsOrDefault().NoteColorsHex; }
        }

        /// <summary>
        /// Gets a value indicating whether the background color of the note is a dark color or not.
        /// </summary>
        [VueDataBinding(VueBindingMode.OneWayToView)]
        public bool IsDark
        {
            get { return ColorExtensions.HexToColor(BackgroundColorHex).IsDark(); }
        }

        /// <summary>
        /// Gets the dark class for a given background color, depending of whether the background
        /// color is a light or a dark color.
        /// </summary>
        /// <param name="backgroundColorHex">Background color of the note. If this parameter is
        /// null, the <see cref="BackgroundColorHex"/> of the note itself is used.</param>
        /// <returns>Html class "dark" if the background color is dark, otherwise an empty string.</returns>
        public string GetDarkClass(string backgroundColorHex = null)
        {
            if (string.IsNullOrEmpty(backgroundColorHex))
                backgroundColorHex = BackgroundColorHex;
            Color backgroundColor = ColorExtensions.HexToColor(backgroundColorHex);
            if (backgroundColor.IsDark())
                return "dark";
            else
                return string.Empty;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the note is deleted and is part of the
        /// recycling bin.
        /// </summary>
        public bool InRecyclingBin
        {
            get { return Model.InRecyclingBin; }

            set
            {
                if (ChangePropertyIndirect(() => Model.InRecyclingBin, (bool v) => Model.InRecyclingBin = v, value, true))
                    Model.RefreshModifiedAt();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the note is in a safe. Notes in a safe can be locked
        /// or unlocked.
        /// </summary>
        public bool IsInSafe
        {
            get { return Model.SafeId != null; }
        }

        /// <summary>
        /// Gets a value indicating whether the note is locked. A locked note is part of a safe and
        /// is not yet decrypted.
        /// </summary>
        public bool IsLocked
        {
            get { return IsInSafe && (UnlockedHtmlContent == null); }
        }

        /// <summary>
        /// Decrypts the note, if the belonging safe is open.
        /// </summary>
        /// <returns>Decrypted note content, or null if the safe is closed.</returns>
        private string UnlockIfSafeOpen(string lockedContent)
        {
            SafeModel safe = _safes.FindById(Model.SafeId);
            if ((safe != null) && safe.IsOpen)
            {
                byte[] binaryContent = CryptoUtils.Base64StringToBytes(lockedContent);
                byte[] unlockedContent = _cryptor.Decrypt(binaryContent, safe.Key);
                return CryptoUtils.BytesToString(unlockedContent);
            }
            return null;
        }

        /// <summary>
        /// Gets a value indicating whether the note is unlocked. An unlocked note is part of a safe
        /// and is decrypted.
        /// </summary>
        private bool IsUnlocked
        {
            get { return IsInSafe && (UnlockedHtmlContent != null); }
        }

        /// <summary>
        /// Encrpyts the unlocked content with the key of the assigned safe.
        /// </summary>
        /// <param name="unlockedContent">Content to encrypt.</param>
        /// <returns>Encrypted content.</returns>
        public string Lock(string unlockedContent)
        {
            string encryptionAlgorithm = _settingsService.LoadSettingsOrDefault().SelectedEncryptionAlgorithm;
            SafeModel safe = _safes.FindById(Model.SafeId);
            byte[] binaryContent = CryptoUtils.StringToBytes(unlockedContent);
            byte[] lockedContent = _cryptor.Encrypt(binaryContent, safe.Key, encryptionAlgorithm, null);
            return CryptoUtils.BytesToBase64String(lockedContent);
        }

        /// <inheritdoc />
        public override void OnStoringUnsavedData()
        {
            if (Modified)
            {
                if (IsUnlocked)
                    Model.HtmlContent = Lock(_unlockedContent);
                else
                    Model.HtmlContent = XmlUtils.SanitizeXmlString(_unlockedContent);

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

        /// <inheritdoc/>
        private void GoBack()
        {
            _navigationService.Navigate(new Navigation(
                ControllerNames.NoteRepository, ControllerParameters.NoteId, Model.Id.ToString()));
        }

        /// <inheritdoc/>
        public override void OnGoBackPressed(out bool handled)
        {
            handled = true;
            GoBack();
        }

        /// <summary>
        /// Gets the command which can overwrite the local note with the note from the online-storage.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand PullNoteFromOnlineStorageCommand { get; private set; }

        private async void PullNoteFromOnlineStorage()
        {
            MessageBoxResult dialogResult = await _feedbackService.ShowMessageAsync(Language["pushpull_pull_confirmation"], Language["note_pull_from_server"], MessageBoxButtons.ContinueCancel, false);
            if (dialogResult != MessageBoxResult.Continue)
                return;

            _feedbackService.ShowBusyIndicator(true);
            try
            {
                OnStoringUnsavedData();
                PullPushStoryBoard storyBoard = new PullPushStoryBoard(Model.Id, PullPushDirection.PullFromServer);
                await storyBoard.Start();
            }
            finally
            {
                _feedbackService.ShowBusyIndicator(false);
            }

            // Refresh view
            if (Model.InRecyclingBin)
                _navigationService.Navigate(new Navigation(ControllerNames.RecycleBin));
            else
                _navigationService.Navigate(new Navigation(ControllerNames.Note, ControllerParameters.NoteId, Model.Id.ToString()));
        }

        /// <summary>
        /// Gets the command which can overwrite the note of the online-storage with the locale note.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand PushNoteToOnlineStorageCommand { get; private set; }

        private async void PushNoteToOnlineStorage()
        {
            MessageBoxResult dialogResult = await _feedbackService.ShowMessageAsync(Language["pushpull_push_confirmation"], Language["note_push_to_server"], MessageBoxButtons.ContinueCancel, false);
            if (dialogResult != MessageBoxResult.Continue)
                return;

            _feedbackService.ShowBusyIndicator(true);
            try
            {
                OnStoringUnsavedData();
                PullPushStoryBoard storyBoard = new PullPushStoryBoard(Model.Id, PullPushDirection.PushToServer);
                await storyBoard.Start();
            }
            finally
            {
                _feedbackService.ShowBusyIndicator(false);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the synchronization mode is set to <see cref="AutoSynchronizationMode.Never"/>.
        /// </summary>
        public bool ShowManualSynchronization
        {
            get
            {
                SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
                return settings != null ? settings.AutoSyncMode == AutoSynchronizationMode.Never : true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the shopping mode is active or inactive
        /// <see cref="NoteModel.ShoppingModeActive"/>
        /// This property does not call <see cref="NoteModel.RefreshModifiedAt"/>, because it is
        /// not seen as changed content, so switching should not overwrite other recent changes.
        /// </summary>
        [VueDataBinding(VueBindingMode.TwoWay)]
        public bool ShoppingModeActive
        {
            get { return Model.ShoppingModeActive; }
            set { ChangePropertyIndirect(() => Model.ShoppingModeActive, (v) => Model.ShoppingModeActive = v, value, true); }
        }

        /// <summary>
        /// Command which toggles the <see cref="ShoppingModeActive"/> property.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand ToggleShoppingModeCommand { get; private set; }

        private void ToggleShoppingMode()
        {
            ShoppingModeActive = !ShoppingModeActive;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the the note is pinned.
        /// <see cref="NoteModel.IsPinned"/>
        /// This property does not call <see cref="NoteModel.RefreshModifiedAt"/>, because it is
        /// not seen as changed content, so switching should not overwrite other recent changes.
        /// </summary>
        [VueDataBinding(VueBindingMode.TwoWay)]
        public bool IsPinned
        {
            get { return Model.IsPinned; }
            set
            {
                ChangePropertyIndirect(() => Model.IsPinned, (v) => Model.IsPinned = v, value, true);
                if (Model.IsPinned)//TODO: delete, just for debugging
                {
                    //BackgroundColorHex = "#ffff00";
                    Border = "5px solid red";
                }
                else
                {
                    //BackgroundColorHex = "#ffffff";
                    Border = "none";
                }
                PinnedChanged = !PinnedChanged; //if you click the button back, it shouldn't register as changed
            }
        }

        /// <summary>
        /// Command which toggles the <see cref="IsPinned"/> property.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand TogglePinnedCommand { get; private set; }

        private void TogglePinned()
        {
            IsPinned = !IsPinned;
            //PinnedChanged = !PinnedChanged; //if you click the button back, it shouldn't register as changed
        }

        /// <summary>
        /// Value indicating whether the <see cref="IsPinned"/> changed by button
        /// </summary>
        public bool PinnedChanged
        {
            get { return Model.PinnedChanged; }
            set
            {
                ChangePropertyIndirect(
                       () => Model.PinnedChanged,
                       (v) => Model.PinnedChanged = v,
                       value,
                       true
                   );
            }
        }

        [VueDataBinding(VueBindingMode.OneWayToView)]
        public string PrettyTimeAgo
        {
            get
            {
                string prettyTime = GetOrCreateTimeAgo().PrettyPrint(Model.ModifiedAt, DateTime.UtcNow);
                return Language.LoadTextFmt("modified_at", prettyTime);
            }
        }

        private TimeAgo GetOrCreateTimeAgo()
        {
            if (_timeAgo == null)
            {
                var localization = new TimeAgo.Localization
                {
                    Today = Language.LoadText("today"),
                    Yesterday = Language.LoadText("yesterday"),
                    NumberOfDaysAgo = Language.LoadText("days_ago"),
                    NumberOfWeeksAgo = Language.LoadText("weeks_ago"),
                    NumberOfMonthsAgo = Language.LoadText("months_ago"),
                    NumberOfYearsAgo = Language.LoadText("years_ago"),
                };
                _timeAgo = new TimeAgo(localization);
            }
            return _timeAgo;
        }

        /// <summary>
        /// Gets the base font size [px] of the notes, from which the relative sizes are derrived.
        /// </summary>
        public string NoteBaseFontSize
        {
            get
            {
                double defaultBaseFontSize = 15.0; // Default size for scale 1.0
                SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
                double fontSize = settings != null ? defaultBaseFontSize * settings.FontScale : defaultBaseFontSize;
                return FloatingPointUtils.FormatInvariant(fontSize);
            }
        }

        /// <summary>
        /// Gets the wrapped model.
        /// </summary>
        internal NoteModel Model { get; private set; }
    }
}