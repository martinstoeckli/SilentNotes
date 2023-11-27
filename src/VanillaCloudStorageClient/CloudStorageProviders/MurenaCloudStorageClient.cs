// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VanillaCloudStorageClient.CloudStorageProviders
{
    /// <summary>
    /// Implementation of the <see cref="ICloudStorageClient"/> interface,
    /// which can handle cloud storage with the murena.io provider.
    /// </summary>
    public class MurenaCloudStorageClient : WebdavCloudStorageClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MurenaCloudStorageClient"/> class.
        /// </summary>
        /// <param name="useSocketsForPropFind">See <see cref="WebdavCloudStorageClient"/>.</param>
        public MurenaCloudStorageClient(bool useSocketsForPropFind)
            : base(useSocketsForPropFind)
        {
        }

        /// <inheritdoc/>
        public override CloudStorageCredentialsRequirements CredentialsRequirements
        {
            get { return CloudStorageCredentialsRequirements.Username | CloudStorageCredentialsRequirements.Password; }
        }

        /// <inheritdoc/>
        public override Task UploadFileAsync(string filename, byte[] fileContent, CloudStorageCredentials credentials)
        {
            return base.UploadFileAsync(filename, fileContent, WithPredefinedUrl(credentials));
        }

        /// <inheritdoc/>
        public override Task<byte[]> DownloadFileAsync(string filename, CloudStorageCredentials credentials)
        {
            return base.DownloadFileAsync(filename, WithPredefinedUrl(credentials));
        }

        /// <inheritdoc/>
        public override Task DeleteFileAsync(string filename, CloudStorageCredentials credentials)
        {
            return base.DeleteFileAsync(filename, WithPredefinedUrl(credentials));
        }

        /// <inheritdoc/>
        public override Task<List<string>> ListFileNamesAsync(CloudStorageCredentials credentials)
        {
            return base.ListFileNamesAsync(WithPredefinedUrl(credentials));
        }

        private static CloudStorageCredentials WithPredefinedUrl(CloudStorageCredentials credentials)
        {
            if (credentials != null)
            {
                string usernameWithoutEmailPart = credentials.Username;
                int emailPartIndex = usernameWithoutEmailPart.IndexOf("@");
                if (emailPartIndex >= 0)
                    usernameWithoutEmailPart = usernameWithoutEmailPart.Remove(emailPartIndex);

                credentials.Url = string.Format("https://murena.io/remote.php/dav/files/{0}%40e.email", usernameWithoutEmailPart);
                credentials.Username = usernameWithoutEmailPart + "@e.email";
            }
            return credentials;
        }
    }
}
