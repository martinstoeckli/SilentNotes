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
}
