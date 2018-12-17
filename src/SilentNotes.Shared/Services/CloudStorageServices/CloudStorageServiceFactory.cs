// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;

namespace SilentNotes.Services.CloudStorageServices
{
    /// <summary>
    /// Implementation of the <see cref="ICloudStorageServiceFactory"/> interface.
    /// To register a new implemented cloud storage service, extend this class.
    /// </summary>
    public class CloudStorageServiceFactory : ICloudStorageServiceFactory
    {
        private readonly INativeBrowserService _nativeBrowserService;
        private readonly ICryptoRandomService _randomService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudStorageServiceFactory"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public CloudStorageServiceFactory(INativeBrowserService nativeBrowserService, ICryptoRandomService randomService)
        {
            _nativeBrowserService = nativeBrowserService;
            _randomService = randomService;
        }

        /// <inheritdoc/>
        public ICloudStorageService Create(CloudStorageAccount account)
        {
            switch (account.CloudType)
            {
                case CloudStorageType.FTP:
                    return new FtpCloudStorageService(account);
                case CloudStorageType.WebDAV:
                    return new WebdavCloudStorageService(account);
                case CloudStorageType.Dropbox:
                    return new DropboxCloudStorageService(account, _nativeBrowserService, _randomService);
                case CloudStorageType.GMX:
                    return new GmxCloudStorageService(account);
                default:
                    return null;
            }
        }

        /// <inheritdoc/>
        public CloudStorageAccount CreateDefaultSettings(CloudStorageType storageType)
        {
            switch (storageType)
            {
                case CloudStorageType.FTP:
                    return new FtpCloudStorageService().Account;
                case CloudStorageType.WebDAV:
                    return new WebdavCloudStorageService().Account;
                case CloudStorageType.Dropbox:
                    return new DropboxCloudStorageService(_nativeBrowserService, _randomService).Account;
                case CloudStorageType.GMX:
                    return new GmxCloudStorageService().Account;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Gets the name of the picture to a given service type.
        /// </summary>
        /// <param name="storageType">Storage type to get the picture for.</param>
        /// <returns>Name of the picture.</returns>
        public static string GetAssetImageName(CloudStorageType storageType)
        {
            switch (storageType)
            {
                case CloudStorageType.FTP:
                    return "cloud_service_ftp.png";
                case CloudStorageType.WebDAV:
                    return "cloud_service_webdav.png";
                case CloudStorageType.Dropbox:
                    return "cloud_service_dropbox.png";
                case CloudStorageType.GMX:
                    return "cloud_service_gmx.png";
                default:
                    return string.Empty;
            }
        }
    }
}
