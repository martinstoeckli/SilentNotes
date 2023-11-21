// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using SilentNotes.Crypto;
using VanillaCloudStorageClient;
using VanillaCloudStorageClient.CloudStorageProviders;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="ICloudStorageClientFactory"/> class, for SilentNotes.
    /// This is the place to add all information, when adding a new cloud storage client.
    /// </summary>
    public class CloudStorageClientFactory : ServiceFactory<string, ICloudStorageClient>, ICloudStorageClientFactory
    {
        public const string CloudStorageIdFtp = "ftp";
        public const string CloudStorageIdWebdav = "webdav";
        public const string CloudStorageIdDropbox = "dropbox";
        public const string CloudStorageIdGoogleDrive = "googledrive";
        public const string CloudStorageIdOneDrive = "onedrive";
        public const string CloudStorageIdNextcloudWebdav = "nextcloud-webdav";
        public const string CloudStorageIdMurena = "murena";
        public const string CloudStorageIdGmx = "gmx";

        private const string _obfuscationKey = "4ed05d88-0193-4b14-9b0b-6977825de265";
        private readonly bool _useSocketsForPropFind;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudStorageClientFactory"/> class.
        /// </summary>
        public CloudStorageClientFactory()
            : base(true)
        {
            _useSocketsForPropFind = false;
#if ANDROID
            // Workaround: The HttpClient on Android will use the Java HttpURLConnection class,
            // which cannot handle the http method "PROPFIND"
            _useSocketsForPropFind = true;
#endif
            Add(CloudStorageIdFtp, () => new FtpCloudStorageClient());
            Add(CloudStorageIdWebdav, () => new WebdavCloudStorageClient(_useSocketsForPropFind));
            Add(CloudStorageIdDropbox, () => new DropboxCloudStorageClient(
                DeobfuscateClientId("b2JmdXNjYXRpb24kdHdvZmlzaF9nY20kaUNDQnhZRDFqTG4veUJQNSRwYmtkZjIkNEVrVTFOVVdZSkJpTWtQR2VNU0lhdz09JDEwMDAkqkhIg8kDs04BHHfD2Dldq7jC8LUT3AqPnyY6THmJJw=="),
                "ch.martinstoeckli.silentnotes://oauth2redirect/"));
            Add(CloudStorageIdGoogleDrive, () => new GoogleCloudStorageClient(
                DeobfuscateClientId("b2JmdXNjYXRpb24kdHdvZmlzaF9nY20kZlR5K1A4L05ka2pVZ2NkRURiTGZMdz09JHBia2RmMiRUYys0QkNwSzRzTCtLZS85LzNEVnh3PT0kMTAwMCS5o32MdtKqjqthaSYKFg+9Bvv9XRQTpPQhnYnPdk9ZIrwxZbYBARv+vG1oFpSdHzRFS3Hgsc0QorFWVVy2Xt4c6RPSYs2kFLa4h36KduqrLwKP5wD4qQuT"),
                "ch.martinstoeckli.silentnotes:/oauth2redirect/"));
            Add(CloudStorageIdOneDrive, () => new OnedriveCloudStorageClient(
                DeobfuscateClientId("b2JmdXNjYXRpb24kdHdvZmlzaF9nY20kMm91Sjk4dVloa3FzVTJMbFV3YzlYZz09JHBia2RmMiRIWDNzazgzdExLUExDRldjeis0RUtnPT0kMTAwMCQhu6dFgd6/j2A9388wATVBekdrXdLcCUHg1gMjKNJlrmAeWWKhJ+Wewi4eALJkqHyF1Np3"),
                "ch.martinstoeckli.silentnotes://oauth2redirect/"));
            Add(CloudStorageIdNextcloudWebdav, () => new WebdavCloudStorageClient(_useSocketsForPropFind));
            Add(CloudStorageIdMurena, () => new MurenaCloudStorageClient(_useSocketsForPropFind));
            Add(CloudStorageIdGmx, () => new GmxCloudStorageClient(_useSocketsForPropFind));
        }

        /// <inheritdoc/>
        public IEnumerable<string> EnumerateCloudStorageIds()
        {
            yield return CloudStorageIdFtp;
            yield return CloudStorageIdWebdav;
            yield return CloudStorageIdDropbox;
            yield return CloudStorageIdGoogleDrive;
            yield return CloudStorageIdOneDrive;
            yield return CloudStorageIdNextcloudWebdav;
            yield return CloudStorageIdMurena;
            yield return CloudStorageIdGmx;
        }

        /// <inheritdoc/>
        public CloudStorageMetadata GetCloudStorageMetadata(string cloudStorageId)
        {
            switch (cloudStorageId)
            {
                case CloudStorageIdFtp:
                    return new CloudStorageMetadata { Title = "FTP", AssetImageName = "cloud_service_ftp.png" };
                case CloudStorageIdWebdav:
                    return new CloudStorageMetadata { Title = "WebDAV", AssetImageName = "cloud_service_webdav.png" };
                case CloudStorageIdDropbox:
                    return new CloudStorageMetadata { Title = "Dropbox", AssetImageName = "cloud_service_dropbox.png" };
                case CloudStorageIdGoogleDrive:
                    return new CloudStorageMetadata { Title = "Google Drive", AssetImageName = "cloud_service_googledrive.png" };
                case CloudStorageIdOneDrive:
                    return new CloudStorageMetadata { Title = "OneDrive (Microsoft)", AssetImageName = "cloud_service_onedrive.png" };
                case CloudStorageIdNextcloudWebdav:
                    return new CloudStorageMetadata { Title = "Nextcloud (WebDAV)", AssetImageName = "cloud_service_nextcloud.png" };
                case CloudStorageIdMurena:
                    return new CloudStorageMetadata { Title = "murena", AssetImageName = "cloud_service_murena.png" };
                case CloudStorageIdGmx:
                    return new CloudStorageMetadata { Title = "GMX", AssetImageName = "cloud_service_gmx.png" };
                default:
                    throw new ArgumentOutOfRangeException(nameof(cloudStorageId), "Unknown cloud storage id.");
            }
        }

        /// <summary>
        /// Gets the plain text OAuth2 client id from an obfuscated client id.
        /// The client id is not really secret, but we don't want to spread it neither.
        /// </summary>
        /// <returns>The plain text client id.</returns>
        private string DeobfuscateClientId(string obfuscatedClientId)
        {
            return CryptoUtils.Deobfuscate(obfuscatedClientId, SecureStringExtensions.StringToSecureString(_obfuscationKey));
        }
    }
}
