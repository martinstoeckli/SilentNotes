// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;

namespace SilentNotes.Services
{
    /// <summary>
    /// Interface of a service which can load images from the locale device.
    /// The image must be picked manually by the user, this grants the necessary privileges,
    /// which must otherwise be declared in the application manifest.
    /// </summary>
    public interface IImagePickerService
    {
        /// <summary>
        /// Lets the user pick an image from the media storage or from the camera if available.
        /// </summary>
        /// <returns>Returns true if the user picked an image, false if the pick dialog was
        /// canceled.</returns>
        Task<bool> PickImageAsync();

        /// <summary>
        /// Tries to read a formerly picked image, call this method only after <see cref="PickImageAsync"/>
        /// returned true.
        /// </summary>
        /// <exception cref="NoImagePickedException">Called when no file was picked by the user.</exception>
        /// <returns>Returns the content of the read image.</returns>
        Task<byte[]> ReadPickedImageAsync();
    }

    /// <summary>
    /// This exception is thrown when trying to call <see cref="IImagePickerService.ReadPickedImageAsync"/>
    /// before an image was picked.
    /// </summary>
    public class NoImagePickedException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NoImagePickedException"/> class.
        /// </summary>
        public NoImagePickedException()
            : base("Cannot read image because no image file was picked by the user.")
        {
        }
    }
}
