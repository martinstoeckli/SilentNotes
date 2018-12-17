// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SilentNotes.Services.CloudStorageServices
{
    /// <summary>
    /// Implementation of the <see cref="ICloudStorageService"/>,
    /// to be used with any WebDav cloud storage.
    /// </summary>
    public class WebdavCloudStorageService : ICloudStorageService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebdavCloudStorageService"/> class.
        /// </summary>
        public WebdavCloudStorageService()
            : this(new CloudStorageAccount { CloudType = CloudStorageType.WebDAV })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebdavCloudStorageService"/> class.
        /// </summary>
        /// <param name="account">The account with the credentials to use with this service.</param>
        public WebdavCloudStorageService(CloudStorageAccount account)
        {
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
        /// Returns a list of filenames in the Webdav directory.
        /// </summary>
        /// <param name="account">Account with login information.</param>
        /// <returns>List of filenames, not including the directory path.</returns>
        private static async Task<List<string>> ListFileNamesAsync(CloudStorageAccount account)
        {
            // Request only displayname and resourcetype to reduce network traffic
            byte[] requestBytes = Encoding.UTF8.GetBytes(
@"<?xml version='1.0' encoding='utf-8'?>
<D:propfind xmlns:D='DAV:'>
<D:prop>
    <D:displayname/>
    <D:resourcetype/>
</D:prop>
</D:propfind>");

            try
            {
                Uri directoryUri = new Uri(account.Url);

                byte[] response;
                using (WebClient webClient = new WebClientWithTimeout(TimeSpan.FromSeconds(30)))
                {
                    webClient.Credentials = new NetworkCredential(account.Username, account.Password);
                    webClient.Headers.Add("Depth", "1");
                    response = await webClient.UploadValuesTaskAsync(
                        directoryUri, "PROPFIND", new NameValueCollection());
                }

                XDocument responseXml;
                using (Stream responseStream = new MemoryStream(response))
                {
                    responseXml = XDocument.Load(responseStream);
                }

                // Files have an empty resourcetype element, folders a child element "collection"
                return ParseWebdavResponseForFileNames(responseXml);
            }
            catch (Exception ex)
            {
                throw CreateNewCloudStorageException(ex);
            }
        }

        internal static List<string> ParseWebdavResponseForFileNames(XDocument responseXml)
        {
            List<string> result = new List<string>();

            // Find all "response" elements, independend of their namespaces
            var responseElements = responseXml
                .Descendants()
                .Where(descendant => descendant.Name.LocalName == "response");

            foreach (XElement responseElement in responseElements)
            {
                // Find the "resourcetype" element, independend of its namespaces
                XElement resourceTypeElement = responseElement
                    .Descendants()
                    .Where(descendant => descendant.Name.LocalName == "resourcetype")
                    .FirstOrDefault();

                // Files have an empty resourcetype
                bool isFile = (resourceTypeElement != null) && (resourceTypeElement.IsEmpty);
                if (isFile)
                {
                    // Extract the "href" element, it contains the filename
                    XElement hrefElement = responseElement
                        .Elements()
                        .Where(descendant => descendant.Name.LocalName == "href")
                        .FirstOrDefault();

                    if (hrefElement != null)
                    {
                        string filename = ExtractUnescapedFilename(hrefElement.Value);
                        result.Add(filename);
                    }
                }
            }
            return result;
        }

        private static string ExtractUnescapedFilename(string url)
        {
            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                Uri someBaseUri = new Uri("http://canbeanything");
                uri = new Uri(someBaseUri, url);
            }
            return Path.GetFileName(uri.LocalPath);
        }

        /// <summary>
        /// Downloads a file from the cloud.
        /// </summary>
        /// <param name="account">Account with login information.</param>
        /// <param name="filename">Name of the file to download.</param>
        /// <returns>A task which can be called async, returning the downloaded file.</returns>
        private static async Task<byte[]> DownloadFileAsync(CloudStorageAccount account, string filename)
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
                    await webClient.UploadDataTaskAsync(fileUri, "PUT", data);
                }
            }
            catch (Exception ex)
            {
                throw CreateNewCloudStorageException(ex);
            }
        }

        /// <summary>
        /// Translates a WebException to its <see cref="CloudStorageException"/> pendant.
        /// </summary>
        /// <param name="ex">A web exception to translate.</param>
        /// <returns>A new exception which can be thrown.</returns>
        private static CloudStorageException CreateNewCloudStorageException(Exception ex)
        {
            if (ex is WebException webException)
            {
                if ((webException.Status == WebExceptionStatus.NameResolutionFailure) || webException.Status == WebExceptionStatus.ConnectFailure)
                    return new CloudStorageConnectionException();

                if (webException.Response is HttpWebResponse httpResponse)
                {
                    if ((httpResponse.StatusCode == HttpStatusCode.Forbidden) || (httpResponse.StatusCode == HttpStatusCode.Unauthorized))
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
