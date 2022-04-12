// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Newtonsoft.Json;
using VanillaCloudStorageClient.OAuth2;

namespace VanillaCloudStorageClient.CloudStorageProviders
{
    /// <summary>
    /// Implementation of the <see cref="ICloudStorageClient"/> interface, which can handle cloud
    /// storage with the Microsoft OneDrive API. Files are stored in the predefined "approot"
    /// directory, the necessary privileges "offline_access", "User.Read" and "Files.ReadWrite.AppFolder"
    /// must be defined in the Azure-developer-console for this client id.
    /// </summary>
    public class OnedriveCloudStorageClient : OAuth2CloudStorageClient, ICloudStorageClient, IOAuth2CloudStorageClient
    {
        private const string AuthorizeUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/authorize";
        private const string TokenUrl = "https://login.microsoftonline.com/common/oauth2/v2.0/token";
        private const string UploadUrl = "https://graph.microsoft.com/v1.0/me/drive/special/approot:/{0}:/createUploadSession";
        private const string DownloadUrl = "https://graph.microsoft.com/v1.0/me/drive/special/approot:/{0}:/content";
        private const string DeleteUrl = ExistsUrl;
        private const string ListUrl = "https://graph.microsoft.com/v1.0/me/drive/special/approot/children";
        private const string ExistsUrl = "https://graph.microsoft.com/v1.0/me/drive/special/approot:/{0}";
        private const string AppRootUrl = "https://graph.microsoft.com/v1.0/me/drive/special/approot";
        private string _appRootId;

        /// <summary>
        /// Initializes a new instance of the <see cref="OnedriveCloudStorageClient"/> class.
        /// </summary>
        /// <param name="oauthClientId">Sets the <see cref="OAuth2Config.ClientId"/> property.</param>
        /// <param name="oauthRedirectUrl">Sets the <see cref="OAuth2Config.RedirectUrl"/> property.</param>
        public OnedriveCloudStorageClient(string oauthClientId, string oauthRedirectUrl)
            : base(new OAuth2Config
            {
                AuthorizeServiceEndpoint = AuthorizeUrl,
                TokenServiceEndpoint = TokenUrl,
                ClientId = oauthClientId,
                RedirectUrl = oauthRedirectUrl,
                Flow = AuthorizationFlow.Code,
                Scope = "offline_access Files.ReadWrite.AppFolder", // offline_access returns refresh token
                ClientSecretHandling = ClientSecretHandling.DoNotSend,
            })
        {
        }

        /// <inheritdoc/>
        public override CloudStorageCredentialsRequirements CredentialsRequirements
        {
            get { return CloudStorageCredentialsRequirements.Token; }
        }

        /// <inheritdoc/>
        public override async Task UploadFileAsync(string filename, byte[] fileContent, CloudStorageCredentials credentials)
        {
            credentials.ThrowIfInvalid(CredentialsRequirements);

            try
            {
                await InitializeAppRootFolderAsync(credentials.Token.AccessToken);

                string url = string.Format(UploadUrl, Url.Encode(filename));
                byte[] requestBytes = Encoding.UTF8.GetBytes("{ \"item\": { \"@microsoft.graph.conflictBehavior\": \"replace\" } }");
                HttpContent sessionContent = new ByteArrayContent(requestBytes);

                // Using an upload session is the recommended approach for reliable file transfer,
                // not only for big files.
                string jsonResponse = await GetFlurl().Request(url)
                    .WithOAuthBearerToken(credentials.Token.AccessToken)
                    .WithHeader("Content-Type", "application/json")
                    .PostAsync(sessionContent)
                    .ReceiveString();

                JsonUploadSession session = JsonConvert.DeserializeObject<JsonUploadSession>(jsonResponse);

                // Now that we have got the session, the file can be uploaded
                HttpContent content = new ByteArrayContent(fileContent);
                IFlurlResponse uploadResponse = await GetFlurl().Request(session.UploadUrl)
                    .WithHeader("Content-Length", fileContent.Length)
                    .WithHeader("Content-Range", string.Format("bytes 0-{0}/{1}", fileContent.Length - 1, fileContent.Length))
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
            credentials.ThrowIfInvalid(CredentialsRequirements);

            try
            {
                string url = string.Format(DownloadUrl, Url.Encode(filename));

                byte[] responseData = await GetFlurl().Request(url)
                    .WithOAuthBearerToken(credentials.Token.AccessToken)
                    .GetAsync()
                    .ReceiveBytes();
                return responseData;
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        /// <inheritdoc/>
        public override async Task DeleteFileAsync(string filename, CloudStorageCredentials credentials)
        {
            credentials.ThrowIfInvalid(CredentialsRequirements);

            try
            {
                string url = string.Format(DeleteUrl, Url.Encode(filename));

                await GetFlurl().Request(url)
                    .WithOAuthBearerToken(credentials.Token.AccessToken)
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
            credentials.ThrowIfInvalid(CredentialsRequirements);

            try
            {
                List<string> result = new List<string>();
                string url = ListUrl;
                while (url != null)
                {
                    string jsonResponse = await GetFlurl().Request(url)
                        .WithOAuthBearerToken(credentials.Token.AccessToken)
                        .SetQueryParam("$select", "name,file")
                        .GetAsync()
                        .ReceiveString();

                    JsonFolderEntries entries = JsonConvert.DeserializeObject<JsonFolderEntries>(jsonResponse);
                    result.AddRange(entries.Entries
                        .Where(item => item.File != null)
                        .Select(item => item.Name));

                    url = entries.NextLink;
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        /// <inheritdoc/>
        public override async Task<bool> ExistsFileAsync(string filename, CloudStorageCredentials credentials)
        {
            try
            {
                string url = string.Format(ExistsUrl, Url.Encode(filename));

                await GetFlurl().Request(url)
                    .WithOAuthBearerToken(credentials.Token.AccessToken)
                    .SetQueryParam("$select", "name")
                    .GetAsync()
                    .ReceiveString();
                return true;
            }
            catch (FlurlHttpException ex)
            {
                if (ex.GetHttpStatusCode() == HttpStatusCode.NotFound)
                    return false;
                else
                    throw ConvertToCloudStorageException(ex);
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        /// <summary>
        /// Gets the id of the app root folder on OneDrive, the id is an internal identifier of
        /// cloud drive and can be used in subsequent requests.
        /// </summary>
        /// <param name="accessToken">A valid OAuth2 access token.</param>
        /// <returns>The id of the folder, or null if no such file was found.
        /// Note that several files with the same name can co-exist in the same folder.</returns>
        private async Task<string> FindAppRootIdAsync(string accessToken)
        {
            try
            {
                string jsonResponse = await GetFlurl().Request(AppRootUrl)
                    .WithOAuthBearerToken(accessToken)
                    .SetQueryParam("$select", "id")
                    .GetAsync()
                    .ReceiveString();
                JsonFolderEntry entry = JsonConvert.DeserializeObject<JsonFolderEntry>(jsonResponse);
                return entry.Id;
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        /// <summary>
        /// The first time a file is uploaded, the appRoot folder does not yet exist and an upload
        /// process will end up with a 403-Forbidden error. A precedent request to retrieve the id
        /// of the appRoot folder, will create this folder.
        /// </summary>
        /// <param name="accessToken">A valid OAuth2 access token.</param>
        private async Task InitializeAppRootFolderAsync(string accessToken)
        {
            if (string.IsNullOrEmpty(_appRootId))
                _appRootId = await FindAppRootIdAsync(accessToken);
        }

        /// <summary>
        /// Json (de)serialization class.
        /// </summary>
        private class JsonUploadSession
        {
            [JsonProperty(PropertyName = "uploadUrl")]
            public string UploadUrl { get; set; }
        }

        /// <summary>
        /// Json (de)serialization class.
        /// </summary>
        private class JsonFolderEntries
        {
            [JsonProperty(PropertyName = "value")]
            public List<JsonFolderEntry> Entries { get; set; }

            [JsonProperty(PropertyName = "@odata.nextLink")]
            public string NextLink { get; set; }
        }

        /// <summary>
        /// Json (de)serialization class.
        /// </summary>
        private class JsonFolderEntry
        {
            [JsonProperty(PropertyName = "file")]
            public JsonFolderEntryFile File { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
        }

        /// <summary>
        /// Json (de)serialization class.
        /// </summary>
        private class JsonFolderEntryFile
        {
        }
    }
}
