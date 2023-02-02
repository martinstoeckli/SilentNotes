// Copyright © 2021 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.IO;
using System.Threading.Tasks;
using SilentNotes.Services;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace SilentNotes.UWP.Services
{
    /// <summary>
    /// Implementation of the <see cref="IFilePickerService"/> interface for the UWP platform.
    /// </summary>
    internal class FilePickerService : IFilePickerService
    {
        private Windows.Storage.IStorageFile _pickedFile;

        /// <inheritdoc/>
        public async Task<bool> PickFile()
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            filePicker.SettingsIdentifier = "oiwurei223i4u";
            filePicker.FileTypeFilter.Add("*");

            _pickedFile = await filePicker.PickSingleFileAsync();
            return _pickedFile != null;
        }

        /// <inheritdoc/>
        public async Task<byte[]> ReadPickedFile()
        {
            if (_pickedFile == null)
                throw new Exception("Pick a file first before it can be read.");

            using (IInputStream inputStream = await _pickedFile.OpenSequentialReadAsync())
            using (Stream stream = inputStream.AsStreamForRead())
            {
                byte[] result = new byte[stream.Length];
                await stream.ReadAsync(result, 0, result.Length);
                return result;
            }
        }
    }
}
