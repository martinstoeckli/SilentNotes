// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.ComponentModel;
using System.Threading.Tasks;
using MudBlazor;

namespace SilentNotes.Services
{
    /// <summary>
    /// The feedback service offers platform independend messages and other feedback.
    /// </summary>
    public interface IFeedbackService
    {
        /// <summary>
        /// Shows a toast popup message.
        /// Implementing classes should ensure that this method can also be called from a non-ui
        /// thread.
        /// </summary>
        /// <remarks>
        /// Implementing classes should ensure that this method can be called from the gui-thread
        /// as well as from a non-ui thread.
        /// thread.
        /// </remarks>
        /// <param name="message">Message to display.</param>
        /// <param name="severity">The severity defines the color and the icon of the toast.</param>
        void ShowToast(string message, Severity severity = Severity.Normal);

        /// <summary>
        /// Shows an indicator that the applcation is busy or hides it (e.g. hourglass).
        /// </summary>
        /// <remarks>
        /// The idea to use IDisposable to automatically hide the busy indicator didn't work out,
        /// because it cannot be guaranteed that the same activity is active when the indicator
        /// should be removed.
        /// </remarks>
        /// <param name="visible">Indicates whether the busy indicator should be shown or hidden.</param>
        void ShowBusyIndicator(bool visible);

        /// <summary>
        /// Displays information to the user.
        /// </summary>
        /// <param name="message">The message to be shown to the user.</param>
        /// <param name="title">The title of the dialog box. This may be null.</param>
        /// <param name="buttons">Determines the buttons to show, and the possible dialog results.</param>
        /// <param name="conservativeDefault">Sets the non destructive cancel button as default.</param>
        /// <returns>The pressed button.</returns>
        Task<MessageBoxResult> ShowMessageAsync(string message, string title, MessageBoxButtons buttons, bool conservativeDefault);
    }

    /// <summary>
    /// Enumeration of all known button combinations for the messagebox.
    /// </summary>
    public enum MessageBoxButtons
    {
        /// <summary>Shows a simple ok button</summary>
        Ok,

        /// <summary>Shows a continue and a cancel button</summary>
        ContinueCancel,


        /// <summary>Shows a yes, a no and a cancel button</summary>
        YesNoCancel,
    }

    /// <summary>
    /// Enumeration of all known buttons which can be pressed in the messagebox.
    /// </summary>
    public enum MessageBoxResult
    {
        /// <summary>The ok button was pressed.</summary>
        Ok,

        /// <summary>The cancel button was pressed or the dialog was closed without pressing a button.</summary>
        Cancel,

        /// <summary>The continue button was pressed.</summary>
        Continue,

        /// <summary>The yes button was pressed.</summary>
        Yes,

        /// <summary>The no button was pressed.</summary>
        No,
    }
}
