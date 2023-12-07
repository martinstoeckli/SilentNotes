// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SilentNotes.Crypto;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Stories.PullPushStory;
using SilentNotes.Workers;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present a single note.
    /// </summary>
    public class NoteViewModel : NoteViewModelReadOnly
    {
        private static TimeAgo _timeAgo;
        private readonly INavigationService _navigationService;
        private readonly IRepositoryStorageService _repositoryService;
        private readonly IFeedbackService _feedbackService;
        private readonly IEnvironmentService _environmentService;
        private readonly INativeBrowserService _nativeBrowserService;
        private readonly IList<string> _allDistinctAndSortedTags;
        private bool _isKeepScreenOnActive;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteViewModel"/> class.
        /// </summary>
        public NoteViewModel(
            NoteModel model,
            SearchableHtmlConverter searchableTextConverter,
            INavigationService navigationService,
            ILanguageService languageService,
            IThemeService themeService,
            IRepositoryStorageService repositoryService,
            IFeedbackService feedbackService,
            ISettingsService settingsService,
            IEnvironmentService environmentService,
            INativeBrowserService nativeBrowserService,
            ICryptor cryptor,
            SafeListModel safes,
            IList<string> allDistinctAndSortedTags)
            : base(model, searchableTextConverter, themeService, settingsService, cryptor, safes)
        {
            Language = languageService;
            _navigationService = navigationService;
            _repositoryService = repositoryService;
            _feedbackService = feedbackService;
            _environmentService = environmentService;
            _nativeBrowserService = nativeBrowserService;
            _allDistinctAndSortedTags = allDistinctAndSortedTags;

            TogglePinnedCommand = new RelayCommand(TogglePinned);
            ShowInfoCommand = new RelayCommand(ShowInfo);
            KeepScreenOnCommand = new RelayCommand(KeepScreenOn);
            OpenLinkCommand = new RelayCommand<string>(OpenLink);
            AddTagCommand = new RelayCommand<string>(AddTag);
            DeleteTagCommand = new RelayCommand<string>(DeleteTag);
            PushNoteToOnlineStorageCommand = new AsyncRelayCommand(PushNoteToOnlineStorage);
            PullNoteFromOnlineStorageCommand = new AsyncRelayCommand(PullNoteFromOnlineStorage);

            Modifications = new NoteModificationDetector(() => GetModificationFingerprint(), () => IsPinned);
            Modifications.MemorizeCurrentState();
            KeepScreenOnActive = false; // is always canceled when closing
        }

        private ILanguageService Language { get; }

        /// <summary>
        /// Gets a modification detector for the note.
        /// </summary>
        private NoteModificationDetector Modifications { get; }

        /// <summary>
        /// Gets a fingerprint of the note which can be edited on this page.
        /// </summary>
        /// <returns>A fingerprint representing the state of the note.</returns>
        public long GetModificationFingerprint()
        {
            return ModificationDetector.CombineHashCodes(new long[]
            {
                IsPinned.GetHashCode(),
                string.GetHashCode(UnlockedHtmlContent),
                string.GetHashCode(BackgroundColorHex),
                ShoppingModeActive.GetHashCode(),
                ModificationDetector.CombineHashCodes(
                    Model.Tags.Select(tag => (long)string.GetHashCode(tag))),
            });
        }

        /// <inheritdoc/>
        public new string UnlockedHtmlContent
        {
            get { return base.UnlockedHtmlContent; }

            set
            {
                if (value == null)
                    value = string.Empty;
                if (SetProperty(ref _unlockedContent, value))
                {
                    MarkSearchableContentAsDirty();
                }
            }
        }

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        public ICommand AddTagCommand { get; private set; }

        /// <inheritdoc/>
        private void AddTag(string value)
        {
            if (Model.Tags.Contains(value, StringComparer.InvariantCultureIgnoreCase))
                return;

            // Use case sensitive tag from suggestions if available
            string existingTag = TagSuggestions.FirstOrDefault(item => item.Equals(value, StringComparison.InvariantCultureIgnoreCase));
            if (existingTag != null)
                value = existingTag;

            Model.Tags.Add(value);
            Model.Tags.Sort(StringComparer.InvariantCultureIgnoreCase);
            OnPropertyChanged(nameof(Tags));
            OnPropertyChanged(nameof(TagSuggestions));
        }

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        public ICommand DeleteTagCommand { get; private set; }

        /// <inheritdoc/>
        private void DeleteTag(string value)
        {
            if (ShoppingModeActive)
                return;

            int tagIndex = Model.Tags.FindIndex(tag => string.Equals(value, tag, StringComparison.InvariantCultureIgnoreCase));
            if (tagIndex == -1)
                return;

            Model.Tags.RemoveAt(tagIndex);
            OnPropertyChanged(nameof(Tags));
            OnPropertyChanged(nameof(TagSuggestions));
        }

        /// <summary>
        /// Gets a list of tags, which are used in other notes, but not in this note. They can be
        /// used to make suggestions about already used tags.
        /// </summary>
        public IEnumerable<string> TagSuggestions
        {
            get { return _allDistinctAndSortedTags.Where(tag => !Tags.Contains(tag, StringComparer.InvariantCultureIgnoreCase)); }
        }

        /// <inheritdoc/>
        public new string BackgroundColorHex
        {
            get { return base.BackgroundColorHex; }

            set
            {
                if (_themeService.IsDarkMode)
                {
                    SettingsModel settings = _settingsService.LoadSettingsOrDefault();
                    if (settings.UseColorForAllNotesInDarkMode)
                    {
                        OnPropertyChanged(nameof(BackgroundColorHex)); // Redraw unchanged color (Vue binding)
                        _feedbackService.ShowToast(Language.LoadText("gui_theme_color_cannot_change"));
                        return;
                    }
                }

                SetProperty(Model.BackgroundColorHex, value, (string v) => Model.BackgroundColorHex = v);
            }
        }

        /// <summary>
        /// Gets a list of available background colors.
        /// </summary>
        public List<string> BackgroundColorsHex
        {
            get { return _settingsService.LoadSettingsOrDefault().NoteColorsHex; }
        }

        /// <inheritdoc />
        public void OnStoringUnsavedData(StoreUnsavedDataMessage message)
        {
            if (Modifications.IsModified())
            {
                Model.RefreshModifiedAt();

                if (IsUnlocked)
                    Model.HtmlContent = Lock(_unlockedContent);
                else
                    Model.HtmlContent = XmlUtils.SanitizeXmlString(_unlockedContent);

                _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);

                if (Modifications.IsPinnedChanged)
                {
                    RepositionNoteBecausePinStateChanged(noteRepository);
                }

                _repositoryService.TrySaveRepository(noteRepository);
                Modifications.MemorizeCurrentState();
            }

            // Reset the KeepScreenOn. The OnClosingPage() is not called when pausing on Android,
            // so we do it here whenever the page closes or goes into pause mode.
            if ((message.Sender == MessageSender.ApplicationEventHandler) || message.Sender == MessageSender.NavigationManager)
            {
                KeepScreenOnActive = false;
                _environmentService.KeepScreenOn?.Stop();
            }
        }

        /// <summary>
        /// Handles moving of the note based on <see cref="IsPinned"/> property.
        /// </summary>
        /// <param name="repository"></param>
        private void RepositionNoteBecausePinStateChanged(NoteRepositoryModel repository)
        {
            var originalPosition = repository.Notes.IndexOf(Model);

            if (Model.IsPinned)
            {
                // the note got pinned, move it to the top
                repository.Notes.Remove(Model);
                repository.Notes.Insert(0, Model);
            }
            else
            {
                // the note got unpinned, move it to the end of pinned notes
                int firstUnpinnedNoteIndex = repository.Notes.IndexOf(
                     repository.Notes.FirstOrDefault(x => x.IsPinned == false && x.Id != Model.Id));

                if (firstUnpinnedNoteIndex == -1)
                {
                    // there's no unpinned note, move to last position
                    repository.Notes.Remove(Model);
                    repository.Notes.Add(Model);
                }
                else
                {
                    firstUnpinnedNoteIndex--; // needs to account for removing the current note

                    repository.Notes.Remove(Model);
                    repository.Notes.Insert(firstUnpinnedNoteIndex, Model);
                }
            }

            if (originalPosition != repository.Notes.IndexOf(Model))
            {
                repository.RefreshOrderModifiedAt();
            }
        }

        /// <summary>
        /// Gets the command which can overwrite the local note with the note from the online-storage.
        /// </summary>
        public ICommand PullNoteFromOnlineStorageCommand { get; private set; }

        private async Task PullNoteFromOnlineStorage()
        {
            MessageBoxResult dialogResult = await _feedbackService.ShowMessageAsync(Language["pushpull_pull_confirmation"], Language["note_pull_from_server"], MessageBoxButtons.ContinueCancel, false);
            if (dialogResult != MessageBoxResult.Continue)
                return;

            _feedbackService.SetBusyIndicatorVisible(true, true);
            try
            {
                OnStoringUnsavedData(new StoreUnsavedDataMessage(MessageSender.ViewModel));
                var storyModel = new PullPushStoryModel(Model.Id, PullPushDirection.PullFromServer);
                ExistsCloudRepositoryStep story = new ExistsCloudRepositoryStep();
                await story.RunStory(storyModel, Ioc.Instance, Stories.StoryMode.Toasts);
            }
            finally
            {
                bool refreshNecessary = false; // No refresh, because navigation is ahead anyway
                _feedbackService.SetBusyIndicatorVisible(false, refreshNecessary);
            }

            // Refresh view
            if (Model.InRecyclingBin)
                _navigationService.NavigateTo(RouteNames.RecycleBin);
            else
                _navigationService.NavigateReload();
        }

        /// <summary>
        /// Gets the command which can overwrite the note of the online-storage with the locale note.
        /// </summary>
        public ICommand PushNoteToOnlineStorageCommand { get; private set; }

        private async Task PushNoteToOnlineStorage()
        {
            MessageBoxResult dialogResult = await _feedbackService.ShowMessageAsync(Language["pushpull_push_confirmation"], Language["note_push_to_server"], MessageBoxButtons.ContinueCancel, false);
            if (dialogResult != MessageBoxResult.Continue)
                return;

            _feedbackService.SetBusyIndicatorVisible(true, true);
            try
            {
                OnStoringUnsavedData(new StoreUnsavedDataMessage(MessageSender.ViewModel));
                var storyModel = new PullPushStoryModel(Model.Id, PullPushDirection.PushToServer);
                ExistsCloudRepositoryStep story = new ExistsCloudRepositoryStep();
                await story.RunStory(storyModel, Ioc.Instance, Stories.StoryMode.Toasts);
            }
            finally
            {
                _feedbackService.SetBusyIndicatorVisible(false, true);
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
        /// </summary>
        public bool ShoppingModeActive
        {
            get { return Model.ShoppingModeActive; }
            set { SetProperty(Model.ShoppingModeActive, value, (v) => Model.ShoppingModeActive = v); }
        }

        /// <summary>
        /// Command which toggles the <see cref="NoteViewModelReadOnly.IsPinned"/> property.
        /// </summary>
        public ICommand TogglePinnedCommand { get; private set; }

        private void TogglePinned()
        {
            IsPinned = !IsPinned;
        }

        /// <summary>
        /// Gets the command which shows the info dialog.
        /// </summary>
        public ICommand ShowInfoCommand { get; private set; }

        private void ShowInfo()
        {
            OnStoringUnsavedData(new StoreUnsavedDataMessage(MessageSender.ViewModel));
            StringBuilder sb = new StringBuilder();

            string creationDate = Language.FormatDateTime(Model.CreatedAt.ToLocalTime(), "d");
            sb.AppendLine(Language.LoadTextFmt("created_at", creationDate));
            sb.AppendLine();

            string modificationDate = Language.FormatDateTime(Model.ModifiedAt.ToLocalTime(), "g");
            sb.Append(Language.LoadTextFmt("modified_at", modificationDate));

            string prettyTime = GetOrCreateTimeAgo().PrettyPrint(Model.ModifiedAt, DateTime.UtcNow);
            sb.Append(" (").Append(prettyTime).Append(")");

            _feedbackService.ShowMessageAsync(sb.ToString(), Language.LoadText("note_show_info"), MessageBoxButtons.Ok, true);
        }

        /// <summary>
        /// Gets the command to open the selected link.
        /// </summary>
        public ICommand OpenLinkCommand { get; private set; }

        private void OpenLink(string link)
        {
            if (WebviewUtils.IsExternalUri(link))
                _nativeBrowserService.OpenWebsite(link);
        }

        /// <summary>
        /// Gets the command which can keep the screen open, or prevents the app from going to sleep.
        /// </summary>
        public ICommand KeepScreenOnCommand { get; private set; }

        private void KeepScreenOn()
        {
            SettingsModel settings = _settingsService.LoadSettingsOrDefault();
            KeepScreenOnActive = true;
            _environmentService?.KeepScreenOn?.Start(TimeSpan.FromMinutes(settings.KeepScreenUpDuration));
        }

        /// <summary>
        /// Gets a value indicating whether the OS supports/needs the <see cref="IKeepScreenOn"/>
        /// functionallity.
        /// </summary>
        public bool CanKeepScreenOn
        {
            get { return _environmentService?.KeepScreenOn != null; }
        }

        /// <summary>
        /// Gets the title for the menu item of the <see cref="IKeepScreenOn"/> function, which
        /// shows the current duration from the settings.
        /// </summary>
        public string KeepScreenOnTitle
        {
            get
            {
                SettingsModel settings = _settingsService?.LoadSettingsOrDefault();
                return Language.LoadTextFmt("keep_screen_on", settings.KeepScreenUpDuration);
            }
        }

        /// <summary>
        /// Gets or sets a bindable value indicating whether the <see cref="IKeepScreenOn"/>
        /// function is currently active (waiting on the timer to stop).
        /// </summary>
        public bool KeepScreenOnActive
        {
            get { return _isKeepScreenOnActive; }
            set { SetProperty(ref _isKeepScreenOnActive, value); }
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
        /// Extends the <see cref="ModificationDetector"/> with specifics of the note page.
        /// </summary>
        private class NoteModificationDetector : ModificationDetector
        {
            private readonly Func<bool> _isPinnedProvider;
            private bool _originalIsPinned;

            public NoteModificationDetector(Func<long?> fingerPrintProvider, Func<bool> isPinnedProvider)
                : base(fingerPrintProvider, false)
            {
                _isPinnedProvider = isPinnedProvider;
            }

            public override void MemorizeCurrentState()
            {
                base.MemorizeCurrentState();
                _originalIsPinned = _isPinnedProvider();
            }

            public bool IsPinnedChanged
            {
                get { return _originalIsPinned != _isPinnedProvider(); }
            }
        }
    }
}