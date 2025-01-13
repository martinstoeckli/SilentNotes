// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace VanillaCloudStorageClient
{
    /// <summary>
    /// Holds the information of an OAuth2 token.
    /// </summary>
    public class CloudStorageToken
    {
        private DateTime? _expiryDate;

        /// <summary>
        /// Gets or sets the access token, which can be passed along future calls to the OAuth2
        /// service, to use its API.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Gets or sets the expiry time of the <see cref="AccessToken"/> in UTC. After this time,
        /// the <see cref="RefreshToken"/> should be used to get a new access token. If an expiry
        /// date of another time zone is set, it will be converted to UTC automatically.
        /// </summary>
        /// <remarks>Some services do not return a <see cref="RefreshToken"/>, in this case the
        /// <see cref="AccessToken"/> usually does not expire and the expiry date can be null.
        /// </remarks>
        public DateTime? ExpiryDate
        {
            get { return _expiryDate; }

            set
            {
                if (value.HasValue)
                    _expiryDate = value.Value.ToUniversalTime();
                else
                    _expiryDate = null;
            }
        }

        /// <summary>
        /// Gets or sets the refresh token, which can be used to get new access tokens after they
        /// expire.
        /// </summary>
        /// <remarks>Some services do not return a refresh token, in this case the <see cref="AccessToken"/>
        /// usually does not expire and the expiry date can be null.
        /// </remarks>
        public string RefreshToken { get; set; }
    }

    /// <summary>
    /// Extension methods for the <see cref="CloudStorageToken"/> class.
    /// </summary>
    public static class CloudStorageTokenExtensions
    {
        /// <summary>
        /// Checks whether the content of two <see cref="CloudStorageToken"/> instances are equal,
        /// or if both are null.
        /// </summary>
        /// <param name="token1">First token.</param>
        /// <param name="token2">Other token.</param>
        /// <returns>Returns true if the tokens are equal, otherwise false.</returns>
        public static bool AreEqualOrNull(this CloudStorageToken token1, CloudStorageToken token2)
        {
            if ((token1 == null) && (token2 == null))
                return true;

            return (token1 != null) && (token2 != null)
                && token1.AccessToken == token2.AccessToken
                && token1.RefreshToken == token2.RefreshToken
                && token1.ExpiryDate == token2.ExpiryDate;
        }

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
                token.ExpiryDate = DateTime.UtcNow.AddSeconds(seconds.Value - tolerance);
            }
        }

        /// <summary>
        /// Checks whether the access token should be refreshed, or if it can still be used.
        /// </summary>
        /// <param name="token">The token with the expiry date.</param>
        /// <returns>Returns true if the token should be refreshed, otherwise false.</returns>
        public static bool NeedsRefresh(this CloudStorageToken token)
        {
            if (token?.RefreshToken == null)
                return false;

            if (token.ExpiryDate != null)
                return token.ExpiryDate < DateTime.UtcNow;
            else
                return true;
        }
    }
}
