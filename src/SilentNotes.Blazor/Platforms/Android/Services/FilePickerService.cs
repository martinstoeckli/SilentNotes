// Copyright © 2021 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using AndroidX.DocumentFile.Provider;
using SilentNotes.Services;
using Uri = Android.Net.Uri;

namespace SilentNotes.Platforms.Services
{
    /// <summary>
    /// Implementation of the <see cref="IFilePickerService"/> interface for the Android platform.
    /// </summary>
    internal class FilePickerService : IFilePickerService
    {
        private readonly IAppContextService _appContext;
        private readonly IActivityResultAwaiter _activityResultAwaiter;
        private Uri _pickedUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderPickerService"/> class.
        /// </summary>
        /// <param name="appContextService">A service which knows about the current main activity.</param>
        /// <param name="activityResultAwaiter">An activity result awaiter.</param>
        public FilePickerService(IAppContextService appContextService, IActivityResultAwaiter activityResultAwaiter)
        {
            _appContext = appContextService;
            _activityResultAwaiter = activityResultAwaiter;
        }

        /// <inheritdoc/>
        public async Task<bool> PickFile(IEnumerable<string> extensions = null)
        {
            Intent filePickerIntent = new Intent(Intent.ActionOpenDocument);
            filePickerIntent.AddFlags(ActivityFlags.GrantReadUriPermission);
            filePickerIntent.AddCategory(Intent.CategoryOpenable);
            filePickerIntent.SetType("application/*");

            var activityResult = await _activityResultAwaiter.StartActivityAndWaitForResult(
                _appContext.RootActivity, filePickerIntent);

            if (activityResult.ResultCode == Result.Ok)
            {
                _pickedUri = activityResult.Data?.Data;
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public async Task<byte[]> ReadPickedFile()
        {
            if (_pickedUri == null)
                throw new Exception("Pick a file first before it can be read.");

            DocumentFile file = DocumentFile.FromSingleUri(_appContext.RootActivity, _pickedUri);
            using (Stream stream = _appContext.RootActivity.ContentResolver.OpenInputStream(file.Uri))
            {
                byte[] result = new byte[stream.Length];
                await stream.ReadAsync(result, 0, (int)stream.Length);
                return result;
            }
        }
    }
}