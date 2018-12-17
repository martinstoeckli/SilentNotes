// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace SilentNotes.Services.CloudStorageServices
{
    /// <summary>
    /// Interface to load/safe data from the cloud
    /// </summary>
    public interface ICloudStorageService
    {
        /// <summary>
        /// Gets the credentials and other settings of the cloud storage sercvice.
        /// </summary>
        CloudStorageAccount Account { get; }

        /// <summary>
        /// Checks whether a repository exists at this location.
        /// </summary>
        /// <returns>Returns true if a repository exists, otherwise false.</returns>
        Task<bool> ExistsRepositoryAsync();

        /// <summary>
        /// Downloads the repository from the cloud.
        /// Please first call <see cref="ExistsRepositoryAsync"/> to check whether a repository
        /// exists, because sometimes one cannot reliable distinguish between missing
        /// privileges and missing repository.
        /// </summary>
        /// <returns>A task which can be called async, returning the downloaded repository.</returns>
        Task<byte[]> DownloadRepositoryAsync();

        /// <summary>
        /// Uploads a repository to the cloud.
        /// </summary>
        /// <param name="repository">The repository to upload.</param>
        /// <returns>A task which can be called async.</returns>
        Task UploadRepositoryAsync(byte[] repository);
    }

    /// <summary>
    /// Base class of all cloud storage exceptions.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Just exceptions belonging tightly to an interface.")]
    public class CloudStorageException : Exception
    {
    }

    /// <summary>
    /// Throw this exception if the server could not be connected.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Just exceptions belonging tightly to an interface.")]
    public class CloudStorageConnectionException : CloudStorageException
    {
    }

    /// <summary>
    /// Throw this exception if the authentication failed or if there are missing privileges.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Just exceptions belonging tightly to an interface.")]
    public class CloudStorageForbiddenException : CloudStorageException
    {
    }
}
