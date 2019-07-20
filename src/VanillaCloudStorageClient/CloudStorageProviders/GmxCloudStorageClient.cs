using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VanillaCloudStorageClient.CloudStorageProviders
{
    /// <summary>
    /// Implementation of the <see cref="ICloudStorageClient"/> interface,
    /// which can handle cloud storage with the GMX.net provider.
    /// </summary>
    public class GmxCloudStorageClient : WebdavCloudStorageClient
    {
        /// <inheritdoc/>
        public override CloudStorageCredentialsRequirements CredentialsRequirements
        {
            get { return CloudStorageCredentialsRequirements.UsernamePassword; }
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
                bool isGmxComUser = 
                    (credentials.Username != null) &&
                    credentials.Username.EndsWith("gmx.com", StringComparison.InvariantCultureIgnoreCase);

                if (isGmxComUser)
                    credentials.Url = "https://storage-file-eu.gmx.com"; // Special url for the gmx.com domain
                else
                    credentials.Url = "https://webdav.mc.gmx.net"; // All other domains like gmx.net, gmx.de
            }
            return credentials;
        }
    }
}
