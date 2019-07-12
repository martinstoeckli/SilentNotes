// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using SilentNotes.Crypto;

namespace SilentNotes.Services.CloudStorageServices
{
    /// <summary>
    /// Implementation of the <see cref="ICloudStorageService"/>,
    /// which can handle cloud storage with dropbox.
    /// </summary>
    /// <remarks>
    /// 10.2018: Unfortunately Dropbox supports the code flow exclusively with "https://*" redirect
    /// urls. User schemes like "myapp://*" are only available for the implizit flow. Intercepting
    /// "https://*" calls in UWP can be done adding the uap3:AppUriHandler in the app manifest, but
    /// it requires a working domain with a valid SSL certificate (ideally a subdomain per app),
    /// which must offer a json file. That means, that the app would depend on a working website.
    /// </remarks>
    public class DropboxCloudStorageService : IOauth2CloudStorageService
    {
        private const string _obfuscationKey = "4ed05d88-0193-4b14-9b0b-6977825de265";
        private const string _obfuscatedAppKey = "b2JmdXNjYXRpb24kdHdvZmlzaF9nY20kaUNDQnhZRDFqTG4veUJQNSRwYmtkZjIkNEVrVTFOVVdZSkJpTWtQR2VNU0lhdz09JDEwMDAkqkhIg8kDs04BHHfD2Dldq7jC8LUT3AqPnyY6THmJJw==";
        private const string _authorizeUrl = "https://www.dropbox.com/oauth2/authorize";
        private const string _redirectUrl = "ch.martinstoeckli.silentnotes://oauth2redirect/";
        private readonly INativeBrowserService _nativeBrowserService;
        private readonly ICryptoRandomService _randomSource;
        private string _oauthState;

        /// <summary>
        /// Initializes a new instance of the <see cref="DropboxCloudStorageService"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public DropboxCloudStorageService(INativeBrowserService nativeBrowserService, ICryptoRandomService randomSource)
            : this(new CloudStorageAccount { CloudType = CloudStorageType.Dropbox }, nativeBrowserService, randomSource)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DropboxCloudStorageService"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public DropboxCloudStorageService(CloudStorageAccount account, INativeBrowserService nativeBrowserService, ICryptoRandomService randomSource)
        {
            if (account.CloudType != CloudStorageType.Dropbox)
                throw new Exception("Invalid account information for DropboxCloudStorageService, expected cloud storage type Dropbox");

            Account = account;
            _nativeBrowserService = nativeBrowserService;
            _randomSource = randomSource;
        }

        /// <summary>
        /// Gets the account information of this
        /// </summary>
        public CloudStorageAccount Account { get; private set; }

        /// <inheritdoc/>
        public async Task<bool> ExistsRepositoryAsync()
        {
            DropboxClient client = new DropboxClient(Account.OauthAccessToken);
            List<string> filenames = await ListFileNamesAsync(client);
            return filenames.Contains(Config.RepositoryFileName, StringComparer.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public async Task<byte[]> DownloadRepositoryAsync()
        {
            try
            {
                string path = GetDropboxRepositoryPath();
                DropboxClient client = new DropboxClient(Account.OauthAccessToken);
                using (var response = await client.Files.DownloadAsync(path))
                {
                    return await response.GetContentAsByteArrayAsync();
                }
            }
            catch (Exception ex)
            {
                ThrowCloudStorageException(ex);
                return null; // return is never reached
            }
        }

        /// <inheritdoc/>
        public async Task UploadRepositoryAsync(byte[] repository)
        {
            try
            {
                string path = GetDropboxRepositoryPath();
                DropboxClient client = new DropboxClient(Account.OauthAccessToken);
                using (var stream = new MemoryStream(repository))
                {
                    CommitInfo info = new CommitInfo(path, WriteMode.Overwrite.Instance, false, null, false);
                    FileMetadata response = await client.Files.UploadAsync(info, stream);
                }
            }
            catch (Exception ex)
            {
                ThrowCloudStorageException(ex);
            }
        }

        /// <inheritdoc/>
        public void ShowOauth2LoginPage()
        {
            // Example url: https://www.dropbox.com/oauth2/authorize?response_type=token&client_id=xxxxxxxxxx&redirect_uri=ch.martinstoeckli.silentnotes%3A%2F%2Foauth2redirect%2F&state=xxxxxxxxxx
            _oauthState = CryptoUtils.GenerateRandomBase62String(16, _randomSource);
            Uri oauthUrl = DropboxOAuth2Helper.GetAuthorizeUri(
                OAuthResponseType.Token, GetAppKey(), new Uri(_redirectUrl), _oauthState);
            _nativeBrowserService.OpenWebsiteInApp(oauthUrl.AbsoluteUri);
        }

        /// <inheritdoc/>
        public void HandleOauth2Redirect(Uri responseUrl)
        {
            RedirectedEventArgs eventArgs;
            if (responseUrl.AbsoluteUri.Contains("access_denied"))
            {
                // Example response: ch.martinstoeckli.silentnotes://oauth2redirect/#state=xxxxxxxxxx&error_description=The+user+chose+not+to+give+your+app+access+to+their+Dropbox+account.&error=access_denied
                eventArgs = new RedirectedEventArgs(Oauth2RedirectResult.Rejected);
            }
            else
            {
                // Example response: ch.martinstoeckli.silentnotes://oauth2redirect/#access_token=xxxxxxxxxx&token_type=bearer&state=xxxxxxxxxx&uid=xxxxxxxxxx&account_id=xxxxxxxxxx
                OAuth2Response response = DropboxOAuth2Helper.ParseTokenFragment(responseUrl);
                if (_oauthState != response.State)
                    throw new CloudStorageException();

                eventArgs = new RedirectedEventArgs(Oauth2RedirectResult.Permitted);
                eventArgs.OauthAccessToken = response.AccessToken;
            }

            // Raise event
            if (Redirected != null)
                Redirected(this, eventArgs);
        }

        /// <summary>
        /// Gets or sets the event which is called when the OAuth2 redirect has finished.
        /// </summary>
        public EventHandler<RedirectedEventArgs> Redirected { get; set; }

        /// <summary>
        /// Gets the dropbox application key. This key is defined by dropbox for the given app and
        /// can be received on the developers page: https://www.dropbox.com/developers/apps
        /// The application key is not necessarily secret, but we don't want to spread it neither.
        /// </summary>
        /// <returns>The SilentNotes application key for DropBox.</returns>
        private string GetAppKey()
        {
            return CryptoUtils.Deobfuscate(_obfuscatedAppKey, _obfuscationKey);
        }

        /// <summary>
        /// Returns a list of filenames in the Webdav directory.
        /// </summary>
        /// <param name="client">The dropbox client object.</param>
        /// <returns>List of filenames, not including the directory path.</returns>
        private async Task<List<string>> ListFileNamesAsync(DropboxClient client)
        {
            try
            {
                List<string> result = new List<string>();
                ListFolderArg args = new ListFolderArg(string.Empty); // empty string specifies the root folder
                ListFolderResult dropboxItems = await client.Files.ListFolderAsync(args);
                foreach (Metadata item in dropboxItems.Entries.Where(i => i.IsFile))
                {
                    FileMetadata file = item.AsFile;
                    result.Add(file.Name);
                }
                return result;
            }
            catch (Exception ex)
            {
                ThrowCloudStorageException(ex);
                return null; // return is never reached
            }
        }

        private string GetDropboxRepositoryPath()
        {
            // Each registered application has a dropbox application root directory.
            // Out repository is located directly in this directory.
            return "/" + Config.RepositoryFileName;
        }

        /// <summary>
        /// Translates a WebException to its <see cref="CloudStorageException"/> pendant and throws it.
        /// </summary>
        /// <param name="ex">A web exception to translate.</param>
        private void ThrowCloudStorageException(Exception ex)
        {
            if ((ex is Dropbox.Api.AuthException) || (ex is Dropbox.Api.AccessException) || (ex is Dropbox.Api.OAuth2Exception))
            {
                throw new CloudStorageForbiddenException();
            }

            Exception innerException = ex.InnerException;
            if ((innerException != null) && (innerException is WebException))
            {
                WebException webException = innerException as WebException;
                if ((webException.Status == WebExceptionStatus.NameResolutionFailure) || webException.Status == WebExceptionStatus.ConnectFailure)
                    throw new CloudStorageConnectionException();
            }

            if (ex is Dropbox.Api.HttpException)
            {
                throw new CloudStorageConnectionException();
            }

            throw new CloudStorageException();
        }
    }
}
