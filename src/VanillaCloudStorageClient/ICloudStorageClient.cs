using System.Collections.Generic;
using System.Threading.Tasks;

namespace VanillaCloudStorageClient
{
    /// <summary>
    /// Describes a cloud storage client, which can be used to download/upload files from/to
    /// an online storage service. The client works with in a single given directory.
    /// </summary>
    public interface ICloudStorageClient
    {
        /// <summary>
        /// Gets information about the required credentials for this storage client.
        /// </summary>
        CloudStorageCredentialsRequirements CredentialsRequirements { get; }

        /// <summary>
        /// Uploads a file to the cloud. If the file does not yet exist it will be created,
        /// otherwise it will be overwritten.
        /// </summary>
        /// <param name="filename">Filename without path.</param>
        /// <param name="fileContent">The content of the file to upload.</param>
        /// <param name="credentials">The credentials to access the storage service.</param>
        /// <returns>A task which can be called async.</returns>
        Task UploadFileAsync(string filename, byte[] fileContent, CloudStorageCredentials credentials);

        /// <summary>
        /// Downloads a file from the cloud.
        /// Please first call <see cref="ExistsFileAsync"/> to check whether the file exists,
        /// because one cannot always reliable distinguish between a missing privileges and a
        /// missing file.
        /// </summary>
        /// <param name="filename">Filename without path.</param>
        /// <param name="credentials">The credentials to access the storage service.</param>
        /// <returns>Returns the downloaded file.</returns>
        Task<byte[]> DownloadFileAsync(string filename, CloudStorageCredentials credentials);

        /// <summary>
        /// Deletes a file from the cloud.
        /// </summary>
        /// <param name="filename">Filename without path.</param>
        /// <param name="credentials">The credentials to access the storage service.</param>
        /// <returns>A task which can be called async.</returns>
        Task DeleteFileAsync(string filename, CloudStorageCredentials credentials);

        /// <summary>
        /// Lists all filenames of the OAuth2 web service in current directory. The filenames
        /// contain only the filename without the path.
        /// </summary>
        /// <param name="credentials">The credentials to access the storage service.</param>
        /// <returns>List of filenames.</returns>
        Task<List<string>> ListFileNamesAsync(CloudStorageCredentials credentials);

        /// <summary>
        /// Checks whether a file exists at this location.
        /// </summary>
        /// <param name="filename">Filename without path.</param>
        /// <param name="credentials">The credentials to access the storage service.</param>
        /// <returns>Returns true if the file exists, otherwise false.</returns>
        Task<bool> ExistsFileAsync(string filename, CloudStorageCredentials credentials);
    }
}
