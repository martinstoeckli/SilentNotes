// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;
using VanillaCloudStorageClient.OAuth2;

namespace VanillaCloudStorageClient.CloudStorageProviders
{
    /// <summary>
    /// Implementation of the <see cref="ICloudStorageClient"/> interface,
    /// which can handle cloud storage with the Dropbox API.
    /// Required permissions scopes are: files.content.read, files.content.write, files.metadata.read
    /// </summary>
    public class DropboxCloudStorageClient : OAuth2CloudStorageClient, ICloudStorageClient, IOAuth2CloudStorageClient
    {
        private const string AuthorizeUrl = "https://www.dropbox.com/oauth2/authorize";
        private const string TokenUrl = "https://api.dropbox.com/oauth2/token";
        private const string UploadUrl = "https://content.dropboxapi.com/2/files/upload";
        private const string DownloadUrl = "https://content.dropboxapi.com/2/files/download";
        private const string DeleteUrl = "https://api.dropboxapi.com/2/files/delete_v2";
        private const string ListUrl = "https://api.dropboxapi.com/2/files/list_folder";
        private const string ListContinueUrl = "https://api.dropboxapi.com/2/files/list_folder/continue";

        /// <summary>
        /// Initializes a new instance of the <see cref="DropboxCloudStorageClient"/> class.
        /// </summary>
        /// <param name="oauthClientId">Sets the <see cref="OAuth2Config.ClientId"/> property.</param>
        /// <param name="oauthRedirectUrl">Sets the <see cref="OAuth2Config.RedirectUrl"/> property.</param>
        public DropboxCloudStorageClient(string oauthClientId, string oauthRedirectUrl)
            : base(new OAuth2Config
            {
                AuthorizeServiceEndpoint = AuthorizeUrl,
                TokenServiceEndpoint = TokenUrl,
                ClientId = oauthClientId,
                RedirectUrl = oauthRedirectUrl,
                Flow = AuthorizationFlow.Code,
                Scope = null,
                ClientSecretHandling = ClientSecretHandling.DoNotSend,
            })
        {
        }

        #region ICloudStorageClient

        /// <inheritdoc/>
        public override string BuildAuthorizationRequestUrl(string state, string codeVerifier)
        {
            string url = base.BuildAuthorizationRequestUrl(state, codeVerifier);
            url = url + "&token_access_type=offline"; // This is only necessary as long as short lived tokens are not the default in the Dropbox developer console (possible until 30.09.2021).
            return url;
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
                string jsonPathParameter = JsonConvert.SerializeObject(new
                {
                    path = EnsureLeadingSlash(filename),
                    mode = "overwrite",
                    autorename = false,
                });
                HttpContent content = new ByteArrayContent(fileContent);

                await Flurl.Request(UploadUrl)
                    .WithOAuthBearerToken(credentials.Token.AccessToken)
                    .WithHeader("Dropbox-API-Arg", jsonPathParameter)
                    .WithHeader("Content-Type", "application/octet-stream")
                    .PostAsync(content);
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
                string jsonPathParameter = JsonConvert.SerializeObject(new
                {
                    path = EnsureLeadingSlash(filename)
                });

                byte[] result = await Flurl.Request(DownloadUrl)
                    .WithOAuthBearerToken(credentials.Token.AccessToken)
                    .WithHeader("Dropbox-API-Arg", jsonPathParameter)
                    .PostAsync(null)
                    .ReceiveBytes();

                return result;
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
                await Flurl.Request(DeleteUrl)
                    .WithOAuthBearerToken(credentials.Token.AccessToken)
                    .PostJsonAsync(new
                    {
                        path = EnsureLeadingSlash(filename)
                    });
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
                string jsonResponse = await Flurl.Request(ListUrl)
                    .WithOAuthBearerToken(credentials.Token.AccessToken)
                    .PostJsonAsync(new
                    {
                        path = string.Empty,
                        recursive = false,
                        include_deleted = false,
                        include_has_explicit_shared_members = false,
                        include_mounted_folders = true,
                        include_non_downloadable_files = false
                    })
                    .ReceiveString();

                JsonFolderEntries entries = JsonConvert.DeserializeObject<JsonFolderEntries>(jsonResponse);
                List<string> result = new List<string>();
                result.AddRange(entries.Entries
                    .Where(item => item.Tag == "file")
                    .Select(item => item.Name));

                // Check whether there are more file names to get and make consecutive requests.
                while (entries.HasMore)
                {
                    jsonResponse = await Flurl.Request(ListContinueUrl)
                        .WithOAuthBearerToken(credentials.Token.AccessToken)
                        .PostJsonAsync(new { cursor = entries.Cursor })
                        .ReceiveString();

                    entries = JsonConvert.DeserializeObject<JsonFolderEntries>(jsonResponse);
                    result.AddRange(entries.Entries
                        .Where(item => item.Tag == "file")
                        .Select(item => item.Name));
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        #endregion

        /// <summary>
        /// Dropbox expects the file path to start with a slash.
        /// </summary>
        /// <param name="filename">Name of the file.</param>
        /// <returns>Filename with leading slash.</returns>
        private string EnsureLeadingSlash(string filename)
        {
            if (filename.StartsWith("/"))
                return filename;
            else
                return "/" + filename;
        }

        /// <summary>
        /// Json (de)serialization class.
        /// </summary>
        private class JsonFolderEntries
        {
            [JsonProperty(PropertyName = "entries")]
            public List<JsonFolderEntry> Entries { get; set; }

            [JsonProperty(PropertyName = "cursor")]
            public string Cursor { get; set; }

            [JsonProperty(PropertyName = "has_more")]
            public bool HasMore { get; set; }
        }

        /// <summary>
        /// Json (de)serialization class.
        /// </summary>
        private class JsonFolderEntry
        {
            [JsonProperty(PropertyName = ".tag")]
            public string Tag { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "cursor")]
            public string Cursor { get; set; }

            [JsonProperty(PropertyName = "has_more")]
            public bool HasMore { get; set; }
        }
    }
}
