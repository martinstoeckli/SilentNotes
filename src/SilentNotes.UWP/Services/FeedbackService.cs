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
        public async Task<MessageBoxResult> ShowMessageAsync(string message, string title, MessageBoxButtons buttons)
        {
            ButtonArrangement arrangement = ButtonArrangement.Create(buttons, _languageService);
            ContentDialog dialog = new ContentDialog()
            {
                Title = title,
                Content = message,
                CloseButtonText = arrangement.CloseButtonText,
                PrimaryButtonText = arrangement.PrimaryButtonText,
            };

            ContentDialogResult result = await dialog.ShowAsync();
            return arrangement.ToMessageBoxResult(result);
        }

        private class ButtonArrangement
        {
            public static ButtonArrangement Create(MessageBoxButtons buttons, ILanguageService languageService)
            {
                ButtonArrangement result = new ButtonArrangement();
                switch (buttons)
                {
                    case MessageBoxButtons.Ok:
                        result.CloseButton = MessageBoxResult.Ok;
                        result.CloseButtonText = languageService.LoadText("ok");
                        break;
                    case MessageBoxButtons.ContinueCancel:
                        result.PrimaryButton = MessageBoxResult.Continue;
                        result.PrimaryButtonText = languageService.LoadText("continue");
                        result.CloseButton = MessageBoxResult.Cancel;
                        result.CloseButtonText = languageService.LoadText("cancel");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(buttons));
                }

                // The dialog won't accept null strings, turn them to empty strings
                result.PrimaryButtonText = result.PrimaryButtonText ?? string.Empty;
                result.CloseButtonText = result.CloseButtonText ?? string.Empty;
                return result;
            }

            public MessageBoxResult PrimaryButton { get; set; }

            public MessageBoxResult CloseButton { get; set; }

            public string PrimaryButtonText { get; set; }

            public string CloseButtonText { get; set; }

            public MessageBoxResult ToMessageBoxResult(ContentDialogResult dialogResult)
            {
                switch (dialogResult)
                {
                    case ContentDialogResult.None:
                        return CloseButton;
                    case ContentDialogResult.Primary:
                        return PrimaryButton;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dialogResult));
                }
            }
        }
    }
}
