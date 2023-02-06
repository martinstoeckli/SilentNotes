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
    internal class FeedbackService : IFeedbackService
    {
        private readonly IAppContextService _appContext;
        private readonly ILanguageService _languageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackService"/> class.
        /// </summary>
        /// <param name="appContextService">A service which knows about the current main activity.</param>
        /// <param name="languageService">A language service.</param>
        public FeedbackService(IAppContextService appContextService, ILanguageService languageService)
        {
            _appContext = appContextService;
            _languageService = languageService;
        }

        /// <inheritdoc/>
        public void ShowToast(string message)
        {
            _appContext.RootActivity.RunOnUiThread(() =>
            {
                Toast.MakeText(_appContext.RootActivity, message, ToastLength.Long).Show();
            });
        }

        /// <inheritdoc/>
        public void ShowBusyIndicator(bool visible)
        {
            _appContext.RootActivity.RunOnUiThread(() =>
            {
                ProgressBar busyIndicator = _appContext.RootActivity.FindViewById<ProgressBar>(Resource.Id.busyIndicator);
                if (visible)
                    busyIndicator.Visibility = ViewStates.Visible;
                else
                    busyIndicator.Visibility = ViewStates.Gone;
            });
        }

        /// <inheritdoc/>
        public async Task<MessageBoxResult> ShowMessageAsync(string message, string title, MessageBoxButtons buttons, bool conservativeDefault)
        {
            ButtonArrangement arrangement = new ButtonArrangement(buttons, _languageService);
            AlertDialogHelper.DialogResult dialogResult = await AlertDialogHelper.ShowAsync(
                _appContext.RootActivity,
                message,
                title,
                arrangement.PrimaryButtonText,
                arrangement.SecondaryButtonText,
                arrangement.CloseButtonText);

            return arrangement.ToMessageBoxResult(dialogResult);
        }

        private class ButtonArrangement
        {
            public ButtonArrangement(MessageBoxButtons buttons, ILanguageService languageService)
            {
                switch (buttons)
                {
                    case MessageBoxButtons.Ok:
                        CloseButton = MessageBoxResult.Ok;
                        CloseButtonText = languageService.LoadText("ok");
                        Back = MessageBoxResult.Cancel;
                        break;
                    case MessageBoxButtons.ContinueCancel:
                        PrimaryButton = MessageBoxResult.Continue;
                        PrimaryButtonText = languageService.LoadText("continue");
                        CloseButton = MessageBoxResult.Cancel;
                        CloseButtonText = languageService.LoadText("cancel");
                        Back = MessageBoxResult.Cancel;
                        break;
                    case MessageBoxButtons.YesNoCancel:
                        PrimaryButton = MessageBoxResult.Yes;
                        PrimaryButtonText = languageService.LoadText("yes");
                        CloseButton = MessageBoxResult.No;
                        CloseButtonText = languageService.LoadText("no");
                        SecondaryButton = MessageBoxResult.Cancel;
                        SecondaryButtonText = languageService.LoadText("cancel");
                        Back = MessageBoxResult.Cancel;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(buttons));
                }
            }

            public MessageBoxResult PrimaryButton { get; }

            public MessageBoxResult SecondaryButton { get; }

            public MessageBoxResult CloseButton { get; }

            public string PrimaryButtonText { get; }

            public string SecondaryButtonText { get; }

            public string CloseButtonText { get; }

            public MessageBoxResult Back { get; }

            public MessageBoxResult ToMessageBoxResult(AlertDialogHelper.DialogResult dialogResult)
            {
                switch (dialogResult)
                {
                    case AlertDialogHelper.DialogResult.None:
                        return Back;
                    case AlertDialogHelper.DialogResult.Positive:
                        return PrimaryButton;
                    case AlertDialogHelper.DialogResult.Negative:
                        return CloseButton;
                    case AlertDialogHelper.DialogResult.Neutral:
                        return SecondaryButton;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dialogResult));
                }
            }
        }
    }
}