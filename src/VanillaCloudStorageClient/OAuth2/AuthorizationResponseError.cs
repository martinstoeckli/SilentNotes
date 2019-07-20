// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;

namespace VanillaCloudStorageClient.OAuth2
{
    /// <summary>
    /// Enumeration of all possible authorization response errors.
    /// See: https://tools.ietf.org/html/rfc6749#section-4.1.2.1
    /// </summary>
    public enum AuthorizationResponseError
    {
        /// <summary>
        /// The OAuth2 service returned an unknown error which is not part of the standard.
        /// </summary>
        Unknown,

        /// <summary>
        /// The request is missing a required parameter, includes an invalid parameter value,
        /// includes a parameter more than once, or is otherwise malformed.
        /// </summary>
        InvalidRequest,

        /// <summary>
        /// The client is not authorized to request an authorization code using this method.
        /// </summary>
        UnauthorizedClient,

        /// <summary>
        /// The resource owner or authorization server denied the request.
        /// </summary>
        AccessDenied,

        /// <summary>
        /// The authorization server does not support obtaining an authorization code using this method.
        /// </summary>
        UnsupportedResponseType,

        /// <summary>
        /// The requested scope is invalid, unknown, or malformed.
        /// </summary>
        InvalidScope,

        /// <summary>
        /// The authorization server encountered an unexpected condition that prevented it from
        /// fulfilling the request. (This error code is needed because a 500 Internal Server Error
        /// HTTP status code cannot be returned to the client via an HTTP redirect.)
        /// </summary>
        ServerError,

        /// <summary>
        /// The authorization server is currently unable to handle the request due to a temporary
        /// overloading or maintenance of the server.  (This error code is needed because a 503
        /// Service Unavailable HTTP status code cannot be returned to the client via an HTTP
        /// redirect.)
        /// </summary>
        TemporarilyUnavailable,
    }

    /// <summary>
    /// Extension methods for the <see cref="AuthorizationResponseError"/> enumeration.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Just extension methods for the same class.")]
    public static class AuthorizationResponseErrorExtensions
    {
        /// <summary>
        /// Converts the error code returned by an OAuth2 service to an error.
        /// </summary>
        /// <param name="error">The returned error code.</param>
        /// <returns>The error.</returns>
        public static AuthorizationResponseError? StringToAuthorizationResponseError(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
                return null;

            error = error.Replace("_", string.Empty); // Remove snake_case
            if (Enum.TryParse(error, true, out AuthorizationResponseError result))
                return result;
            return AuthorizationResponseError.Unknown;
        }
    }
}
