// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Provider;
using AndroidX.Activity.Result;
using AndroidX.Core.Content;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using SilentNotes.Services;

namespace SilentNotes.Platforms.Services
{
    /// <summary>
    /// Implementation of the <see cref="IImagePickerService"/> for the Android platform.
    /// </summary>
    internal class ImagePickerService : IImagePickerService
    {
        private readonly IAppContextService _appContext;
        private readonly IActivityResultAwaiter _activityResultAwaiter;
        private Android.Net.Uri _pickedUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImagePickerService"/> class.
        /// </summary>
        /// <param name="appContextService">A service which knows about the current main activity.</param>
        /// <param name="activityResultAwaiter">An activity result awaiter.</param>
        public ImagePickerService(IAppContextService appContextService, IActivityResultAwaiter activityResultAwaiter)
        {
            _appContext = appContextService;
            _activityResultAwaiter = activityResultAwaiter;
        }

        public async Task<bool> PickImage()
        {
            List<Intent> availableIntents = new List<Intent>();

            // Add gallery app as additional intent to choose
            Intent galleryIntent = new Intent(Intent.ActionPick, MediaStore.Images.Media.ExternalContentUri);
            galleryIntent.SetType("image/*");
            availableIntents.Add(galleryIntent);

            // Create gallery image picker intent
            Intent pickIntent = new Intent(MediaStore.ActionPickImages);
            if (pickIntent.ResolveActivity(_appContext.Context.PackageManager) != null)
                availableIntents.Add(pickIntent);

            // Create chooser intent which allows to choose the picker
            Intent chooserIntent = Intent.CreateChooser(availableIntents[0], string.Empty);
            availableIntents.RemoveAt(0);
            if (availableIntents.Count > 0)
                chooserIntent.PutExtra(Intent.ExtraInitialIntents, availableIntents.ToArray());

            var activityResult = await _activityResultAwaiter.StartActivityAndWaitForResult(
                _appContext.RootActivity, chooserIntent);

            if (activityResult.ResultCode == Result.Ok)
            {
                _pickedUri = activityResult.Data?.Data;
                return true;
            }
            else
            {
                _pickedUri = null;
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<byte[]> ReadPickedImage()
        {
            if (_pickedUri == null)
                throw new Exception("Pick an image first before it can be read.");

            byte[] result = null;
            using (Stream inStream = _appContext.Context.ContentResolver.OpenInputStream(_pickedUri))
            {
                Microsoft.Maui.Graphics.IImage platImg = Microsoft.Maui.Graphics.Platform.PlatformImage.FromStream(inStream);
                result = await platImg.AsBytesAsync();
            }
            return result;
        }
    }
}
