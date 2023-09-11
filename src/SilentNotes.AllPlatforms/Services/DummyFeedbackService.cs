// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;
using MudBlazor;

namespace SilentNotes.Services
{
    /// <summary>
    /// Dummy implementation of the <see cref="IFeedbackService"/> interface. This implementation
    /// provides no functionallity and can be used when all feedback should be ignored.
    /// </summary>
    public class DummyFeedbackService : IFeedbackService
    {
        /// <inheritdoc/>
        public bool IsBusyIndicatorVisible { get; set; }

        /// <inheritdoc/>
        public Task<MessageBoxResult> ShowMessageAsync(string message, string title, MessageBoxButtons buttons, bool conservativeDefault)
        {
            MessageBoxResult result;
            switch (buttons)
            {
                case MessageBoxButtons.Ok:
                    result = MessageBoxResult.Ok;
                    break;
                case MessageBoxButtons.ContinueCancel:
                    result = MessageBoxResult.Continue;
                    break;
                default:
                    result = MessageBoxResult.Ok;
                    break;
            }
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public void ShowToast(string message, Severity severity = Severity.Normal)
        {
        }
    }
}
