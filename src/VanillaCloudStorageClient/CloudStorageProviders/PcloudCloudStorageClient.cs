﻿// Copyright © 2024 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Flurl.Http;
using VanillaCloudStorageClient.OAuth2;

namespace VanillaCloudStorageClient.CloudStorageProviders
{
    /// <summary>
    /// Implementation of the <see cref="ICloudStorageClient"/> interface,
    /// which can handle cloud storage with the pCloud API.
    /// </summary>
    public class PcloudCloudStorageClient : OAuth2CloudStorageClient, ICloudStorageClient, IOAuth2CloudStorageClient
    {
        private const string AuthorizeUrl = "https://my.pcloud.com/oauth2/authorize";
        private const string TokenUrl = "https://{0}/oauth2_token";
        private const string UploadUrl = "https://{0}/uploadfile";
        private const string FileLinkUrl = "https://{0}/getfilelink";
        private const string DeleteUrl = "https://{0}/deletefile";
        private const string ListUrl = "https://{0}/listfolder";
        private const long APP_ROOT_FOLDER_ID = 0;

        private readonly string _dataCenterHost;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleCloudStorageClient"/> class.
        /// </summary>
        /// <param name="oauthClientId">Sets the <see cref="OAuth2Config.ClientId"/> property.</param>
        /// <param name="oauthRedirectUrl">Sets the <see cref="OAuth2Config.RedirectUrl"/> property.</param>
        /// <param name="dataCenter">The data center used by the pCloud user.</param>
        public PcloudCloudStorageClient(string oauthClientId, string oauthRedirectUrl, DataCenter dataCenter)
            : base(new OAuth2Config
            {
                AuthorizeServiceEndpoint = AuthorizeUrl,
                TokenServiceEndpoint = string.Format(TokenUrl, GetDataCenterHost(dataCenter)),
                ClientId = oauthClientId,
                RedirectUrl = oauthRedirectUrl,
                Flow = AuthorizationFlow.Token, // Use the token-flow, because currently the provider requires the client_secret for the code-flow.
                ClientSecretHandling = ClientSecretHandling.DoNotSend,
            })
        {
            // The host depends on the datacenter and will be inserted into the API urls
            _dataCenterHost = GetDataCenterHost(dataCenter);
        }

        /// <summary>
        /// Enumeration of known data centers, which determine the API urls.
        /// </summary>
        public enum DataCenter
        {
            Us,
            Europe,
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
                HttpContent content = new ByteArrayContent(fileContent);
                string requestUrl = string.Format(UploadUrl, _dataCenterHost);

                string jsonResponse = await GetFlurl().Request(requestUrl)
                    .WithOAuthBearerToken(credentials.Token.AccessToken)
                    .SetQueryParam("folderid", APP_ROOT_FOLDER_ID)
                    .SetQueryParam("filename", filename)
                    .SetQueryParam("nopartial", 1)
                    .SetQueryParam("filtermeta", "fileid")
                    .PutAsync(content)
                    .ReceiveString();
                ThrowIfErrorResponse(jsonResponse);
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
                // First find the id of the file
                long? fileId = await FindFileIdAsync(credentials.Token.AccessToken, filename);
                if (!fileId.HasValue)
                    throw new ConnectionFailedException(string.Format("The file '{0}' does not exist.", filename), null);
                string requestUrl = string.Format(FileLinkUrl, _dataCenterHost);

                // Get download link
                string jsonResponse = await GetFlurl().Request(requestUrl)
                    .WithOAuthBearerToken(credentials.Token.AccessToken)
                    .SetQueryParam("fileid", fileId)
                    .GetAsync()
                    .ReceiveString();
                ThrowIfErrorResponse(jsonResponse);
                JsonFileLink fileLink = JsonSerializer.Deserialize<JsonFileLink>(jsonResponse);

                // Use downloadlink to get the file
                requestUrl = string.Format("https://{0}{1}", fileLink.Hosts[0], fileLink.Path);
                byte[] responseData = await GetFlurl().Request(requestUrl)
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
                long? fileId = await FindFileIdAsync(credentials.Token.AccessToken, filename);
                if (!fileId.HasValue)
                    throw new ConnectionFailedException(string.Format("The file '{0}' does not exist.", filename), null);

                string requestUrl = string.Format(DeleteUrl, _dataCenterHost);

                // Get download link
                string jsonResponse = await GetFlurl().Request(requestUrl)
                    .WithOAuthBearerToken(credentials.Token.AccessToken)
                    .SetQueryParam("fileid", fileId)
                    .SetQueryParam("filtermeta", "isdeleted")
                    .PostAsync()
                    .ReceiveString();
                ThrowIfErrorResponse(jsonResponse);
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
                string requestUrl = string.Format(ListUrl, _dataCenterHost);

                string jsonResponse = await GetFlurl().Request(requestUrl)
                    .WithOAuthBearerToken(credentials.Token.AccessToken)
                    .SetQueryParam("folderid", APP_ROOT_FOLDER_ID)
                    .SetQueryParam("filtermeta", "name,isfolder")
                    .GetAsync()
                    .ReceiveString();
                ThrowIfErrorResponse(jsonResponse);

                JsonListFolder entries = JsonSerializer.Deserialize<JsonListFolder>(jsonResponse);
                return entries.MetaData.Contents
                    .Where(item => item.IsFolder == false)
                    .Select(item => item.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        /// <inheritdoc/>
        public override async Task<bool> ExistsFileAsync(string filename, CloudStorageCredentials credentials)
        {
            long? id = await FindFileIdAsync(credentials.Token.AccessToken, filename);
            return id.HasValue;
        }

        /// <summary>
        /// Gets the id of a given file on the PCloud drive, the id is an internal identifier of
        /// cloud drive and can be used in subsequent requests.
        /// </summary>
        /// <param name="accessToken">A valid OAuth2 access token.</param>
        /// <param name="filename">The filename to search for.</param>
        /// <returns>The found id of the file, or null if no such file was found.</returns>
        private async Task<long?> FindFileIdAsync(string accessToken, string filename)
        {
            try
            {
                string requestUrl = string.Format(ListUrl, _dataCenterHost);

                string jsonResponse = await GetFlurl().Request(requestUrl)
                    .WithOAuthBearerToken(accessToken)
                    .SetQueryParam("folderid", APP_ROOT_FOLDER_ID)
                    .SetQueryParam("filtermeta", "name,isfolder,fileid")
                    .GetAsync()
                    .ReceiveString();
                ThrowIfErrorResponse(jsonResponse);

                JsonListFolder entries = JsonSerializer.Deserialize<JsonListFolder>(jsonResponse);
                return entries.MetaData.Contents
                    .Where(item => item.IsFolder == false && string.Equals(filename, item.Name, StringComparison.InvariantCultureIgnoreCase))
                    .Select(item => item.FileId)
                    .FirstOrDefault();
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        private static string GetDataCenterHost(DataCenter dataCenter)
        {
            switch (dataCenter)
            {
                case DataCenter.Us:
                    return "api.pcloud.com";
                case DataCenter.Europe:
                    return "eapi.pcloud.com";
                default:
                    throw new InvalidParameterException(nameof(dataCenter));
            }
        }

        private static int GetDataCenterId(DataCenter dataCenter)
        {
            switch (dataCenter)
            {
                case DataCenter.Us:
                    return 1;
                case DataCenter.Europe:
                    return 2;
                default:
                    throw new InvalidParameterException(nameof(dataCenter));
            }
        }

        private static void ThrowIfErrorResponse(string jsonResponse)
        {
            Exception ex = ResponseToException(jsonResponse);
            if (ex != null)
                throw ex;
        }

        private static Exception ResponseToException(string jsonResponse)
        {
            try
            {
                JsonError error = JsonSerializer.Deserialize<JsonError>(jsonResponse);

                // error.Result 0 = successful
                // error.Result 6??? are are legitimate non-error answers
                if ((error.Result != 0) && ((error.Result < 6000) || (error.Result >= 7000)))
                {
                    PcloudApiException apiException = new PcloudApiException(error.Result, error.Error);
                    switch (apiException.Result)
                    {
                        case 2011: // Requested speed limit too low, see minspeed for minimum.
                        case 2041: // Connection broken.
                        case 5001: // Internal upload error.
                        case 5002: // Internal error, no servers available. Try again later.
                            return new ConnectionFailedException(apiException);
                        case 1000: // Log in required.
                        case 2000: // Log in failed.
                        case 2003: // Access denied. You do not have permissions to preform this operation.
                        case 2094: // Invalid 'access_token' provided.
                        case 4000: // Too many login tries from this IP address.
                            return new AccessDeniedException(apiException);
                        default:
                            return new CloudStorageException("Error", apiException);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }

        /// <summary>
        /// Json (de)serialization class.
        /// </summary>
        private class JsonListFolder
        {
            [JsonPropertyName("metadata")]
            public JsonMetaData MetaData { get; set; }
        }

        private class JsonMetaData
        {
            [JsonPropertyName("contents")]
            public List<JsonMetaDataContent> Contents { get; set; }
        }

        private class JsonMetaDataContent
        {
            [JsonPropertyName("isfolder")]
            public bool IsFolder { get; set; }

            [JsonPropertyName("folderid")]
            public long? FolderId { get; set; }

            [JsonPropertyName("fileid")]
            public long? FileId { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("path")]
            public string Path { get; set; }
        }

        private class JsonFileLink
        {
            [JsonPropertyName("path")]
            public string Path { get; set; }

            [JsonPropertyName("hosts")]
            public List<string> Hosts { get; set; }
        }

        private class JsonError
        {
            [JsonPropertyName("result")]
            public long Result { get; set; }

            [JsonPropertyName("error")]
            public string Error { get; set; }
        }

        /// <summary>
        /// Exception describing an error feedback of the PCloud API.
        /// </summary>
        public class PcloudApiException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PcloudApiException"/> class.
            /// </summary>
            /// <param name="result">Sets the <see cref="Result"/> property.</param>
            /// <param name="error">Sets the <see cref="Error"/> property.</param>
            public PcloudApiException(long result, string error)
            {
                Result = result;
                Error = error;
            }

            /// <summary>
            /// Gets the error code from the pCloud API request.
            /// </summary>
            public long Result { get; }

            /// <summary>
            /// Gets the error description from the pCloud API request.
            /// </summary>
            public string Error { get; }
        }
    }
}
