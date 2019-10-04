// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
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
            _rootActivity.RunOnUiThread(() =>
            {
                ProgressBar busyIndicator = _rootActivity.FindViewById<ProgressBar>(Resource.Id.busyIndicator);
                if (visible)
                    busyIndicator.Visibility = ViewStates.Visible;
                else
                    busyIndicator.Visibility = ViewStates.Gone;
            });
        }

        /// <inheritdoc/>
        public async Task<MessageBoxResult> ShowMessageAsync(string message, string title, MessageBoxButtons buttons)
        {
            ButtonArrangement arrangement = ButtonArrangement.Create(buttons, _languageService);
            bool dialogResult = await AlertDialogHelper.ShowAsync(
                _rootActivity,
                message,
                title,
                arrangement.PositiveButtonText,
                arrangement.NegativeButtonText);

            return arrangement.ToMessageBoxResult(dialogResult);
        }

        private class ButtonArrangement
        {
            public static ButtonArrangement Create(MessageBoxButtons buttons, ILanguageService languageService)
            {
                ButtonArrangement result = new ButtonArrangement();
                switch (buttons)
                {
                    case MessageBoxButtons.Ok:
                        result.PositiveButton = MessageBoxResult.Ok;
                        result.PositiveButtonText = languageService.LoadText("ok");
                        break;
                    case MessageBoxButtons.ContinueCancel:
                        result.PositiveButton = MessageBoxResult.Continue;
                        result.PositiveButtonText = languageService.LoadText("continue");
                        result.NegativeButton = MessageBoxResult.Cancel;
                        result.NegativeButtonText = languageService.LoadText("cancel");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(buttons));
                }
                return result;
            }

            public MessageBoxResult PositiveButton { get; set; }

            public MessageBoxResult NegativeButton { get; set; }

            public string PositiveButtonText { get; set; }

            public string NegativeButtonText { get; set; }

            public MessageBoxResult ToMessageBoxResult(bool dialogResult)
            {
                if (dialogResult)
                    return PositiveButton;
                else
                    return NegativeButton;
            }
        }
    }
}