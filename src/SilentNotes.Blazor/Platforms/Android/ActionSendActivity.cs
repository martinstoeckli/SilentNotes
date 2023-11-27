// Copyright © 2021 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;

namespace SilentNotes
{
    /// <summary>
    /// This activity handles retrieving text data from other applications.
    /// </summary>
    [Activity(Label = "SilentNotes", NoHistory = true, Exported = true)]
    [IntentFilter(
        new[] { Intent.ActionSend },
        Categories = new[] { Intent.CategoryDefault },
        DataMimeType = @"text/plain")]
    public class ActionSendActivity : Activity
    {
        public const string NoteHtmlParam = "ActionSendNoteHtml";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Start main activity to create new note. If later the application is stopped/restarted
            // it will be opened with the main activity instead of this ActionSend activity, this
            // avoids creating new notes each time.
            Intent mainActivityIntent = new Intent(this, typeof(MainActivity));
            mainActivityIntent.PutExtra(NoteHtmlParam, PlainTextToHtml(GetSendIntentText()));
            StartActivity(mainActivityIntent);

            // Stop the activity, its job is just to start the main activity with parameters.
            Finish();
        }

        private string GetSendIntentText()
        {
            return Intent.GetStringExtra(Intent.ExtraText);
        }

        /// <summary>
        /// Escapes special characters which would be potentially dangerous inside HTML, and puts
        /// each new line into a paragraph section.
        /// </summary>
        /// <param name="plainText">Plain text.</param>
        /// <returns>Html content.</returns>
        private static string PlainTextToHtml(string plainText)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(plainText))
            {
                string encodedText = System.Net.WebUtility.HtmlEncode(plainText);
                sb.Append("<p>");
                sb.Append(encodedText);
                sb.Append("</p>");
                sb.Replace("\r\n", "\n");
                sb.Replace("\n\n", "\n");
                sb.Replace("\n", "</p><p>");
            }
            return sb.ToString();
        }
    }
}