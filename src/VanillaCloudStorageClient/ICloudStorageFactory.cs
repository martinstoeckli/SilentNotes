using System;

namespace VanillaCloudStorageClient
{
    /// <summary>
    /// Factory to create different types of cloud storage clients.
    /// </summary>
    public interface ICloudStorageFactory
    {
        /// <summary>
        /// Registers delegates to create new cloud storage clients. An application should register
        /// all clients for easier creation, in the <paramref name="constructorDelegate"/>
        /// the app can pass additional required constructor parameters for each client.
        /// If an IOC framework is used anyway, the clients may be the better registered there.
        /// <example><code>
        /// factory.Register("webdav", () => new WebdavCloudStorageClient());
        /// </code></example>
        /// </summary>
        /// <remarks>
        /// Usually each type of client is registered only once, but it can be registered multiple
        /// times if there is e.g. need to access different Dropbox accounts at the same time.
        /// </remarks>
        /// <param name="cloudStorageType">The type of the cloud storage, like "Dropbox".
        /// The storage type is case insensitive.</param>
        /// <param name="constructorDelegate">Delegate which creates a new instance of the cloud
        /// storage client.</param>
        void Register(string cloudStorageType, Func<ICloudStorageClient> constructorDelegate);

        /// <summary>
        /// Creates a new cloud storage client, for the given <paramref name="cloudStorageType"/>.
        /// </summary>
        /// <param name="cloudStorageType">The type of the cloud storage, like the
        /// <see cref="CloudStorageFactory"/>.CloudStorageType* constants.
        /// The storage type is case insensitive.</param>
        /// <exception cref="CloudStorageException">Is thrown when the <paramref name="cloudStorageType"/>
        /// is is not known.</exception>
        /// <returns>A new instance of a cloud storage client.</returns>
        ICloudStorageClient Create(string cloudStorageType);
    }
}
