// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Flurl.Http;

namespace VanillaCloudStorageClient
{
    /// <summary>
    /// Base class for implementations of the <see cref="ICloudStorageClient"/> interface.
    /// </summary>
    public abstract class CloudStorageClientBase : ICloudStorageClient
    {
        /// <summary>
        /// Since we use the Flurl client only explicitely without stateful properties, we can
        /// share it between all cloud storage clients. Use the <see cref="Flurl"/> property to
        /// access this member.
        /// </summary>
        private static IFlurlClient _sharedFlurlClient;

        /// <inheritdoc/>
        public abstract CloudStorageCredentialsRequirements CredentialsRequirements { get; }

        /// <inheritdoc/>
        public abstract Task UploadFileAsync(string filename, byte[] fileContent, CloudStorageCredentials credentials);

        /// <inheritdoc/>
        public abstract Task<byte[]> DownloadFileAsync(string filename, CloudStorageCredentials credentials);

        /// <inheritdoc/>
        public abstract Task DeleteFileAsync(string filename, CloudStorageCredentials credentials);

        /// <inheritdoc/>
        public abstract Task<List<string>> ListFileNamesAsync(CloudStorageCredentials credentials);

        /// <inheritdoc/>
        public virtual async Task<bool> ExistsFileAsync(string filename, CloudStorageCredentials credentials)
        {
            List<string> filenames = await ListFileNamesAsync(credentials);
            return filenames.Contains(filename, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Gets or sets or creates a Flurl client. The client is created on demand (lazy loading)
        /// and shared between all cloud storage clients. This means one should not rely on stateful
        /// properties like IFlurlClient.BaseUrl, instead set all necessary properties explicitely
        /// for each request.
        /// </summary>
        protected IFlurlClient Flurl
        {
            get { return _sharedFlurlClient ?? (_sharedFlurlClient = new FlurlClient()); }
            set { _sharedFlurlClient = value; }
        }

        /// <summary>
        /// Ensures a trailing slash "/" at the end of the url, to mark a directory.
        /// </summary>
        /// <param name="url">Url with or without a trailing slash.</param>
        /// <returns>Returns the url with trailing slash, or the original url in case or null or empty.</returns>
        public static string IncludeTrailingSlash(string url)
        {
            if (!string.IsNullOrEmpty(url) && !url.EndsWith("/"))
            {
                return url + "/";
            }
            return url;
        }

        /// <summary>
        /// Analyses the exception and converts it to an exception deriving from
        /// <see cref="CloudStorageException"/>, so it can be re-thrown.
        /// <example><code>
        /// catch (Exception ex)
        /// {
        ///     throw CreateCloudStorageException(ex);
        /// }
        /// </code></example>
        /// </summary>
        /// <param name="catchedException">The original exception.</param>
        /// <returns>A cloud storage exception.</returns>
        protected static CloudStorageException ConvertToCloudStorageException(Exception catchedException)
        {
            if (catchedException is CloudStorageException catchedCloudStorageException)
            {
                // The catched exception is already of correct type.
                return catchedCloudStorageException;
            }
            else if (catchedException.InnerException is CloudStorageException innerCloudStorageException)
            {
                // The catched exception is already of correct type but is wrapped inside another exception.
                return innerCloudStorageException;
            }
            else if (catchedException is FlurlHttpException flurlException)
            {
                // Handle Flurl exceptions
                if (catchedException is FlurlHttpTimeoutException)
                {
                    return new ConnectionFailedException("Timeout was reached", catchedException);
                }
                else if (catchedException is FlurlParsingException)
                {
                    return new CloudStorageException("The web response had an unexpected format", catchedException);
                }
                else
                {
                    switch (flurlException.Call.HttpStatus)
                    {
                        case HttpStatusCode.Unauthorized:
                        case HttpStatusCode.Forbidden:
                            return new AccessDeniedException(catchedException);
                        case HttpStatusCode.BadRequest:
                        case HttpStatusCode.NotFound:
                            return new ConnectionFailedException(catchedException);
                    }
                }
            }
            else if (catchedException is WebException webException)
            {
                // Handle WebExceptions
                if (webException.Response is FtpWebResponse ftpResponse)
                {
                    switch (ftpResponse.StatusCode)
                    {
                        case FtpStatusCode.ActionNotTakenFileUnavailable:
                            return new ConnectionFailedException(catchedException);
                        case FtpStatusCode.NotLoggedIn:
                            return new AccessDeniedException(catchedException);
                    }
                }
                else if ((webException.Status == WebExceptionStatus.NameResolutionFailure) || (webException.Status == WebExceptionStatus.ConnectFailure))
                {
                    return new ConnectionFailedException(catchedException);
                }
            }
            else if (catchedException is UriFormatException)
            {
                return new CloudStorageException("The Url has an invalid format.", catchedException);
            }

            // Fallback to unexpected exception
            return new CloudStorageException("An unexpected error occured", catchedException);
        }
    }
}
