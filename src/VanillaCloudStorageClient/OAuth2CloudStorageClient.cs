// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json;
using VanillaCloudStorageClient.OAuth2;

namespace VanillaCloudStorageClient
{
    /// <summary>
    /// Base class for for implementations of the <see cref="ICloudStorageClient"/> interface,
    /// which authorize with the OAuth2 protocol.
    /// </summary>
    public abstract class OAuth2CloudStorageClient : CloudStorageClientBase, ICloudStorageClient, IOAuth2CloudStorageClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OAuth2CloudStorageClient"/> class.
        /// </summary>
        /// <param name="config">Sets the <see cref="Config"/> property.</param>
        protected OAuth2CloudStorageClient(OAuth2Config config)
        {
            Config = config;
        }

        /// <summary>
        /// Gets the necessary parameters to do the OAuth2 authorization.
        /// </summary>
        protected OAuth2Config Config { get; private set; }

        #region IOAuth2CloudStorageClient

        /// <inheritdoc/>
        public virtual string BuildAuthorizationRequestUrl(string state, string codeVerifier)
        {
            return OAuth2Utils.BuildAuthorizationRequestUrl(Config, state, codeVerifier);
        }

        /// <inheritdoc/>
        public virtual async Task<CloudStorageToken> FetchTokenAsync(string redirectedUrl, string state, string codeVerifier)
        {
            if (string.IsNullOrWhiteSpace(redirectedUrl))
                throw new ArgumentNullException(nameof(redirectedUrl));
            if (string.IsNullOrWhiteSpace(state))
                throw new ArgumentNullException(nameof(state));

            AuthorizationResponse response = OAuth2Utils.ParseAuthorizationResponseUrl(redirectedUrl);

            // Check whether the user denied access
            if (!response.IsAccessGranted)
                return null;

            // Verify state
            if (response.State != state)
                throw new CloudStorageException("The authorization response has a wrong state, this indicates a hacking attempt.", null);

            // Determine flow type
            AuthorizationFlow flow;
            if (!string.IsNullOrWhiteSpace(response.Token))
                flow = AuthorizationFlow.Token;
            else if (!string.IsNullOrWhiteSpace(response.Code))
                flow = AuthorizationFlow.Code;
            else
                throw new CloudStorageException("The authorization response is neither form a token-flow, nor from a code-flow request.", null);

            switch (flow)
            {
                case AuthorizationFlow.Token:
                    return new CloudStorageToken
                    {
                        AccessToken = response.Token,
                        ExpiryDate = null,
                        RefreshToken = null
                    };
                case AuthorizationFlow.Code:
                    return await ExchangeCodeForTokenAsync(response.Code, codeVerifier);
                default:
                    return null; // Never happens
            }
        }

        /// <inheritdoc/>
        public virtual async Task<CloudStorageToken> RefreshTokenAsync(CloudStorageToken token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));
            if (string.IsNullOrWhiteSpace(token.RefreshToken))
                throw new ArgumentNullException(string.Format("{0}.{1}", nameof(token), nameof(CloudStorageToken.RefreshToken)));

            try
            {
                string jsonResponse = await Flurl.Request(Config.TokenServiceEndpoint)
                    .PostUrlEncodedAsync(new
                    {
                        refresh_token = token.RefreshToken,
                        client_id = Config.ClientId,
                        client_secret = string.Empty, // Installable apps cannot keep secrets, thus the client secret is not transmitted
                        grant_type = "refresh_token",
                    })
                    .ReceiveString();

                JsonTokenExchangeResponse response = JsonConvert.DeserializeObject<JsonTokenExchangeResponse>(jsonResponse);
                CloudStorageToken result = new CloudStorageToken
                {
                    AccessToken = response.AccessToken,
                    RefreshToken = token.RefreshToken // keep the refresh token
                };
                result.SetExpiryDateBySecondsFromNow(response.ExpiresIn);
                return result;
            }
            catch (Exception ex)
            {
                if (await IsInvalidGrantException(ex))
                    throw new RefreshTokenExpiredException(ex);
                throw ConvertToCloudStorageException(ex);
            }
        }

        #endregion

        /// <summary>
        /// If the OAuth2 service requires the code flow, we have to exchange the authorization
        /// code with an access token, which can be used to access the API of the cloud storage
        /// service.
        /// </summary>
        /// <param name="authorizationCode">The code form the authorization request, it can be
        /// found in <see cref="AuthorizationResponse.Code"/>.
        /// </param>
        /// <param name="codeVerifier">The optional code verifier.</param>
        /// <returns>A token object containing the access token and a refresh token.</returns>
        protected virtual async Task<CloudStorageToken> ExchangeCodeForTokenAsync(
            string authorizationCode, string codeVerifier)
        {
            if (string.IsNullOrWhiteSpace(authorizationCode))
                throw new InvalidParameterException(nameof(authorizationCode));

            try
            {
                string jsonResponse = await Flurl.Request(Config.TokenServiceEndpoint)
                    .PostUrlEncodedAsync(new
                    {
                        code = authorizationCode,
                        client_id = Config.ClientId,
                        client_secret = string.Empty, // Installable apps cannot keep secrets, thus the client secret is not transmitted
                        redirect_uri = Config.RedirectUrl,
                        grant_type = "authorization_code",
                        code_verifier = codeVerifier,
                    })
                    .ReceiveString();

                JsonTokenExchangeResponse response = JsonConvert.DeserializeObject<JsonTokenExchangeResponse>(jsonResponse);
                CloudStorageToken result = new CloudStorageToken
                {
                    AccessToken = response.AccessToken,
                    RefreshToken = response.RefreshToken
                };
                result.SetExpiryDateBySecondsFromNow(response.ExpiresIn);
                return result;
            }
            catch (Exception ex)
            {
                throw ConvertToCloudStorageException(ex);
            }
        }

        private async Task<bool> IsInvalidGrantException(Exception ex)
        {
            HttpStatusCode[] possibleCodes = { HttpStatusCode.BadRequest, HttpStatusCode.Unauthorized, HttpStatusCode.Forbidden };
            if ((ex is FlurlHttpException flurlHttpException) &&
                (flurlHttpException.Call?.HttpStatus != null) &&
                possibleCodes.Contains(flurlHttpException.Call.HttpStatus.Value))
            {
                string jsonResponse = await flurlHttpException.GetResponseStringAsync();
                return (jsonResponse != null) && (jsonResponse.IndexOf("invalid_grant", StringComparison.InvariantCultureIgnoreCase) > 0);
            }
            return false;
        }

        /// <summary>
        /// Json (de)serialization class.
        /// </summary>
        private class JsonTokenExchangeResponse
        {
            [JsonProperty(PropertyName = "access_token")]
            public string AccessToken { get; set; }

            [JsonProperty(PropertyName = "refresh_token")]
            public string RefreshToken { get; set; }

            [JsonProperty(PropertyName = "expires_in")]
            public int? ExpiresIn { get; set; }
        }
    }
}
