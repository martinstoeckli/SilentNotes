// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using Microsoft.UI.Xaml.Controls;
using MudBlazor;
using SilentNotes.Services;

namespace SilentNotes.Platforms.Services
{
    /// <summary>
    /// Implementation of the <see cref="IFeedbackService"/> interface for the Windows platform.
    /// </summary>
    internal class FeedbackService : FeedbackServiceBase
    {
        /// <inheritdoc/>
        public FeedbackService(ISnackbar snackbar, ILanguageService languageService)
            : base(snackbar, languageService)
        {
        }

        /// <summary>
        /// Displays information to the user.
        /// </summary>
        /// <remarks>
        /// The decision to use the native ContentDialog was made because a message can also be
        /// started from a background thread after a page change, when the new page is not yet
        /// ready to access the JSRuntime.
        /// </remarks>
        /// <param name="message">The message to be shown to the user.</param>
        /// <param name="title">The title of the dialog box. This may be null.</param>
        /// <param name="buttons">Determines the buttons to show, and the possible dialog results.</param>
        /// <param name="conservativeDefault">Sets the non destructive cancel button as default.</param>
        /// <returns>The pressed button.</returns>
        public override async Task<MessageBoxResult> ShowMessageAsync(string message, string title, MessageBoxButtons buttons, bool conservativeDefault)
        {
            // Switch to the UI thread if necessary
            return await MainThread.InvokeOnMainThreadAsync(async () =>
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
                    XamlRoot = ((MauiWinUIWindow)App.Current.Windows[0].Handler.PlatformView).Content.XamlRoot, // required since WinUI3
                };
                ContentDialogResult result = await dialog.ShowAsync();
                return arrangement.ToMessageBoxResult(result);
            });
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
