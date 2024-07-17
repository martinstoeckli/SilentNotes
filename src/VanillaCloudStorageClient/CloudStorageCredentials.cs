﻿// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.Serialization;
using System.Security;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace VanillaCloudStorageClient
{
    /// <summary>
    /// Holds the credentials to connect to a cloud storage service. Usually this is either an
    /// OAuth2 token, a username/password combination, or a username/password/url combination.
    /// </summary>
    /// <remarks>If you need to store the credentials on a locale device, use the inherited
    /// class <see cref="SerializeableCloudStorageCredentials"/> instead.</remarks>
    [DataContract]
    public class CloudStorageCredentials : IDisposable
    {
        private bool _disposed = false;

        /// <summary>
        /// Gets or sets an id of the cloud storage client.
        /// It is a developer generated id, which should allow to get/create the correct
        /// <see cref="ICloudStorageClient"/>, the credentials where made for.
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public string CloudStorageId { get; set; }

        /// <summary>
        /// Gets or sets the oauth2 access token if necessary, otherwise this is null.
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public CloudStorageToken Token { get; set; }

        /// <summary>
        /// Gets or sets the username for login if necessary, otherwise this is null.
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets a password for login if necessary, otherwise this is null.
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public SecureString Password { get; set; }

        /// <summary>
        /// Gets or sets the url for login if necessary, otherwise this is null.
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an encrypted SSL connection should be used.
        /// This value is currently used onyl by the FTP provider, others will ignore this value.
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public bool Secure { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a SSL certifacte shoudl be accepted, even if
        /// the validation was not successful.
        /// This value is currently used onyl by the FTP provider, others will ignore this value.
        /// </summary>
        [XmlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public bool AcceptInvalidCertificate { get; set; }

        /// <summary>
        /// Gets or sets the plain text password. Keep the usage of this property to a minimum,
        /// try to work with SeureString all the way instead.
        /// </summary>
        /// <remarks>
        /// The password itself will still be stored in the <see cref="Password"/> property.
        /// </remarks>
        [XmlIgnore]
        [JsonIgnore]
        [IgnoreDataMember]
        public string UnprotectedPassword
        {
            get { return Password.SecureStringToString(); }
            set { Password = SecureStringExtensions.StringToSecureString(value); }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;

                // Remove password from memory
                Password?.Dispose();
                Password = null;
            }
        }
    }

    /// <summary>
    /// Extension methods for the <see cref="CloudStorageCredentials"/> class.
    /// </summary>
    public static class CloudStorageCredentialsExtensions
    {
        /// <summary>
        /// Checks whether the content of two <see cref="CloudStorageCredentials"/> instances are equal,
        /// or if both are null.
        /// </summary>
        /// <param name="credentials1">First credentials.</param>
        /// <param name="credentials2">Other credentials.</param>
        /// <returns>Returns true if the credentials are equal, otherwise false.</returns>
        public static bool AreEqualOrNull(this CloudStorageCredentials credentials1, CloudStorageCredentials credentials2)
        {
            if ((credentials1 == null) && (credentials2 == null))
                return true;

            return (credentials1 != null) && (credentials2 != null)
                && string.Equals(credentials1.CloudStorageId, credentials2.CloudStorageId, StringComparison.InvariantCultureIgnoreCase)
                && credentials1.Token.AreEqualOrNull(credentials2.Token)
                && credentials1.Url == credentials2.Url
                && credentials1.Username == credentials2.Username
                && credentials1.Secure == credentials2.Secure
                && credentials1.Password.AreEqual(credentials2.Password);
        }

        /// <summary>
        /// Validates the credentials according to the given <paramref name="requirements"/>.
        /// The <see cref="CloudStorageCredentials.CloudStorageId"/> is not part of the validation,
        /// because it is not used for authorization itself.
        /// </summary>
        /// <param name="credentials">The credentials to validate.</param>
        /// <param name="requirements">The requirements the credentials must comply with.</param>
        /// <param name="allowAnonymous">Specifies whether username and password can be empty.</param>
        /// <exception cref="InvalidParameterException">Is thrown when the credentials are invalid.</exception>
        public static void ThrowIfInvalid(this CloudStorageCredentials credentials, CloudStorageCredentialsRequirements requirements, bool allowAnonymous = false)
        {
            if (credentials == null)
                throw new InvalidParameterException(nameof(CloudStorageCredentials));

            if (requirements.HasRequirement(CloudStorageCredentialsRequirements.Token) && (credentials.Token == null))
                throw new InvalidParameterException(string.Format("{0}.{1}", nameof(CloudStorageCredentials), nameof(CloudStorageCredentials.Token)));

            if (requirements.HasRequirement(CloudStorageCredentialsRequirements.Url) && string.IsNullOrWhiteSpace(credentials.Url))
                throw new InvalidParameterException(string.Format("{0}.{1}", nameof(CloudStorageCredentials), nameof(CloudStorageCredentials.Url)));

            bool usernameRequiredOrProvided = !allowAnonymous || !string.IsNullOrEmpty(credentials.Username);
            if (usernameRequiredOrProvided)
            {
                if (requirements.HasRequirement(CloudStorageCredentialsRequirements.Username) && string.IsNullOrWhiteSpace(credentials.Username))
                    throw new InvalidParameterException(string.Format("{0}.{1}", nameof(CloudStorageCredentials), nameof(CloudStorageCredentials.Username)));

                if (requirements.HasRequirement(CloudStorageCredentialsRequirements.Password) && (credentials.Password == null))
                    throw new InvalidParameterException(string.Format("{0}.{1}", nameof(CloudStorageCredentials), nameof(CloudStorageCredentials.Password)));
            }
        }
    }
}
