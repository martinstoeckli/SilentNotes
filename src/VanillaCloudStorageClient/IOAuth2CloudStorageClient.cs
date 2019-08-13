// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;

namespace VanillaCloudStorageClient
{
    /// <summary>
    /// Any <see cref="ICloudStorageClient"/> can support this interface, if it has to authenticate
    /// to an OAuth2 service.
    /// <example>
    /// The Dropbox client would implement this interface, while the FTP client would not.
    /// </example>
    /// </summary>
    public interface IOAuth2CloudStorageClient
    {
        /// <summary>
        /// Builds the url which shows the authorization page of the OAuth2 web service, where the
        /// user can confirm that the app is allowed to use the service.
        /// </summary>
        /// <remarks>
        /// The authorization page must be displayed with an external browser, this is a security
        /// requirement and is enforced by some OAuth2 services. Some platforms offer to show the
        /// external browser inside the app, like Androids custom tabs.
        /// </remarks>
        /// <param name="state">A random string which will be passed to the OAuth2 web service,
        /// and is returned in its response. An application should persist this state so it can
        /// verify the response.</param>
        /// <param name="codeVerifier">An optional random string [43-128 chars a-z,A-Z,0-9] which
        /// will be passed to the OAuth2 web service. An application should persist this code
        /// verifier, because it is required for the <see cref="FetchTokenAsync(string, string, string)"/>
        /// call.</param>
        /// <returns>A url which can be used to show the authorization page.</returns>
        string BuildAuthorizationRequestUrl(string state, string codeVerifier);

        /// <summary>
        /// Token-flow: The access-token is extracted from the redirect url and returned.
        /// Code-flow: The authorization-code is extracted from the redirect url and is exchanged
        /// to an access-token/redirect-token.
        /// </summary>
        /// <param name="redirectedUrl">The redirect url with parameters which was called by the
        /// OAuth2 service.</param>
        /// <param name="state">Provide the same state which was passed to <see cref="BuildAuthorizationRequestUrl(string, string)"/>.</param>
        /// <param name="codeVerifier">Provide the same verifier which was passed to <see cref="BuildAuthorizationRequestUrl(string, string)"/>.</param>
        /// <returns>Returns a token whose access-token can be used to access the service API, or
        /// null when the user denied access to the service.</returns>
        Task<CloudStorageToken> FetchTokenAsync(string redirectedUrl, string state, string codeVerifier);

        /// <summary>
        /// Makes a token refresh call to the OAuth2 service to get a new access token from the
        /// refresh token. Call this method only if necessary, first check whether it is necessary
        /// with token.NeedsRefresh()/>.
        /// </summary>
        /// <remarks>
        /// Clients which use the token-flow usually don't have to implement this method.
        /// </remarks>
        /// <param name="token">The token holding the access-token and the refresh-token.</param>
        /// <exception cref="RefreshTokenExpiredException">Is thrown when the refresh token has expired
        /// and the user has to do a new authorization.</exception>
        /// <returns>Returns a new refreshed token.</returns>
        Task<CloudStorageToken> RefreshTokenAsync(CloudStorageToken token);
    }
}
