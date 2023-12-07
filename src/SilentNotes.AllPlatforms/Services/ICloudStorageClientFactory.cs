// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using VanillaCloudStorageClient;

namespace SilentNotes.Services
{
    /// <summary>
    /// Factory to create different types of cloud storage clients.
    /// An app has to implement this optional factory on its own if needed, if an IOC framework is
    /// used anyway, the clients may also be registered there directly, or the factory can make
    /// use of the IOC framework itself.
    /// </summary>
    public interface ICloudStorageClientFactory
    {
        /// <summary>
        /// Creates a new cloud storage client for the given <paramref name="cloudStorageId"/>, or
        /// returns the already existing one.
        /// </summary>
        /// <param name="cloudStorageId">Developer defined id of the cloud storage, like "Dropbox".
        /// </param>
        /// <exception cref="CloudStorageException">Is thrown when the <paramref name="cloudStorageId"/>
        /// is is not known.</exception>
        /// <returns>A new instance of a cloud storage client.</returns>
        ICloudStorageClient GetByKey(string cloudStorageId);

        /// <summary>
        /// Enumerates the ids of all available cloud storage clients.
        /// </summary>
        /// <returns>Enumeration of available cloud storage ids.</returns>
        IEnumerable<string> EnumerateCloudStorageIds();

        /// <summary>
        /// Gets additional information about a cloud storage.
        /// </summary>
        /// <param name="cloudStorageId">The id of the cloud storage client.</param>
        /// <returns>Metadata to the cloud storage client.</returns>
        CloudStorageMetadata GetCloudStorageMetadata(string cloudStorageId);
    }

    /// <summary>
    /// Additional information about a cloud storage client.
    /// </summary>
    public class CloudStorageMetadata
    {
        /// <summary>Gets or sets the title which is displayed in the user interface.</summary>
        public string Title { get; set; }

        /// <summary>Gets or sets the file name of the image asset (without path).</summary>
        public string AssetImageName { get; set; }
    }
}
