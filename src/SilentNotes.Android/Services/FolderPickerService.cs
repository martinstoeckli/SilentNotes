// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Android.App;
using Android.Content;
using SilentNotes.Services;
using System.Threading.Tasks;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// Implementation of the <see cref="IFolderPickerService"/> interface for the Android platform.
    /// </summary>
    public class FolderPickerService : IFolderPickerService
    {
        private readonly ActivityResultAwaiter _activityResultAwaiter;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderPickerService"/> class.
        /// </summary>
        /// <param name="activityResultAwaiter">Can start activities and get their result.</param>
        public FolderPickerService(ActivityResultAwaiter activityResultAwaiter)
        {
            _activityResultAwaiter = activityResultAwaiter;
        }

        /// <inheritdoc/>
        public async Task<bool> PickFolder()
        {
            Intent folderPickerIntent = new Intent(Intent.ActionOpenDocumentTree);
            folderPickerIntent.AddFlags(ActivityFlags.GrantWriteUriPermission);

            ActivityResult activityResult = await _activityResultAwaiter.StartActivityAndWaitForResult(folderPickerIntent);
            return activityResult.ResultCode == Result.Ok;
        }

        /// <inheritdoc/>
        public async Task<bool> TrySaveFileToPickedFolder(string relativeFilePath, byte[] content)
        {
            return await Task.FromResult(true);
        }
    }
}