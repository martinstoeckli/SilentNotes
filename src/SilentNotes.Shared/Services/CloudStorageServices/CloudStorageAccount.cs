// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security;
using System.Xml.Serialization;
using SilentNotes.Workers;

namespace SilentNotes.Services.CloudStorageServices
{
    /// <summary>
    /// Serializeable model to store the credentials of a cloud storage service.
    /// </summary>
    [Serializable]
    [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1502:ElementMustNotBeOnSingleLine", Justification = "The *Specified properties are only markers for serialization.")]
    [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1516:ElementsMustBeSeparatedByBlankLine", Justification = "The *Specified properties are only markers for serialization.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "The *Specified properties are only markers for serialization.")]
    public class CloudStorageAccount : IDisposable, IEquatable<CloudStorageAccount>
    {
        private SecureString _securePassword;

        /// <summary>
        /// Gets or sets a password in a form which can be disposed and removed from memory.
        /// </summary>
        [XmlIgnore]
        public SecureString Password
        {
            get { return _securePassword; }

            set
            {
                _securePassword?.Dispose(); // remove old content
                _securePassword = value;
            }
        }

        /// <summary>
        /// Gets or sets the username for login if necessary, otherwise this is null.
        /// </summary>
        [XmlElement("username")]
        public string Username { get; set; }
        public bool UsernameSpecified { get { return Username != null; } }

        /// <summary>
        /// Gets or sets the password in a protected form, so it can be savely serialized and
        /// written to a config file. Use a platform specific protection service like a keystore.
        /// </summary>
        [XmlElement("protected_password")]
        public string ProtectedPassword { get; set; }
        public bool ProtectedPasswordSpecified { get { return ProtectedPassword != null; } }

        /// <summary>
        /// Gets or sets the url for login if necessary, otherwise this is null.
        /// </summary>
        [XmlElement("url")]
        public string Url { get; set; }
        public bool UrlSpecified { get { return Url != null; } }

        /// <summary>
        /// Gets or sets the oauth2 access token if necessary, otherwise this is null.
        /// </summary>
        [XmlElement("oauth_access_token")]
        public string OauthAccessToken { get; set; }
        public bool OauthAccessTokenSpecified { get { return OauthAccessToken != null; } }

        /// <summary>
        /// Gets or sets the oauth2 refresh token if necessary, otherwise this is null.
        /// </summary>
        [XmlElement("oauth_refresh_token")]
        public string OauthRefreshToken { get; set; }
        public bool OauthRefreshTokenSpecified { get { return OauthRefreshToken != null; } }

        /// <summary>
        /// Gets or sets the cloud type assotiated with the account.
        /// </summary>
        [XmlElement("cloud_type")]
        public CloudStorageType CloudType { get; set; }

        /// <inheritdoc/>
        public void Dispose()
        {
            Password = null; // remove password from memory
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as CloudStorageAccount);
        }

        /// <inheritdoc/>
        public bool Equals(CloudStorageAccount other)
        {
            return other != null
                && Username == other.Username
                && Url == other.Url
                && OauthAccessToken == other.OauthAccessToken
                && OauthRefreshToken == other.OauthRefreshToken
                && CloudType == other.CloudType
                && SecureStringExtensions.AreEqual(Password, other.Password);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hashCode = 521822988;
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Username);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Url);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(OauthAccessToken);
            hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(OauthRefreshToken);
            hashCode = (hashCode * -1521134295) + CloudType.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the URL is fix, or if it can be edited by the user.
        /// </summary>
        [XmlIgnore]
        public bool IsUrlReadonly { get; set; }
    }
}
