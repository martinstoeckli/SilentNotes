// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;
using Android.App;
using Android.Views;
using Android.Widget;
using SilentNotes.Services;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// Implementation of the <see cref="IFeedbackService"/> interface for the Android platform.
    /// </summary>
    public class FeedbackService : IFeedbackService
    {
        private readonly Activity _rootActivity;
        private readonly ILanguageService _languageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackService"/> class.
        /// </summary>
        /// <param name="rootActivity">The context of the Android app.</param>
        /// <param name="languageService">A language service.</param>
        public FeedbackService(Activity rootActivity, ILanguageService languageService)
        {
            _rootActivity = rootActivity;
            _languageService = languageService;
        }

        /// <inheritdoc/>
        public void ShowToast(string message)
        {
            _rootActivity.RunOnUiThread(() =>
            {
                Toast.MakeText(_rootActivity, message, ToastLength.Long).Show();
            });
        }

        /// <inheritdoc/>
        public void ShowBusyIndicator(bool visible)
        {
            ProgressBar busyIndicator = _rootActivity.FindViewById<ProgressBar>(Resource.Id.busyIndicator);
            if (visible)
                busyIndicator.Visibility = ViewStates.Visible;
            else
                busyIndicator.Visibility = ViewStates.Gone;
        }

        /// <inheritdoc/>
        public async Task ShowMessageAsync(string message, string title)
        {
            if (string.IsNullOrEmpty(title))
                title = "SilentNotes";
            await AlertDialogHelper.ShowAsync(_rootActivity, message, title, _languageService.LoadText("ok"), null);
        }
    }
}