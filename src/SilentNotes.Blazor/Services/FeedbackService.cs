// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MudBlazor;
using SilentNotes.Views;

namespace SilentNotes.Services
{
    internal class FeedbackService : IFeedbackService
    {
        private readonly IDialogService _dialogService;
        private readonly ILanguageService _languageService;

        public FeedbackService(IDialogService dialogService, ILanguageService languageService)
        {
            _dialogService = dialogService;
            _languageService = languageService;
        }

        public void ShowToast(string message)
        {
        }

        public void ShowBusyIndicator(bool visible)
        {
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string message, string title, MessageBoxButtons buttons, bool conservativeDefault)
        {
            buttons = MessageBoxButtons.ContinueCancel;

            ButtonArrangement arrangement = new ButtonArrangement(buttons, _languageService);

            var parameters = new DialogParameters
            {
                ["Title"] = title,
                ["Message"] = message,
                ["MarkupMessage"] = null,
                ["CancelText"] = arrangement.CloseButtonText,
                ["NoText"] = arrangement.SecondaryButtonText,
                ["YesText"] = arrangement.PrimaryButtonText,
                ["ConservativeDefault"] = conservativeDefault,
            };
            DialogOptions options = null; // Global options are defined in MainLayout.razor
            IDialogReference dialogReference = await _dialogService.ShowAsync<CustomMessageBox>(title, parameters, options);
            DialogResult dialogResult = await dialogReference.Result;

            return arrangement.ToMessageBoxResult(dialogResult);
        }

        private class ButtonArrangement
        {
            public ButtonArrangement(MessageBoxButtons buttons, ILanguageService languageService)
            {
                switch (buttons)
                {
                    case MessageBoxButtons.Ok:
                        PrimaryButton = MessageBoxResult.Ok;
                        PrimaryButtonText = languageService.LoadText("ok");
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
                        SecondaryButton = MessageBoxResult.No;
                        SecondaryButtonText = languageService.LoadText("no");
                        CloseButton = MessageBoxResult.Cancel;
                        CloseButtonText = languageService.LoadText("cancel");
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

            public MessageBoxResult ToMessageBoxResult(DialogResult dialogResult)
            {
                if (dialogResult.Canceled)
                {
                    return Back;
                }
                else
                {
                    return (bool)dialogResult.Data ? PrimaryButton : SecondaryButton;
                }
            }
        }
    }
}
