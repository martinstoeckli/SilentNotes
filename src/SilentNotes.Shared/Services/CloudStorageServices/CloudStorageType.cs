// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services.CloudStorageServices
{
    /// <summary>
    /// Enumeration of all known cloud storage service types.
    /// </summary>
    public enum CloudStorageType
    {
        Unknown,
        FTP,
        WebDAV,
        Dropbox,
        GMX,
    }

    /// <summary>
    /// Extension methods to the <see cref="CloudStorageType"/> enumeration.
    /// </summary>
    public static class CloudStorageTypeExtensions
    {
        /// <summary>
        /// Converts the name of a storage service type to an enum.
        /// </summary>
        /// <param name="storageTypeString">Name of storage service type.</param>
        /// <returns>The storage type.</returns>
        public static CloudStorageType StringToStorageType(string storageTypeString)
        {
            if (Enum.TryParse(storageTypeString, true, out CloudStorageType serviceType))
                return serviceType;
            return CloudStorageType.Unknown;
        }

        /// <summary>
        /// Gets the name of a given service type.
        /// </summary>
        /// <param name="storageType">Service type to get the name for.</param>
        /// <returns>Name of the service type.</returns>
        public static string StorageTypeToString(this CloudStorageType storageType)
        {
            if (storageType == CloudStorageType.Unknown)
                return string.Empty;
            else
                return storageType.ToString();
        }
    }
}
