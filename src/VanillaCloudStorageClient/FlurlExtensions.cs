// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Flurl.Http;

namespace VanillaCloudStorageClient
{
    /// <summary>
    /// Additional extension methods to the Flurl library.
    /// </summary>
    public static class FlurlExtensions
    {
        /// <summary>
        /// Sets HTTP authorization header according to Basic Authentication protocol to be sent with this IFlurlRequest or all requests made with this IFlurlClient.
        /// </summary>
        /// <param name="clientOrRequest">The IFlurlClient or IFlurlRequest.</param>
        /// <param name="username">Username, or an empty string for anonymous access.</param>
        /// <param name="password">Password, or an empty string for anonymous access.</param>
        /// <returns>This IFlurlClient or IFlurlRequest.</returns>
        public static T WithBasicAuthOrAnonymous<T>(this T clientOrRequest, string username, string password) where T : IHttpSettingsContainer
        {
            if (string.IsNullOrEmpty(username))
                return clientOrRequest;
            return clientOrRequest.WithBasicAuth(username, password);
        }
    }
}
