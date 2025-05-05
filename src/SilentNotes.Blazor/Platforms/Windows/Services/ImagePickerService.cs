// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SilentNotes.Services;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace SilentNotes.Platforms.Services
{
    internal class ImagePickerService : IImagePickerService
    {
        private Windows.Storage.IStorageFile _pickedFile;

        /// <inheritdoc/>
        public async Task<bool> PickImage()
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            filePicker.SettingsIdentifier = "zertrei223i4u";
            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".jpeg");
            filePicker.FileTypeFilter.Add(".png");

            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, GetMainWindowHandle());

            _pickedFile = await filePicker.PickSingleFileAsync();
            return _pickedFile != null;
        }

        private nint GetMainWindowHandle()
        {
            return ((MauiWinUIWindow)App.Current.Windows[0].Handler.PlatformView).WindowHandle;
        }
    }
}
