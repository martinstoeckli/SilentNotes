// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SilentNotes.Services.CloudStorageServices
{
    /// <summary>
    /// Implementation of the <see cref="ICloudStorageService"/>,
    /// which can handle any FTP cloud storage.
    /// </summary>
    public class FtpCloudStorageService : ICloudStorageService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCloudStorageService"/> class.
        /// </summary>
        public FtpCloudStorageService()
            : this(new CloudStorageAccount { CloudType = CloudStorageType.FTP })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FtpCloudStorageService"/> class.
        /// </summary>
        /// <param name="account">The account with the credentials to use with this service.</param>
        public FtpCloudStorageService(CloudStorageAccount account)
        {
            if (account.CloudType != CloudStorageType.FTP)
                throw new Exception("Invalid account information for FtpCloudStorageService, expected cloud storage type FTP");

            Account = account;
        }

        /// <inheritdoc/>
        public CloudStorageAccount Account { get; private set; }

        /// <inheritdoc/>
        public async Task<bool> ExistsRepositoryAsync()
        {
            List<string> filenames = await ListFileNamesAsync(Account);
            return filenames.Contains(Config.RepositoryFileName, StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public async Task<byte[]> DownloadRepositoryAsync()
        {
            return await DownloadFileAsync(Account, Config.RepositoryFileName);
        }

        /// <inheritdoc/>
        public async Task UploadRepositoryAsync(byte[] repository)
        {
            await UploadFileAsync(repository, Account, Config.RepositoryFileName);
        }

        /// <summary>
        /// Returns a list of filenames in the FTP directory.
        /// </summary>
        /// <param name="account">Account with login information.</param>
        /// <returns>List of filenames, not including the directory path.</returns>
        private static async Task<List<string>> ListFileNamesAsync(CloudStorageAccount account)
        {
            TimeSpan timeout = TimeSpan.FromSeconds(30);
            try
            {
                Uri directoryUri = new Uri(account.Url);

                WebRequest request = WebRequest.Create(directoryUri);
                request.Timeout = (int)timeout.TotalMilliseconds;
                request.Credentials = new NetworkCredential(account.Username, account.Password);
                request.Proxy = WebRequest.DefaultWebProxy;
                request.Method = WebRequestMethods.Ftp.ListDirectory; // "NLST"

                // Call the list command
                string responseText = null;
                using (WebResponse response = await request.GetResponseAsync())
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        using (StreamReader reader = new StreamReader(responseStream))
                        {
                            responseText = reader.ReadToEnd();
                        }
                    }
                }

                // Interpret the response
                string unixDelimitedResponse = responseText.Replace("\r\n", "\n");
                string[] fileNames = unixDelimitedResponse.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                List<string> result = new List<string>(fileNames);
                result.Remove("..");
                result.Remove(".");
                return result;
            }
            catch (Exception ex)
            {
                throw CreateNewCloudStorageException(ex);
            }
        }

        /// <summary>
        /// Downloads a file from the cloud.
        /// </summary>
        /// <param name="account">Account with login information.</param>
        /// <param name="filename">Name of the file to download.</param>
        /// <returns>A task which can be called async, returning the downloaded file.</returns>
        private async Task<byte[]> DownloadFileAsync(CloudStorageAccount account, string filename)
        {
            try
            {
                Uri fileUri = new Uri(UrlCombine(account.Url, filename));
                byte[] data;
                using (WebClient webClient = new WebClientWithTimeout(TimeSpan.FromSeconds(60)))
                {
                    webClient.Credentials = new NetworkCredential(account.Username, account.Password);
                    data = await webClient.DownloadDataTaskAsync(fileUri);
                }
                return data;
            }
            catch (Exception ex)
            {
                throw CreateNewCloudStorageException(ex);
            }
        }

        /// <summary>
        /// Uploads a file to the cloud.
        /// </summary>
        /// <param name="data">The content of the file to upload.</param>
        /// <param name="account">Account with login information.</param>
        /// <param name="filename">Name of the file to upload.</param>
        /// <returns>A task which can be called async.</returns>
        private static async Task UploadFileAsync(byte[] data, CloudStorageAccount account, string filename)
        {
            try
            {
                Uri fileUri = new Uri(UrlCombine(account.Url, filename));
                using (WebClient webClient = new WebClientWithTimeout(TimeSpan.FromSeconds(90)))
                {
                    webClient.Credentials = new NetworkCredential(account.Username, account.Password);
                    await webClient.UploadDataTaskAsync(fileUri, data);
                }
            }
            catch (Exception ex)
            {
                throw CreateNewCloudStorageException(ex);
            }
        }

        /// <summary>
        /// Translates a WebException to its <see cref="CloudStorageException"/> pendant and throws it.
        /// </summary>
        /// <param name="ex">A web exception to translate.</param>
        private void ThrowCloudStorageException(Exception ex)
        {
            if (ex is WebException webException)
            {
                if ((webException.Status == WebExceptionStatus.NameResolutionFailure) || webException.Status == WebExceptionStatus.ConnectFailure)
                    throw new CloudStorageConnectionException();

                FtpWebResponse ftpResponse = webException.Response as FtpWebResponse;
                if (ftpResponse != null)
                {
                    if ((ftpResponse.StatusCode == FtpStatusCode.NotLoggedIn) || (ftpResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable))
                        throw new CloudStorageForbiddenException();
                }
            }
            else if (ex is UriFormatException)
            {
                throw new CloudStorageConnectionException();
            }

            throw new CloudStorageException();
        }

        private static CloudStorageException CreateNewCloudStorageException(Exception ex)
        {
            if (ex is WebException)
            {
                WebException webException = ex as WebException;
                if ((webException.Status == WebExceptionStatus.NameResolutionFailure) || webException.Status == WebExceptionStatus.ConnectFailure)
                    return new CloudStorageConnectionException();

                if (webException.Response is FtpWebResponse ftpResponse)
                {
                    if ((ftpResponse.StatusCode == FtpStatusCode.NotLoggedIn) || (ftpResponse.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable))
                        return new CloudStorageForbiddenException();
                }
            }
            else if (ex is UriFormatException)
            {
                return new CloudStorageConnectionException();
            }

            return new CloudStorageException();
        }

        private static string UrlCombine(string path, string filename)
        {
            string result = path;
            if (!result.EndsWith("/"))
                result += "/";
            result += filename;
            return result;
        }
    }
}
