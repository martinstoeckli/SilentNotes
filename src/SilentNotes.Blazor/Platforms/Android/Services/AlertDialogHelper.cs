// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;

namespace SilentNotes.Platforms.Services
{
    /// <summary>
    /// Helper class to show message dialogs.
    /// </summary>
    public class AlertDialogHelper : Java.Lang.Object, IDialogInterfaceOnDismissListener
    {
        private readonly Context _context;
        private readonly string _message;
        private readonly string _title;
        private readonly string _positiveButtonCaption;
        private readonly string _negativeButtonCaption;
        private readonly string _neutralButtonCaption;
        private ManualResetEvent _waitHandle;
        private DialogResult _dialogResult;

        /// <summary>
        /// Shows a message dialog.
        /// </summary>
        /// <param name="context">The context of the Android app.</param>
        /// <param name="message">The message to show, or null.</param>
        /// <param name="title">The title to show, or null.</param>
        /// <param name="positiveButton">The text to show on the ok/yes button.</param>
        /// <param name="neutralButton">The text to show on the third button.</param>
        /// <param name="negativeButton">The text to show on the cancel/no button.</param>
        /// <returns>Returns true if the user pressed the positive button, otherwise false.</returns>
        public static async Task<DialogResult> ShowAsync(Context context, string message, string title, string positiveButton, string neutralButton, string negativeButton)
        {
            return await new AlertDialogHelper(context, message, title, positiveButton, neutralButton, negativeButton).ShowAsync();
        }

        private AlertDialogHelper(Context context, string message, string title, string positiveButton, string neutralButton, string negativeButton)
        {
            _context = context;
            _message = message;
            _title = title;
            _positiveButtonCaption = positiveButton;
            _neutralButtonCaption = neutralButton;
            _negativeButtonCaption = negativeButton;
        }

        private async Task<DialogResult> ShowAsync()
        {
            _waitHandle = new ManualResetEvent(false);
            AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(_context);

            if (!string.IsNullOrEmpty(_title))
                dialogBuilder.SetTitle(_title);
            if (!string.IsNullOrEmpty(_message))
                dialogBuilder.SetMessage(_message);
            if (!string.IsNullOrEmpty(_positiveButtonCaption))
                dialogBuilder.SetPositiveButton(_positiveButtonCaption, OnPositiveClick);
            if (!string.IsNullOrEmpty(_negativeButtonCaption))
                dialogBuilder.SetNegativeButton(_negativeButtonCaption, OnNegativeClick);
            if (!string.IsNullOrEmpty(_neutralButtonCaption))
                dialogBuilder.SetNeutralButton(_neutralButtonCaption, OnNeutralClick);
            dialogBuilder.SetOnDismissListener(this);
            dialogBuilder.Show();

            Task<DialogResult> dialogTask = new Task<DialogResult>(() =>
            {
                _waitHandle.WaitOne();
                return _dialogResult;
            });
            dialogTask.Start();
            return await dialogTask;
        }

        private void OnPositiveClick(object sender, DialogClickEventArgs e)
        {
            _dialogResult = DialogResult.Positive;
            _waitHandle.Set();
        }

        private void OnNegativeClick(object sender, DialogClickEventArgs e)
        {
            _dialogResult = DialogResult.Negative;
            _waitHandle.Set();
        }

        private void OnNeutralClick(object sender, DialogClickEventArgs e)
        {
            _dialogResult = DialogResult.Neutral;
            _waitHandle.Set();
        }

        /// <inheritdoc/>
        public void OnDismiss(IDialogInterface dialog)
        {
            _dialogResult = DialogResult.None;
            _waitHandle.Set();
        }

        /// <summary>
        /// Enumeration of all known buttons which can be pressed in the messagebox.
        /// </summary>
        public enum DialogResult
        {
            /// <summary>Undefined result.</summary>
            None,

            /// <summary>The positive button was pressed.</summary>
            Positive,

            /// <summary>The negative button was pressed.</summary>
            Negative,

            /// <summary>The neutral button was pressed.</summary>
            Neutral,
        }
    }
}