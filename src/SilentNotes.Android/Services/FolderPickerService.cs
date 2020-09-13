// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Android.App;
using Android.Content;
using AndroidX.DocumentFile.Provider;
using Java.Net;
using SilentNotes.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using Uri = Android.Net.Uri;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// Implementation of the <see cref="IFolderPickerService"/> interface for the Android platform.
    /// </summary>
    public class FolderPickerService : IFolderPickerService
    {
        private readonly Context _context;
        private readonly ActivityResultAwaiter _activityResultAwaiter;
        private Uri _pickedUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderPickerService"/> class.
        /// </summary>
        /// <param name="activityResultAwaiter">Can start activities and get their result.</param>
        public FolderPickerService(Context context, ActivityResultAwaiter activityResultAwaiter)
        {
            _context = context;
            _activityResultAwaiter = activityResultAwaiter;
        }

        /// <inheritdoc/>
        public async Task<bool> PickFolder()
        {
            Intent folderPickerIntent = new Intent(Intent.ActionOpenDocumentTree);
            folderPickerIntent.AddFlags(ActivityFlags.GrantReadUriPermission);
            folderPickerIntent.AddFlags(ActivityFlags.GrantWriteUriPermission);
            folderPickerIntent.PutExtra("android.content.extra.SHOW_ADVANCED", true);

            var activityResult = await _activityResultAwaiter.StartActivityAndWaitForResult(folderPickerIntent);
            if (activityResult.ResultCode == Result.Ok)
            {
                _pickedUri = activityResult.Data?.Data;
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public async Task<bool> TrySaveFileToPickedFolder(string fileName, byte[] content)
        {
            if (_pickedUri == null)
                return false;

            try
            {
                DocumentFile folder = DocumentFile.FromTreeUri(_context, _pickedUri);
                string mimeType = URLConnection.GuessContentTypeFromName(fileName);
                DocumentFile file = folder.CreateFile(mimeType, fileName);
                using (Stream stream = _context.ContentResolver.OpenOutputStream(file.Uri, "w"))
                {
                    await stream.WriteAsync(content, 0, content.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}