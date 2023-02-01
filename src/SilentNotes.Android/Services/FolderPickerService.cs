// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using AndroidX.DocumentFile.Provider;
using Java.Net;
using SilentNotes.Services;
using Uri = Android.Net.Uri;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// Implementation of the <see cref="IFolderPickerService"/> interface for the Android platform.
    /// </summary>
    internal class FolderPickerService : IFolderPickerService
    {
        private readonly IAppContextService _appContext;
        private Uri _pickedUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderPickerService"/> class.
        /// </summary>
        /// <param name="appContextService">A service which knows about the current main activity.</param>
        public FolderPickerService(IAppContextService appContextService)
        {
            _appContext = appContextService;
        }

        /// <inheritdoc/>
        public async Task<bool> PickFolder()
        {
            Intent folderPickerIntent = new Intent(Intent.ActionOpenDocumentTree);
            folderPickerIntent.AddFlags(ActivityFlags.GrantReadUriPermission);
            folderPickerIntent.AddFlags(ActivityFlags.GrantWriteUriPermission);
            folderPickerIntent.PutExtra("android.content.extra.SHOW_ADVANCED", true);

            using (ActivityResultAwaiter awaiter = new ActivityResultAwaiter(_appContext.RootActivity))
            {
                var activityResult = await awaiter.StartActivityAndWaitForResult(folderPickerIntent);
                if (activityResult.ResultCode == Result.Ok)
                {
                    _pickedUri = activityResult.Data?.Data;
                    return true;
                }
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
                DocumentFile folder = DocumentFile.FromTreeUri(_appContext.RootActivity, _pickedUri);
                string mimeType = URLConnection.GuessContentTypeFromName(fileName);
                DocumentFile file = folder.CreateFile(mimeType, fileName);
                using (Stream stream = _appContext.RootActivity.ContentResolver.OpenOutputStream(file.Uri, "w"))
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