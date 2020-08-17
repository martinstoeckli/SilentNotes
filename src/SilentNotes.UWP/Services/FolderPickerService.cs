// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SilentNotes.UWP.Services
{
    /// <summary>
    /// Implementation of the <see cref="IFolderPickerService"/> interface for the UWP platform.
    /// </summary>
    public class FolderPickerService : IFolderPickerService
    {
        private Windows.Storage.IStorageFolder _pickedFolder;

        /// <inheritdoc/>
        public async Task<bool> PickFolder()
        {
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");

            _pickedFolder = await folderPicker.PickSingleFolderAsync();
            return _pickedFolder != null;

            //if (folder != null)
            //{
            //    // Application now has read/write access to all contents in the picked folder
            //    // (including other sub-folder contents)
            //    Windows.Storage.AccessCache.StorageApplicationPermissions.
            //    FutureAccessList.AddOrReplace("PickedFolderToken", folder);
            //}
            //else
            //{
            //}
        }

        /// <inheritdoc/>
        public async Task<bool> TrySaveFileToFolder(string relativeFilePath, byte[] content)
        {
            if (_pickedFolder == null)
                return false;

            try
            {
                using (Stream stream = await _pickedFolder.OpenStreamForWriteAsync(relativeFilePath, Windows.Storage.CreationCollisionOption.ReplaceExisting))
                {
                    await stream.WriteAsync(content, 0, content.Length);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
