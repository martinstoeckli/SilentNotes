// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using SilentNotes.Services;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace SilentNotes.UWP.Services
{
    /// <summary>
    /// Implementation of the <see cref="IFeedbackService"/> interface for the UWP platform.
    /// </summary>
    public class FeedbackService : IFeedbackService
    {
        private readonly MainPage _mainPage;
        private readonly ILanguageService _languageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackService"/> class.
        /// </summary>
        /// <param name="mainPage">The main page of the aplication.</param>
        /// <param name="languageService">A language service.</param>
        public FeedbackService(MainPage mainPage, ILanguageService languageService)
        {
            _mainPage = mainPage;
            _languageService = languageService;
        }

        /// <inheritdoc/>
        public void ShowToast(string message)
        {
            Task.Run(async () => await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Set new text
                TextBlock toastText = _mainPage.FindName("ToastText") as TextBlock;
                toastText.Text = message;

                // Start fade-in fade-out animation
                Storyboard toastStoryboard = _mainPage.Resources["ToastFadeInOut"] as Storyboard;
                toastStoryboard.Begin();
            }));
        }

        /// <inheritdoc/>
        public void ShowBusyIndicator(bool visible)
        {
            Task.Run(async () => await _mainPage.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ProgressRing busyIndicator = _mainPage.FindName("BusyIndicator") as ProgressRing;
                busyIndicator.IsActive = visible;
            })).Wait(100);
        }

        /// <inheritdoc/>
        public async Task<MessageBoxResult> ShowMessageAsync(string message, string title, MessageBoxButtons buttons, bool conservativeDefault)
        {
            ButtonArrangement arrangement = new ButtonArrangement(buttons, conservativeDefault, _languageService);
            ContentDialog dialog = new ContentDialog()
            {
                Title = title,
                Content = message,
                CloseButtonText = arrangement.CloseButtonText,
                SecondaryButtonText = arrangement.SecondaryButtonText,
                PrimaryButtonText = arrangement.PrimaryButtonText,
                DefaultButton = arrangement.DefaultButton,
            };

            ContentDialogResult result = await dialog.ShowAsync();
            return arrangement.ToMessageBoxResult(result);
        }

        private class ButtonArrangement
        {
            public ButtonArrangement(MessageBoxButtons buttons, bool conservativeDefault, ILanguageService languageService)
            {
                switch (buttons)
                {
                    case MessageBoxButtons.Ok:
                        PrimaryButton = MessageBoxResult.Ok;
                        PrimaryButtonText = languageService.LoadText("ok");
                        CloseButton = MessageBoxResult.Cancel;
                        DefaultButton = ContentDialogButton.Primary;
                        break;
                    case MessageBoxButtons.ContinueCancel:
                        PrimaryButton = MessageBoxResult.Continue;
                        PrimaryButtonText = languageService.LoadText("continue");
                        CloseButton = MessageBoxResult.Cancel;
                        CloseButtonText = languageService.LoadText("cancel");
                        DefaultButton = conservativeDefault ? ContentDialogButton.Close : ContentDialogButton.Primary;
                        break;
                    case MessageBoxButtons.YesNoCancel:
                        PrimaryButton = MessageBoxResult.Yes;
                        PrimaryButtonText = languageService.LoadText("yes");
                        SecondaryButton = MessageBoxResult.No;
                        SecondaryButtonText = languageService.LoadText("no");
                        CloseButton = MessageBoxResult.Cancel;
                        CloseButtonText = languageService.LoadText("cancel");
                        DefaultButton = conservativeDefault ? ContentDialogButton.Close : ContentDialogButton.Primary;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(buttons));
                }

                // The dialog won't accept null strings, turn them to empty strings
                PrimaryButtonText = PrimaryButtonText ?? string.Empty;
                SecondaryButtonText = SecondaryButtonText ?? string.Empty;
                CloseButtonText = CloseButtonText ?? string.Empty;
            }

            public MessageBoxResult PrimaryButton { get; }

            public MessageBoxResult SecondaryButton { get; }

            public MessageBoxResult CloseButton { get; }

            public string PrimaryButtonText { get; }

            public string SecondaryButtonText { get; }

            public string CloseButtonText { get; }

            public ContentDialogButton DefaultButton { get; }

            public MessageBoxResult ToMessageBoxResult(ContentDialogResult dialogResult)
            {
                switch (dialogResult)
                {
                    case ContentDialogResult.None: // Close button, back, esc
                        return CloseButton;
                    case ContentDialogResult.Primary:
                        return PrimaryButton;
                    case ContentDialogResult.Secondary:
                        return SecondaryButton;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dialogResult));
                }
            }
        }
    }
}
