// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;

namespace VanillaCloudStorageClient
{
    /// <summary>
    /// Holds the information of an OAuth2 token.
    /// </summary>
    public class CloudStorageToken
    {
        /// <summary>
        /// Gets or sets the access token, which can be passed along future calls to the OAuth2
        /// service, to use its API.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the expiry time of the <see cref="AccessToken"/>. After this time, the
        /// <see cref="RefreshToken"/> can be used to get a new access token.
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Gets or sets the refresh token, which can be used to get new access tokens after they
        /// expire.
        /// </summary>
        public string RefreshToken { get; set; }
    }

    /// <summary>
    /// Extension methods for the <see cref="CloudStorageToken"/> class.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Just extension methods for the same class.")]
    public static class CloudStorageTokenExtensions
    {
        /// <summary>
        /// Sets the expiry date, by calculating the date with a number of seconds, as an OAuth2
        /// request returns.
        /// </summary>
        /// <param name="token">The token whose expiry date to set.</param>
        /// <param name="seconds">Number of seconds the token is valid, or null if this information
        /// was not delivered by the request.</param>
        public static void SetExpiryDateBySecondsFromNow(this CloudStorageToken token, int? seconds)
        {
            if (seconds == null)
            {
                token.ExpiryDate = null;
            }
            else
            {
                // Decrease time by a tolerance (10%) but not more than one minute
                double tolerance = Math.Min(60.0, seconds.Value / 10.0);
                token.ExpiryDate = DateTime.Now.AddSeconds(seconds.Value - tolerance);
            }
        }

        /// <summary>
        /// Checks whether the access token should be refreshed, or if it can still be used.
        /// </summary>
        /// <param name="token">The token with the expiry date.</param>
        /// <returns>Returns true if the token should be refreshed, otherwise false.</returns>
        public static bool NeedsRefresh(this CloudStorageToken token)
        {
            if ((token == null) || (token.RefreshToken == null))
                return false;

            if (token.ExpiryDate != null)
                return token.ExpiryDate < DateTime.Now;
            else
                return true;
        }
    }
}
