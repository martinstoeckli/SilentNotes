// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using SilentNotes.Services;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace SilentNotes.Platforms.Services
{
    /// <summary>
    /// Implementation of the <see cref="IImagePickerService"/> for the Windows platform.
    /// </summary>
    internal class ImagePickerService : IImagePickerService
    {
        private Windows.Storage.IStorageFile _pickedFile;

        /// <inheritdoc/>
        public async Task<bool> PickImageAsync()
        {
            FileOpenPicker filePicker = new FileOpenPicker();
            filePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            filePicker.SettingsIdentifier = "4A4BD804-F5B4-41E5-AF71-7CE431FAD554";
            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".jpeg");
            filePicker.FileTypeFilter.Add(".png");

            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, GetMainWindowHandle());

            _pickedFile = await filePicker.PickSingleFileAsync();
            return _pickedFile != null;
        }

        /// <inheritdoc/>
        public async Task<byte[]> ReadPickedImageAsync()
        {
            if (_pickedFile == null)
                throw new NoImagePickedException();

            byte[] result = null;
            using (IInputStream inputStream = await _pickedFile.OpenSequentialReadAsync())
            using (Stream stream = inputStream.AsStreamForRead())
            {
                Microsoft.Maui.Graphics.IImage platImg = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(stream);
                result = await platImg.AsBytesAsync();
            }
            return result;
        }

        private nint GetMainWindowHandle()
        {
            return ((MauiWinUIWindow)App.Current.Windows[0].Handler.PlatformView).WindowHandle;
        }
    }
}
