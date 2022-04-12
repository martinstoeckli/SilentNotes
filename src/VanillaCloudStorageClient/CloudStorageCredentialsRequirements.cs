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
    [Flags]
    public enum CloudStorageCredentialsRequirements
    {
        /// <summary>No requirements or not set.</summary>
        None = 0,

        /// <summary>The storage client needs an OAuth2 access token.</summary>
        Token = 1,

        /// <summary>The storage client needs a user name.</summary>
        Username = 2,

        /// <summary>The storage client needs a password.</summary>
        Password = 4,

        /// <summary>The storage client needs just an url.</summary>
        Url = 8,

        /// <summary>The storage client needs a secure flag.</summary>
        Secure = 16,

        /// <summary>The storage client accepts unsafe certificates.</summary>
        AcceptUnsafeCertificate = 32,
    }

    /// <summary>
    /// Extension methods for the <see cref="CloudStorageCredentialsRequirements"/> class.
    /// </summary>
    public static class CloudStorageCredentialsRequirementsExtensions
    {
        /// <summary>
        /// Checks whether a given requirement is set (type-safe <see cref="Enum.HasFlag(Enum)"/>).
        /// </summary>
        /// <param name="requirements">Requirements which may or may not contain the specified requirement.</param>
        /// <param name="requirement">Requirement we want to check if it is contained in <paramref name="requirements"/>.</param>
        /// <returns>Returns true if it requires a secure flag, otherwise false.</returns>
        public static bool HasRequirement(this CloudStorageCredentialsRequirements requirements, CloudStorageCredentialsRequirements requirement)
        {
            return (requirements & requirement) == requirement;
        }
    }
}
