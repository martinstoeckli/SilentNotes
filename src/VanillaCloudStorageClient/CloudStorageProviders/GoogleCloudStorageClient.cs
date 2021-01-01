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
    /// Implementation of the <see cref="ICloudStorageClient"/> interface, which can handle cloud
    /// storage with the Google Drive API. Files are stored in the predefined "appDataFolder"
    /// directory, the necessary privileges must be defined in the Goolge-developer-console for
    /// this client id.
    /// </summary>
    public class GoogleCloudStorageClient : OAuth2CloudStorageClient, ICloudStorageClient, IOAuth2CloudStorageClient
    {
        private const string AuthorizeUrl = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string TokenUrl = "https://www.googleapis.com/oauth2/v4/token";
        private const string UploadUrl = "https://www.googleapis.com/upload/drive/v3/files";
        private const string DownloadUrl = ListUrl;
        private const string DeleteUrl = ListUrl;
        private const string ListUrl = "https://www.googleapis.com/drive/v3/files";
        private const string DataFolder = "appDataFolder";

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleCloudStorageClient"/> class.
        /// </summary>
        /// <param name="oauthClientId">Sets the <see cref="OAuth2Config.ClientId"/> property.</param>
        /// <param name="oauthRedirectUrl">Sets the <see cref="OAuth2Config.RedirectUrl"/> property.</param>
        public GoogleCloudStorageClient(string oauthClientId, string oauthRedirectUrl)
            : base(new OAuth2Config
            {
                AuthorizeServiceEndpoint = AuthorizeUrl,
                TokenServiceEndpoint = TokenUrl,
                ClientId = oauthClientId,
                RedirectUrl = oauthRedirectUrl,
                Flow = AuthorizationFlow.Code,
                Scope = "https://www.googleapis.com/auth/drive.appdata",
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
                // First we check whether this file already exists, creating new files with the
                // same name will generate different versions.
                string fileId = await FindFileIdAsync(credentials.Token.AccessToken, filename);
                if (fileId == null)
                    await CreateNewFile(filename, fileContent, credentials);
                else
                    await UpdateExistingFile(fileId, fileContent, credentials);
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        private async Task CreateNewFile(string filename, byte[] fileContent, CloudStorageCredentials credentials)
        {
            // The resumable upload type is the recommended approach for reliable file transfer,
            // not only for big files.
            IFlurlResponse sessionResponse = await Flurl.Request(UploadUrl)
                .SetQueryParam("uploadType", "resumable")
                .WithOAuthBearerToken(credentials.Token.AccessToken)
                .WithHeader("X-Upload-Content-Type", "application/octet-stream")
                .WithHeader("X-Upload-Content-Length", fileContent.Length)
                .PostJsonAsync(new
                {
                    name = filename,
                    parents = new[] { DataFolder }
                });
            string sessionUri = sessionResponse.Headers.FirstOrDefault("Location");

            // Now that we have got the session, the file can be uploaded
            HttpContent content = new ByteArrayContent(fileContent);
            IFlurlResponse uploadResponse = await Flurl.Request(sessionUri)
                .WithOAuthBearerToken(credentials.Token.AccessToken)
                .PostAsync(content);
        }

        private async Task UpdateExistingFile(string fileId, byte[] fileContent, CloudStorageCredentials credentials)
        {
            // The resumable upload type is the recommended approach for reliable file transfer,
            // not only for big files.
            // Adding SetQueryParam("addParents", DataFolder) is not necessary because the id is unique
            IFlurlResponse sessionResponse = await Flurl.Request(UploadUrl, fileId)
                .SetQueryParam("uploadType", "resumable")
                .WithOAuthBearerToken(credentials.Token.AccessToken)
                .WithHeader("X-Upload-Content-Type", "application/octet-stream")
                .WithHeader("X-Upload-Content-Length", fileContent.Length)
                .PatchAsync(null);
            string sessionUri = sessionResponse.Headers.FirstOrDefault("Location");

            // Now that we have got the session, the file can be uploaded
            HttpContent content = new ByteArrayContent(fileContent);
            IFlurlResponse uploadResponse = await Flurl.Request(sessionUri)
                .WithOAuthBearerToken(credentials.Token.AccessToken)
                .PutAsync(content);
        }

        /// <inheritdoc/>
        public override async Task<byte[]> DownloadFileAsync(string filename, CloudStorageCredentials credentials)
        {
            credentials.ThrowIfInvalid(CredentialsRequirements);

            try
            {
                // First find the id of the file
                string fileId = await FindFileIdAsync(credentials.Token.AccessToken, filename);
                if (fileId == null)
                    throw new ConnectionFailedException(string.Format("The file '{0}' does not exist.", filename), null);

                byte[] responseData = await Flurl.Request(DownloadUrl, fileId)
                    .WithOAuthBearerToken(credentials.Token.AccessToken)
                    .SetQueryParam("alt", "media")
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
                // First find the id of the file
                string fileId = await FindFileIdAsync(credentials.Token.AccessToken, filename);
                if (fileId == null)
                    throw new ConnectionFailedException(string.Format("The file '{0}' does not exist.", filename), null);

                await Flurl.Request(DeleteUrl, fileId)
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
                JsonFolderEntries entries = null;
                do
                {
                    string jsonResponse = await Flurl.Request(ListUrl)
                        .WithOAuthBearerToken(credentials.Token.AccessToken)
                        .SetQueryParam("spaces", DataFolder)
                        .SetQueryParam("pageToken", entries?.Cursor)
                        .SetQueryParam("fields", "nextPageToken, incompleteSearch, files(kind, id, name)")
                        .GetAsync()
                        .ReceiveString();

                    entries = JsonConvert.DeserializeObject<JsonFolderEntries>(jsonResponse);
                    result.AddRange(entries.Entries
                        .Where(item => item.Kind == "drive#file")
                        .Select(item => item.Name));
                }
                while (entries?.Cursor != null);

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
            string id = await FindFileIdAsync(credentials.Token.AccessToken, filename);
            return !string.IsNullOrEmpty(id);
        }

        /// <summary>
        /// Gets the id of a given file on the Google drive, the id is an internal identifier of
        /// cloud drive and can be used in subsequent requests.
        /// </summary>
        /// <param name="accessToken">A valid OAuth2 access token.</param>
        /// <param name="filename">The filename to search for.</param>
        /// <returns>The first found id of the file, or null if no such file was found.
        /// Note that several files with the same name can co-exist in the same folder.</returns>
        private async Task<string> FindFileIdAsync(string accessToken, string filename)
        {
            try
            {
                string jsonResponse = await Flurl.Request(ListUrl)
                    .WithOAuthBearerToken(accessToken)
                    .SetQueryParam("spaces", DataFolder)
                    .SetQueryParam("q", string.Format("name='{0}'", filename))
                    .SetQueryParam("fields", "nextPageToken, incompleteSearch, files(kind, id, name)")
                    .GetAsync()
                    .ReceiveString();

                JsonFolderEntries entries = JsonConvert.DeserializeObject<JsonFolderEntries>(jsonResponse);
                return entries.Entries
                    .Where(item => item.Kind == "drive#file")
                    .Select(item => item.Id)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        /// <summary>
        /// Json (de)serialization class.
        /// </summary>
        private class JsonFolderEntries
        {
            [JsonProperty(PropertyName = "files")]
            public List<JsonFolderEntry> Entries { get; set; }

            [JsonProperty(PropertyName = "nextPageToken")]
            public string Cursor { get; set; }

            [JsonProperty(PropertyName = "incompleteSearch")]
            public bool IncompleteSearch { get; set; }
        }

        /// <summary>
        /// Json (de)serialization class.
        /// </summary>
        private class JsonFolderEntry
        {
            [JsonProperty(PropertyName = "kind")]
            public string Kind { get; set; }

            [JsonProperty(PropertyName = "id")]
            public string Id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }
        }
    }
}
