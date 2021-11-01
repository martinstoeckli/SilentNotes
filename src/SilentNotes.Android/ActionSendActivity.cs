// Copyright © 2021 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using SilentNotes.Controllers;
using SilentNotes.Models;
using SilentNotes.Services;

namespace SilentNotes.Android
{
    /// <summary>
    /// This activity handles retrieving text data from other applications.
    /// </summary>
    [Activity(Label = "SilentNotes")]
    [IntentFilter(
        new[] { Intent.ActionSend },
        Categories = new[] { Intent.CategoryDefault },
        DataMimeType = @"text/plain")]
    public class ActionSendActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Ioc.Reset();
            Startup.InitializeApplication(this, null);
            ISettingsService settingsService = Ioc.GetOrCreate<ISettingsService>();
            IRepositoryStorageService repositoryStorageService = Ioc.GetOrCreate<IRepositoryStorageService>();

            // Create new note
            NoteModel note = null;
            if (repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository) != RepositoryStorageLoadResult.InvalidRepository)
            {
                note = new NoteModel();
                note.BackgroundColorHex = settingsService.LoadSettingsOrDefault().DefaultNoteColorHex;
                note.HtmlContent = PlainTextToHtml(GetSendIntentText());
                noteRepository.Notes.Insert(0, note);

                repositoryStorageService.TrySaveRepository(noteRepository);
            }

            // Stop the activity, its job is already done
            Finish();

            // Start main activity to show new note. If later the application is stopped/restarted
            // it will be opened with the main activity instead of this ActionSend activity, this
            // avoids creating new notes each time.
            if (note != null)
            {
                Intent intent = new Intent(this, typeof(MainActivity));
                intent.PutExtra(ControllerParameters.NoteId, note.Id.ToString());
                StartActivity(intent);
            }
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