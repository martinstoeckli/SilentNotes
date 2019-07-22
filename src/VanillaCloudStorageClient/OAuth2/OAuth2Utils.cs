// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using Flurl;

namespace VanillaCloudStorageClient.OAuth2
{
    /// <summary>
    /// Can be used by client-side apps, to get access to a cloud service like DropBox.
    /// </summary>
    public static class OAuth2Utils
    {
        /// <summary>
        /// Builds the url which shows the authorization page of the OAuth2 web service, where the
        /// user can confirm that the app is allowed to use the service.
        /// </summary>
        /// <remarks>
        /// Client-side apps should show the web page in an external browser (or Android custom tab)
        /// and not inside a WebView of the application (requirement for login with Google account).
        /// </remarks>
        /// <param name="config">The parameters of the OAuth2 service. The parameters
        /// can be read from the OAuth2 web service after registration of the app.</param>
        /// <param name="stateParam">A random string which will be passed to the OAuth2 web service,
        /// and is returned in the response. An application should persist this state so it can
        /// verify the response.</param>
        /// <returns>A url which can be used to show the authorization page.</returns>
        public static string BuildAuthorizationRequestUrl(OAuth2Config config, string stateParam)
        {
            config.ThrowIfInvalidForAuthorizationRequest();

            return new Url(config.AuthorizeServiceEndpoint)
                .SetQueryParams(new
                {
                    response_type = config.Flow.ToString().ToLowerInvariant(),
                    client_id = config.ClientId,
                    redirect_uri = config.RedirectUrl,
                    scope = config.Scope,
                    state = stateParam
                });
        }

        /// <summary>
        /// Parses the url called by the OAuth2 service, after an authorization request.
        /// </summary>
        /// <remarks>
        /// The OAuth2 service uses the <see cref="OAuth2Config.RedirectUrl"/> to
        /// build an url containing the result of the request. This method extracts the parts of
        /// the answer.
        /// </remarks>
        /// <param name="redirectedUrl">The redirected url called by the OAuth2 service.</param>
        /// <returns>The authorization response.</returns>
        public static AuthorizationResponse ParseAuthorizationResponseUrl(string redirectedUrl)
        {
            if (string.IsNullOrWhiteSpace(redirectedUrl))
                throw new ArgumentNullException(nameof(redirectedUrl));

            Url url = new Url(redirectedUrl);

            // The url contains the parameters either in the query part (code-flow) or in the
            // fragment part (token-flow). We move it to query part so we can extract its values.
            if (string.IsNullOrWhiteSpace(url.Query))
                url.Query = url.Fragment;

            return new AuthorizationResponse
            {
                Token = url.QueryParams["access_token"]?.ToString(),
                Code = url.QueryParams["code"]?.ToString(),
                State = url.QueryParams["state"]?.ToString(),
                Error = AuthorizationResponseErrorExtensions.StringToAuthorizationResponseError(url.QueryParams["error"]?.ToString())
            };
        }
    }
}
