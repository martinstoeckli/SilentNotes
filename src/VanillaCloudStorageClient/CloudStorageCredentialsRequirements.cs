// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace VanillaCloudStorageClient
{
    /// <summary>
    /// Enumeration of possible requirements for credentials. Providers Every <see cref="ICloudStorageClient"/>
    /// provides this information, which can be used for validation and to display the correct
    /// controls in a view.
    /// </summary>
    public enum CloudStorageCredentialsRequirements
    {
        /// <summary>
        /// The storage client needs just an OAuth2 access token.
        /// </summary>
        Token,

        /// <summary>
        /// The storage client needs a username and a password.
        /// </summary>
        UsernamePassword,

        /// <summary>
        /// The storage client needs a username, a password and an Url
        /// </summary>
        UsernamePasswordUrl,

        /// <summary>
        /// The storage client needs a username, a password, an Url and the secure flag.
        /// </summary>
        UsernamePasswordUrlSecure,
    }

    /// <summary>
    /// Extension methods for the <see cref="CloudStorageCredentialsRequirements"/> class.
    /// </summary>
    public static class CloudStorageCredentialsRequirementsExtensions
    {
        /// <summary>
        /// Checks whether this requirement needs an OAuth2 token.
        /// </summary>
        /// <param name="requirements">Requirement to check.</param>
        /// <returns>Returns true if if requires a token, otherwise false.</returns>
        public static bool NeedsToken(this CloudStorageCredentialsRequirements requirements)
        {
            return requirements == CloudStorageCredentialsRequirements.Token;
        }

        /// <summary>
        /// Checks whether this requirement needs a user name.
        /// </summary>
        /// <param name="requirements">Requirement to check.</param>
        /// <returns>Returns true if if requires a user name, otherwise false.</returns>
        public static bool NeedsUsername(this CloudStorageCredentialsRequirements requirements)
        {
            return requirements != CloudStorageCredentialsRequirements.Token;
        }

        /// <summary>
        /// Checks whether this requirement needs a password.
        /// </summary>
        /// <param name="requirements">Requirement to check.</param>
        /// <returns>Returns true if if requires a password, otherwise false.</returns>
        public static bool NeedsPassword(this CloudStorageCredentialsRequirements requirements)
        {
            return requirements != CloudStorageCredentialsRequirements.Token;
        }

        /// <summary>
        /// Checks whether this requirement needs a url.
        /// </summary>
        /// <param name="requirements">Requirement to check.</param>
        /// <returns>Returns true if if requires a url, otherwise false.</returns>
        public static bool NeedsUrl(this CloudStorageCredentialsRequirements requirements)
        {
            return requirements == CloudStorageCredentialsRequirements.UsernamePasswordUrl ||
                requirements == CloudStorageCredentialsRequirements.UsernamePasswordUrlSecure;
        }

        /// <summary>
        /// Checks whether this requirement needs a secure flag.
        /// </summary>
        /// <param name="requirements">Requirement to check.</param>
        /// <returns>Returns true if if requires a secure flag, otherwise false.</returns>
        public static bool NeedsSecureFlag(this CloudStorageCredentialsRequirements requirements)
        {
            return requirements == CloudStorageCredentialsRequirements.UsernamePasswordUrlSecure;
        }
    }
}
