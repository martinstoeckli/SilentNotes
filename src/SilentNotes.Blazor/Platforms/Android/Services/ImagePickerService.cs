// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Android.App;
using Android.Content;
using Android.Provider;
using SilentNotes.Services;
using Uri = Android.Net.Uri;

namespace SilentNotes.Platforms.Services
{
    /// <summary>
    /// Implementation of the <see cref="IImagePickerService"/> for the Android platform.
    /// </summary>
    internal class ImagePickerService : IImagePickerService
    {
        private readonly IAppContextService _appContext;
        private readonly IActivityResultAwaiter _activityResultAwaiter;
        private static readonly string[] AcceptedMimeTypes = { "image/jpeg", "image/png" };

        private Uri _pickedUri;

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

        /// <inheritdoc/>
        public async Task<bool> PickImageAsync()
        {
            List<Intent> availableIntents = new List<Intent>();

            // Add gallery app as additional intent to choose
            Intent galleryIntent = new Intent(Intent.ActionPick, MediaStore.Images.Media.ExternalContentUri);
            galleryIntent.SetType("image/*");
            galleryIntent.PutExtra(Intent.ExtraMimeTypes, AcceptedMimeTypes);
            availableIntents.Add(galleryIntent);

            // Create gallery image picker intent
            Intent pickIntent = new Intent(MediaStore.ActionPickImages);
            galleryIntent.PutExtra(Intent.ExtraMimeTypes, AcceptedMimeTypes);
            if (pickIntent.ResolveActivity(_appContext.Context.PackageManager) != null)
                availableIntents.Add(pickIntent);

            // Create chooser intent which allows to choose the picker
            Intent chooserIntent = Intent.CreateChooser(availableIntents[0], string.Empty);
            availableIntents.RemoveAt(0);
            if (availableIntents.Count > 0)
                chooserIntent.PutExtra(Intent.ExtraInitialIntents, availableIntents.ToArray());

            var activityResult = await _activityResultAwaiter.StartActivityAndWaitForResult(
                _appContext.RootActivity, chooserIntent);

            if (activityResult.ResultCode == Result.Ok && activityResult.Data?.Data is Uri uri)
            {
                string mimeType = _appContext.Context.ContentResolver.GetType(uri);
                if (AcceptedMimeTypes.Contains(mimeType, StringComparer.OrdinalIgnoreCase))
                {
                    _pickedUri = uri;
                    return true;
                }
            }

            _pickedUri = null;
            return false;
        }

        /// <inheritdoc/>
        public async Task<byte[]> ReadPickedImageAsync()
        {
            if (_pickedUri == null)
                throw new NoImagePickedException();

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
