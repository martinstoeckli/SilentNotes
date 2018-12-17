// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services.CloudStorageServices
{
    /// <summary>
    /// Additional functionality for OpenAuth2 logins.
    /// </summary>
    public interface IOauth2CloudStorageService : ICloudStorageService
    {
        /// <summary>
        /// Opens the login page of the OAuth2 service in an external browser.
        /// </summary>
        void ShowOauth2LoginPage();

        /// <summary>
        /// When the user accepts the request, the OAuth2 site will redirect to a given url.
        /// Pass this url to the cloud storage service, so it can handle the response.
        /// </summary>
        /// <param name="responseUrl">Redirect url from the OAuth2 site.</param>
        void HandleOauth2Redirect(Uri responseUrl);

        /// <summary>
        /// Gets or sets the event which is called as a reaction of <see cref="HandleOauth2Redirect(Uri)"/>
        /// and contains the information extracted from the redirect.
        /// </summary>
        EventHandler<RedirectedEventArgs> Redirected { get; set; }
    }

    /// <summary>
    /// The event arguments of the <see cref="Oauth2CloudStorageServiceBase.Redirected"/> event.
    /// </summary>
    public class RedirectedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectedEventArgs"/> class.
        /// </summary>
        /// <param name="redirectResult">Result type of the redirect.</param>
        public RedirectedEventArgs(Oauth2RedirectResult redirectResult)
        {
            RedirectResult = redirectResult;
        }

        /// <summary>
        /// Gets the result type of the redirect.
        /// </summary>
        public Oauth2RedirectResult RedirectResult { get; private set; }

        /// <summary>
        /// Gets or sets the OAuth2 access token.
        /// </summary>
        public string OauthAccessToken { get; set; }

        /// <summary>
        /// Gets or sets the OAuth2 refresh token.
        /// </summary>
        public string OauthRefreshToken { get; set; }
    }

    /// <summary>
    /// Enumeration of all possible redirect results.
    /// </summary>
    public enum Oauth2RedirectResult
    {
        Permitted,
        Rejected
    }
}
