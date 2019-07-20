// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;

namespace VanillaCloudStorageClient.OAuth2
{
    /// <summary>
    /// The required parameters to make an OAuth2 authorization request.
    /// </summary>
    public class OAuth2Config
    {
        /// <summary>
        /// Gets or sets the url to the OAuth2 service endpoint, which should be used to display
        /// the service's login page.
        /// <example>
        /// https://www.dropbox.com/oauth2/authorize
        /// </example>
        /// </summary>
        public string AuthorizeServiceEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the url to the OAuth2 service endpoint, which can be used to get and
        /// refresh an access token. Only required for the code-flow <see cref="AuthorizationFlow.Code"/>.
        /// <example>
        /// https://www.googleapis.com/oauth2/v4/token
        /// </example>
        /// </summary>
        public string TokenServiceEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the client-id/app-key of the app which requires access to the OAuth2
        /// service. This client-id can be read form the OAuth2 service, after the app is
        /// registered.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets a redirect url, which can be called by the OAuth2 service, to send the
        /// authorization response. For native apps this is often a self registered protocol,
        /// instead of an http url.
        /// <example>
        /// com.example.myapp://oauth2redirect/
        /// </example>
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Gets or sets the scope the application requires. The scope depends on the OAuth2
        /// service, e.g. for Google drive it is https://www.googleapis.com/auth/drive.appdata
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Gets or sets the expected response type. With this parameter we decide whether to use
        /// the token-flow or the code-flow.
        /// </summary>
        public AuthorizationFlow Flow { get; set; }
    }

    /// <summary>
    /// Extension methods for the <see cref="OAuth2Config"/> class.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Just extension methods for the same class.")]
    public static class OAuth2ConfigExtensions
    {
        /// <summary>
        /// Validates the request parameters.
        /// </summary>
        /// <param name="requestParams">The request parameters to validate.</param>
        /// <exception cref="InvalidParameterException">Is thrown when the parameters are invalid.</exception>
        public static void ThrowIfInvalidForAuthorizationRequest(this OAuth2Config requestParams)
        {
            if (requestParams == null)
                throw new InvalidParameterException(nameof(OAuth2Config));

            if (string.IsNullOrWhiteSpace(requestParams.AuthorizeServiceEndpoint))
                throw new InvalidParameterException(string.Format("{0}.{1}", nameof(OAuth2Config), nameof(OAuth2Config.AuthorizeServiceEndpoint)));
            if (string.IsNullOrWhiteSpace(requestParams.ClientId))
                throw new InvalidParameterException(string.Format("{0}.{1}", nameof(OAuth2Config), nameof(OAuth2Config.ClientId)));
            if (string.IsNullOrWhiteSpace(requestParams.RedirectUrl))
                throw new InvalidParameterException(string.Format("{0}.{1}", nameof(OAuth2Config), nameof(OAuth2Config.RedirectUrl)));
        }
    }
}
