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
            }));
        }

        /// <inheritdoc/>
        public async Task ShowMessageAsync(string message, string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                title = "SilentNotes";
            }
            ContentDialog dialog = new ContentDialog()
            {
                Title = title,
                Content = message,
                CloseButtonText = _languageService.LoadText("ok"),
            };
            await dialog.ShowAsync();
        }
    }
}
