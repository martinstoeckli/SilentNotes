using System;
using System.Collections.Generic;

namespace VanillaCloudStorageClient
{
    ///// <summary>
    ///// Implementation of the <see cref="ICloudStorageFactory"/> interface.
    ///// </summary>
    //public class CloudStorageFactory : ICloudStorageFactory
    //{
    //    /// <summary>The type of an FTP cloud storage client.</summary>
    //    public const string CloudStorageTypeFtp = "FTP";

    //    /// <summary>The type of a WebDAV cloud storage client.</summary>
    //    public const string CloudStorageTypeWebdav = "WebDAV";

    //    /// <summary>The type of an Dropbox cloud storage client.</summary>
    //    public const string CloudStorageTypeDropbox = "Dropbox";

    //    /// <summary>The type of a GMX cloud storage client.</summary>
    //    public const string CloudStorageTypeGmx = "GMX";

    //    private readonly Dictionary<string, Func<ICloudStorageClient>> _registeredConstructorDelegates;

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="CloudStorageFactory"/> class.
    //    /// </summary>
    //    public CloudStorageFactory()
    //    {
    //        _registeredConstructorDelegates = new Dictionary<string, Func<ICloudStorageClient>>(StringComparer.InvariantCultureIgnoreCase);
    //    }

    //    /// <inheritdoc/>
    //    public void Register(string cloudStorageType, Func<ICloudStorageClient> constructorDelegate)
    //    {
    //        if (string.IsNullOrWhiteSpace(cloudStorageType))
    //            throw new ArgumentNullException(nameof(cloudStorageType));
    //        if (constructorDelegate == null)
    //            throw new ArgumentNullException(nameof(constructorDelegate));

    //        _registeredConstructorDelegates.Add(cloudStorageType, constructorDelegate);
    //    }

    //    /// <inheritdoc/>
    //    public ICloudStorageClient Create(string cloudStorageType)
    //    {
    //        if (!_registeredConstructorDelegates.TryGetValue(cloudStorageType, out Func<ICloudStorageClient> constructorDelegate))
    //            throw new CloudStorageException(string.Format("Cannot create cloud storage client, because the requested type '{0}' is unknown.", cloudStorageType), null);

    //        return constructorDelegate();
    //    }
    //}
}
