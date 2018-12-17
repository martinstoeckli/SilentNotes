// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services.CloudStorageServices
{
    /// <summary>
    /// Factory which can create instances of cloud storage services.
    /// </summary>
    public interface ICloudStorageServiceFactory
    {
        /// <summary>
        /// Creates a cloud storage service instance.
        /// </summary>
        /// <param name="account">The settings for the service.</param>
        /// <returns>New cloud storage service instance, or null.</returns>
        ICloudStorageService Create(CloudStorageAccount account);

        /// <summary>
        /// Creates default settings for a given <paramref name="storageType"/>.
        /// </summary>
        /// <param name="storageType">Create settings for this storage type.</param>
        /// <returns>Default settings.</returns>
        CloudStorageAccount CreateDefaultSettings(CloudStorageType storageType);
    }
}
