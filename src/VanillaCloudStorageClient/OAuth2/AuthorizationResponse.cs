// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace VanillaCloudStorageClient.OAuth2
{
    /// <summary>
    /// Holds the information of a response form the OAuth2 service, after an authorization request.
    /// </summary>
    public class AuthorizationResponse
    {
        /// <summary>
        /// Gets a value indicating whether the app got access to the OAuth2 service. If no access
        /// was granted, the <see cref="Error"/> describes the reason.
        /// </summary>
        public bool IsAccessGranted
        {
            get { return Error == null; }
        }

        /// <summary>
        /// Gets or sets the access token, which can be passed along future calls to the OAuth2
        /// service, for authorization. The token is only available in the code-flow requested
        /// with <see cref="AuthorizationFlow.Token"/>.
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets the retrieved code from the OAuth2 service response. The code is only
        /// available in the token-flow requested with <see cref="AuthorizationFlow.Code"/>.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the retrieved state from the OAuth2 service response. The state must
        /// match the state passed in the request, otherwise it indicates a hacking attempt.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the error type of the response, in case of an error. If the response was
        /// successful, the error is null.
        /// </summary>
        public AuthorizationResponseError? Error { get; set; }
    }
}
