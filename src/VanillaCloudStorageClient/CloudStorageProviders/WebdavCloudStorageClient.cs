// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        private readonly bool _useSocketsForPropFind;

        /// <summary>
        /// Initializes a new intance of the <see cref="WebdavCloudStorageClient"/> class.
        /// </summary>
        /// <param name="useSocketsForPropFind">Use sockets for the http method "PROPFIND", when
        /// running on Android, because the default HttpClient will internally use the Java class
        /// HttpURLConnection, which doesn't allow custom http methods like "PROPFIND".</param>
        public WebdavCloudStorageClient(bool useSocketsForPropFind)
        {
            _useSocketsForPropFind = useSocketsForPropFind;
        }

        /// <inheritdoc/>
        public override CloudStorageCredentialsRequirements CredentialsRequirements
        {
            get { return CloudStorageCredentialsRequirements.Username | CloudStorageCredentialsRequirements.Password | CloudStorageCredentialsRequirements.Url | CloudStorageCredentialsRequirements.AcceptUnsafeCertificate; }
        }

        /// <inheritdoc/>
        public override async Task UploadFileAsync(string filename, byte[] fileContent, CloudStorageCredentials credentials)
        {
            credentials.ThrowIfInvalid(CredentialsRequirements, true);

            try
            {
                HttpContent content = new ByteArrayContent(fileContent);
                await GetFlurl(credentials.AcceptInvalidCertificate)
                    .Request(credentials.Url, filename)
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
                return await GetFlurl(credentials.AcceptInvalidCertificate)
                    .Request(credentials.Url, filename)
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
                await GetFlurl(credentials.AcceptInvalidCertificate)
                    .Request(credentials.Url, filename)
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

                if (!_useSocketsForPropFind)
                {
                    using (Stream responseStream = await GetFlurl(credentials.AcceptInvalidCertificate)
                        .Request(url)
                        .WithBasicAuthOrAnonymous(credentials.Username, credentials.UnprotectedPassword)
                        .WithHeader("Depth", "1")
                        .WithTimeout(20)
                        .SendAsync(new HttpMethod("PROPFIND"), content)
                        .ReceiveStream())
                    {
                        responseXml = XDocument.Load(responseStream);
                    }
                }
                else
                {
                    // Workaround: On Android the default HttpClientHandler uses the underlying Java
                    // class "HttpURLConnection", which unfortunately cannot handle the http method
                    // "PROPFIND".
                    // So we make an expection for this request and use the "SocketsHttpHandler"
                    // which has its own implementation, though it cannot validate SSL certificates
                    // from LetsEncrypt. We can safely ignore the validation, because consecutive
                    // calls will do the validation with the Android implementation.
                    // see: https://github.com/martinstoeckli/SilentNotes/issues/111
                    using (HttpMessageHandler httpMessageHandler = new SocketsHttpHandler
                    {
                        AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                        Credentials = new NetworkCredential(credentials.Username, credentials.Password),
                        SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                        {
                            RemoteCertificateValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; },
                        },
                    })
                    {
                        using (HttpClient httpClient = new HttpClient(httpMessageHandler, false) // Disposed by its own "using"
                            { Timeout = TimeSpan.FromSeconds(20) })
                        using (HttpRequestMessage msg = new HttpRequestMessage(new HttpMethod("PROPFIND"), url))
                        {
                            msg.Content = content;
                            msg.Content.Headers.TryAddWithoutValidation("Depth", "1");
                            using (HttpResponseMessage response = await httpClient.SendAsync(msg, new HttpCompletionOption()))
                            {
                                response.EnsureSuccessStatusCode();
                                using (Stream responseStream = await response.Content.ReadAsStreamAsync())
                                {
                                    responseXml = XDocument.Load(responseStream);
                                }
                            }
                        }
                    }
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
                bool isFile = ExistsAndHasNoChilds(resourceTypeElement);
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

        /// <summary>
        /// Checks whether the XElement is either an empty element (self closing), or doesn't have
        /// any child elements (opening and closing tag without children).
        /// </summary>
        /// <param name="element">Xml element to check.</param>
        /// <returns>True if it is an empty element, otherwise false.</returns>
        private static bool ExistsAndHasNoChilds(XElement element)
        {
            return (element != null) && !element.HasElements;
        }
    }
}
