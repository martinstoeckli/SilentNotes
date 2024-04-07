// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="IFeedbackService"/> interface.
    /// </summary>
    internal abstract class FeedbackServiceBase : IFeedbackService
    {
        //protected readonly IDialogService _dialogService;
        protected readonly ISnackbar _snackbar;
        protected readonly ILanguageService _languageService;
        private bool _isBusyIndicatorVisible;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackServiceBase"/>.
        /// </summary>
        /// <param name="snackbar">The Mudblazor snackbar, to show toasts.</param>
        /// <param name="languageService">Language service to get localized textes.</param>
        public FeedbackServiceBase(ISnackbar snackbar, ILanguageService languageService)
        {
            _snackbar = snackbar;
            _languageService = languageService;
            _isBusyIndicatorVisible = false;
        }

        /// <inheritdoc/>
        public void ShowToast(string message, Severity severity = Severity.Normal)
        {
            _snackbar.Add(message, severity, config => { config.HideIcon = true; });
        }

        /// <inheritdoc/>
        public bool IsBusyIndicatorVisible
        {
            get { return _isBusyIndicatorVisible; }
        }

        /// <inheritdoc/>
        public void SetBusyIndicatorVisible(bool value, bool refreshGui)
        {
            if (value != _isBusyIndicatorVisible)
            {
                _isBusyIndicatorVisible = value;
                if (refreshGui)
                    WeakReferenceMessenger.Default.Send<RedrawMainPageMessage>();
            }
        }

        /// <inheritdoc/>
        public abstract Task<MessageBoxResult> ShowMessageAsync(string message, string title, MessageBoxButtons buttons, bool conservativeDefault);

        //public virtual async Task<MessageBoxResult> ShowMessageAsync(string message, string title, MessageBoxButtons buttons, bool conservativeDefault)
        //{
        //    ButtonArrangement arrangement = new ButtonArrangement(buttons, _languageService);
        //    message = message.Replace("\r\n", "<br />");
        //    message = message.Replace("\n", "<br />");

        //    var parameters = new DialogParameters
        //    {
        //        ["Title"] = title,
        //        ["Message"] = null,
        //        ["MarkupMessage"] = (MarkupString)message,
        //        ["CancelText"] = arrangement.CloseButtonText,
        //        ["NoText"] = arrangement.SecondaryButtonText,
        //        ["YesText"] = arrangement.PrimaryButtonText,
        //        ["ConservativeDefault"] = conservativeDefault,
        //    };
        //    DialogOptions options = null; // Global options are defined in MainLayout.razor
        //    IDialogReference dialogReference = await _dialogService.ShowAsync<CustomMessageBox>(title, parameters, options);
        //    DialogResult dialogResult = await dialogReference.Result;

        //    return arrangement.ToMessageBoxResult(dialogResult);
        //}

        ///// <summary>
        ///// Inherit from <see cref="MudMessageBox"/> to extend it.
        ///// </summary>
        //private class CustomMessageBox : MudMessageBox
        //{
        //    /// <summary>
        //    /// Gets or sets a parameter indicating whether the enter key is executing the primary
        //    /// action or not. A value "false" (default) will execute the primary action, "true" will
        //    /// set the focus on the cancel button.
        //    /// </summary>
        //    [Parameter]
        //    public bool ConservativeDefault { get; set; }

        //    protected override void OnInitialized()
        //    {
        //        base.OnInitialized();

        //        // Take focus from the primary action (which is the last button)
        //        if (ConservativeDefault)
        //            UserAttributes.Add("DefaultFocus", DefaultFocus.FirstChild);
        //    }
        //}

        ///// <summary>
        ///// Map the buttons to the parameters of the mud blazor dialog.
        ///// </summary>
        //private class ButtonArrangement
        //{
        //    public ButtonArrangement(MessageBoxButtons buttons, ILanguageService languageService)
        //    {
        //        switch (buttons)
        //        {
        //            case MessageBoxButtons.Ok:
        //                PrimaryButton = MessageBoxResult.Ok;
        //                PrimaryButtonText = languageService.LoadText("ok");
        //                Back = MessageBoxResult.Cancel;
        //                break;
        //            case MessageBoxButtons.ContinueCancel:
        //                PrimaryButton = MessageBoxResult.Continue;
        //                PrimaryButtonText = languageService.LoadText("continue");
        //                CloseButton = MessageBoxResult.Cancel;
        //                CloseButtonText = languageService.LoadText("cancel");
        //                Back = MessageBoxResult.Cancel;
        //                break;
        //            case MessageBoxButtons.YesNoCancel:
        //                PrimaryButton = MessageBoxResult.Yes;
        //                PrimaryButtonText = languageService.LoadText("yes");
        //                SecondaryButton = MessageBoxResult.No;
        //                SecondaryButtonText = languageService.LoadText("no");
        //                CloseButton = MessageBoxResult.Cancel;
        //                CloseButtonText = languageService.LoadText("cancel");
        //                Back = MessageBoxResult.Cancel;
        //                break;
        //            default:
        //                throw new ArgumentOutOfRangeException(nameof(buttons));
        //        }
        //    }

        //    public MessageBoxResult PrimaryButton { get; }

        //    public MessageBoxResult SecondaryButton { get; }

        //    public MessageBoxResult CloseButton { get; }

        //    public string PrimaryButtonText { get; }

        //    public string SecondaryButtonText { get; }

        //    public string CloseButtonText { get; }

        //    public MessageBoxResult Back { get; }

        //    public MessageBoxResult ToMessageBoxResult(DialogResult dialogResult)
        //    {
        //        if (dialogResult.Canceled)
        //        {
        //            return MessageBoxResult.Cancel;
        //        }
        //        else
        //        {
        //            return (bool)dialogResult.Data ? PrimaryButton : SecondaryButton;
        //        }
        //    }
        //}
    }
}
