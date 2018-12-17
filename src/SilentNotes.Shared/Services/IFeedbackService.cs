// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;

namespace SilentNotes.Services
{
    /// <summary>
    /// The feedback service offers platform independend messages and other feedback.
    /// </summary>
    public interface IFeedbackService
    {
        /// <summary>
        /// Shows a toast popup message.
        /// </summary>
        /// <param name="message">Message to display.</param>
        void ShowToast(string message);

        /// <summary>
        /// Shows an indicator that the applcation is busy (e.g. hourglass). The indicator will be
        /// shown as long as the returned IDisposable object is alive, this object should be used
        /// inside a "using" if possible. A nested calling is allowed.
        /// </summary>
        /// <returns>A disposable object, which keeps the busy indicator running.</returns>
        IDisposable ShowBusyIndicator();

        /// <summary>
        /// Displays information to the user. The dialog box will have only one button with the text "OK".
        /// </summary>
        /// <param name="message">The message to be shown to the user.</param>
        /// <param name="title">The title of the dialog box. This may be null.</param>
        /// <returns>A Task allowing this async method to be awaited.</returns>
        Task ShowMessageAsync(string message, string title);
    }
}
