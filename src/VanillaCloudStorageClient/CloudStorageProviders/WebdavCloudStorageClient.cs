// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Flurl;
using Flurl.Http;

namespace VanillaCloudStorageClient.CloudStorageProviders
{
    /// <summary>
    /// Implementation of the <see cref="ICloudStorageClient"/> interface,
    /// which can handle cloud storage on WebDav servers.
    /// </summary>
    public class WebdavCloudStorageClient : CloudStorageClientBase, ICloudStorageClient
    {
        /// <inheritdoc/>
        public override CloudStorageCredentialsRequirements CredentialsRequirements
        {
            get { return CloudStorageCredentialsRequirements.UsernamePasswordUrl; }
        }

        /// <inheritdoc/>
        public override async Task UploadFileAsync(string filename, byte[] fileContent, CloudStorageCredentials credentials)
        {
            credentials.ThrowIfInvalid(CredentialsRequirements, true);

            try
            {
                HttpContent content = new ByteArrayContent(fileContent);
                await Flurl.Request(credentials.Url, filename)
                    .WithBasicAuthOrAnonymous(credentials.Username, credentials.UnprotectedPassword)
                    .PutAsync(content);
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        /// <inheritdoc/>
        public override async Task<byte[]> DownloadFileAsync(string filename, CloudStorageCredentials credentials)
        {
            credentials.ThrowIfInvalid(CredentialsRequirements, true);

            try
            {
                return await Flurl.Request(credentials.Url, filename)
                    .WithBasicAuthOrAnonymous(credentials.Username, credentials.UnprotectedPassword)
                    .GetBytesAsync();
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        /// <inheritdoc/>
        public override async Task DeleteFileAsync(string filename, CloudStorageCredentials credentials)
        {
            credentials.ThrowIfInvalid(CredentialsRequirements, true);

            try
            {
                await Flurl.Request(credentials.Url, filename)
                    .WithBasicAuthOrAnonymous(credentials.Username, credentials.UnprotectedPassword)
                    .DeleteAsync();
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        /// <inheritdoc/>
        public override async Task<List<string>> ListFileNamesAsync(CloudStorageCredentials credentials)
        {
            credentials.ThrowIfInvalid(CredentialsRequirements, true);

            try
            {
                // Request only resourcetype to reduce network traffic
                byte[] requestBytes = Encoding.UTF8.GetBytes(
@"<?xml version='1.0' encoding='utf-8'?>
<D:propfind xmlns:D='DAV:'>
<D:prop>
    <D:resourcetype/>
</D:prop>
</D:propfind>");
                
                HttpContent content = new ByteArrayContent(requestBytes);
                XDocument responseXml;
                Url url = new Url(IncludeTrailingSlash(credentials.Url));
                using (Stream responseStream = await Flurl.Request(url)
                    .WithBasicAuthOrAnonymous(credentials.Username, credentials.UnprotectedPassword)
                    .WithHeader("Depth", "1")
                    .WithTimeout(20)
                    .SendAsync(new HttpMethod("PROPFIND"), content)
                    .ReceiveStream())
                {
                    responseXml = XDocument.Load(responseStream);
                }

                // Files have an empty resourcetype element, folders a child element "collection"
                return ParseWebdavResponseForFileNames(responseXml);
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        /// <summary>
        /// Made public for unit testing, this method interprets the response of a list file request.
        /// </summary>
        /// <param name="responseXml">Xml document generate from the webdav response.</param>
        /// <returns>List of file names.</returns>
        public static List<string> ParseWebdavResponseForFileNames(XDocument responseXml)
        {
            List<string> result = new List<string>();

            // Find all "response" elements, independend of their namespaces
            var responseElements = responseXml
                .Descendants()
                .Where(descendant => string.Equals("response", descendant.Name.LocalName, StringComparison.OrdinalIgnoreCase));

            foreach (XElement responseElement in responseElements)
            {
                // Find the "resourcetype" element, independend of its namespaces
                XElement resourceTypeElement = responseElement
                    .Descendants()
                    .Where(descendant => string.Equals("resourcetype", descendant.Name.LocalName, StringComparison.OrdinalIgnoreCase))
                    .FirstOrDefault();

                // Files have an empty resourcetype
                bool isFile = (resourceTypeElement != null) && (resourceTypeElement.IsEmpty);
                if (isFile)
                {
                    // Extract the "href" element, it contains the filename
                    XElement hrefElement = responseElement
                        .Elements()
                        .Where(descendant => string.Equals("href", descendant.Name.LocalName, StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();

                    if (hrefElement != null)
                    {
                        string filePath = Url.Decode(hrefElement.Value, false);
                        result.Add(Path.GetFileName(filePath));
                    }
                }
            }
            return result;
        }
    }
}
