// Copyright © 2024 Martin Stoeckli.
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
    public class PcloudCloudStorageProviderClient : OAuth2CloudStorageClient, ICloudStorageClient, IOAuth2CloudStorageClient
    {
        private const string AuthorizeUrl = "https://my.pcloud.com/oauth2/authorize";
        private const string TokenUrl = "https://{0}/oauth2_token";
        private const string UploadUrl = "https://{0}/uploadfile";
        private const string DownloadUrl = "https://{0}/downloadfile";
        private const string DeleteUrl = "https://{0}/deletefile";
        private const string ListUrl = "https://{0}/listfolder";

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleCloudStorageClient"/> class.
        /// </summary>
        /// <param name="oauthClientId">Sets the <see cref="OAuth2Config.ClientId"/> property.</param>
        /// <param name="oauthRedirectUrl">Sets the <see cref="OAuth2Config.RedirectUrl"/> property.</param>
        /// <param name="dataCenter">The data center used by the pCloud user.</param>
        public PcloudCloudStorageProviderClient(string oauthClientId, string oauthRedirectUrl, DataCenter dataCenter)
            : base(new OAuth2Config
            {
                AuthorizeServiceEndpoint = AuthorizeUrl,
                TokenServiceEndpoint = string.Format(TokenUrl, GetDataCenterHost(dataCenter)),
                ClientId = oauthClientId,
                RedirectUrl = oauthRedirectUrl,
                Flow = AuthorizationFlow.Code,
                Scope = null,
                ClientSecretHandling = ClientSecretHandling.DoNotSend,
            })
        {
        }

        /// <summary>
        /// Enumeration of known data centers, which determine the API urls.
        /// </summary>
        public enum DataCenter
        {
            Us,
            Europe,
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
            throw new NotImplementedException();
        }

        private async Task UpdateExistingFile(string fileId, byte[] fileContent, CloudStorageCredentials credentials)
        {
            throw new NotImplementedException();
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

                throw new NotImplementedException();
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

                throw new NotImplementedException();
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
                throw new NotImplementedException();
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
                throw new NotImplementedException();
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }
    }
}
