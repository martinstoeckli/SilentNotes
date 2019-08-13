// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace VanillaCloudStorageClient
{
    /// <summary>
    /// Use this class instead of <see cref="CloudStorageCredentials"/>, if the credentials have
    /// to be stored on a local device and therefore must be serialized.
    /// Call <see cref="EncryptBeforeSerialization(Func{string, string})"/> before doing the
    /// serialization, and <see cref="DecryptAfterDeserialization(Func{string, string})"/> after
    /// deserialization.
    /// </summary>
    [Serializable]
    [XmlType("cloud_storage_credentials")]
    [DataContract(Name = "cloud_storage_credentials")]
    [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1502:ElementMustNotBeOnSingleLine", Justification = "The *Specified properties are only markers for serialization.")]
    [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1516:ElementsMustBeSeparatedByBlankLine", Justification = "The *Specified properties are only markers for serialization.")]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "The *Specified properties are only markers for serialization.")]
    public class SerializeableCloudStorageCredentials : CloudStorageCredentials
    {
        /// <summary>
        /// Call this method before doing the serialization, it encrypts all sensitive properties,
        /// so they do not show up in the serialized text.
        /// </summary>
        /// <param name="encrypt">A delegate used to do the encryption of the plain texts.
        /// The resulting string should be Base64 encoded (no binary content), so it can be used
        /// safely for serialization.</param>
        public void EncryptBeforeSerialization(Func<string, string> encrypt)
        {
            SerializeableCloudStorageId = CloudStorageId;
            SerializeableAccessToken = EncryptProperty(Token?.AccessToken, encrypt);
            SerializeableExpiryDate = Token?.ExpiryDate;
            SerializeableRefreshToken = EncryptProperty(Token?.RefreshToken, encrypt);
            SerializeableUsername = EncryptProperty(Username, encrypt);
            SerializeablePassword = EncryptProperty(UnprotectedPassword, encrypt);
            SerializeableUrl = Url;
            SerializeableSecure = Secure;
        }

        /// <summary>
        /// Call this method after deserialization, it decrypts all sensitive properties, so they
        /// can be used by the application.
        /// </summary>
        /// <param name="decrypt">A delegate used to do the decryption of the encrypted textes.</param>
        public void DecryptAfterDeserialization(Func<string, string> decrypt)
        {
            CloudStorageToken token = new CloudStorageToken
            {
                AccessToken = DecryptProperty(SerializeableAccessToken, decrypt),
                ExpiryDate = SerializeableExpiryDate,
                RefreshToken = DecryptProperty(SerializeableRefreshToken, decrypt),
            };
            if ((token.AccessToken == null) && (token.RefreshToken == null))
                token = null;

            CloudStorageId = SerializeableCloudStorageId;
            Token = token;
            Username = DecryptProperty(SerializeableUsername, decrypt);
            UnprotectedPassword = DecryptProperty(SerializeablePassword, decrypt);
            Url = SerializeableUrl;
            Secure = SerializeableSecure;
        }

        /// <summary>
        /// Gets or sets the serializable <see cref="CloudStorageCredentials.CloudStorageId"/>.
        /// </summary>
        [XmlElement("cloud_storage_id")]
        [JsonProperty("cloud_storage_id")]
        [DataMember(Name = "cloud_storage_id")]
        public string SerializeableCloudStorageId { get; set; }

        /// <summary>
        /// Gets or sets the serializable <see cref="CloudStorageToken.AccessToken"/>.
        /// </summary>
        [XmlElement("access_token")]
        [JsonProperty("access_token")]
        [DataMember(EmitDefaultValue = false, Name = "access_token")]
        public string SerializeableAccessToken { get; set; }
        [JsonIgnore]
        public bool SerializeableAccessTokenSpecified { get { return SerializeableAccessToken != null; } } // Do only serialize when set

        /// <summary>
        /// Gets or sets the serializable <see cref="CloudStorageToken.ExpiryDate"/>.
        /// </summary>
        [XmlElement("token_expiry_date")]
        [JsonProperty("token_expiry_date")]
        [DataMember(EmitDefaultValue = false, Name = "token_expiry_date")]
        public DateTime? SerializeableExpiryDate { get; set; }
        [JsonIgnore]
        public bool SerializeableExpiryDateSpecified { get { return SerializeableExpiryDate != null; } } // Do only serialize when set

        /// <summary>
        /// Gets or sets the serializable <see cref="CloudStorageToken.RefreshToken"/>.
        /// </summary>
        [XmlElement("refresh_token")]
        [JsonProperty("refresh_token")]
        [DataMember(EmitDefaultValue = false, Name = "refresh_token")]
        public string SerializeableRefreshToken { get; set; }
        [JsonIgnore]
        public bool SerializeableRefreshTokenSpecified { get { return SerializeableRefreshToken != null; } } // Do only serialize when set

        /// <summary>
        /// Gets or sets the serializable <see cref="CloudStorageCredentials.Username"/>.
        /// </summary>
        [XmlElement("username")]
        [JsonProperty("username")]
        [DataMember(EmitDefaultValue = false, Name = "username")]
        public string SerializeableUsername { get; set; }
        [JsonIgnore]
        public bool SerializeableUsernameSpecified { get { return SerializeableUsername != null; } } // Do only serialize when set

        /// <summary>
        /// Gets or sets the serializable <see cref="CloudStorageCredentials.Password"/>.
        /// </summary>
        [XmlElement("password")]
        [JsonProperty("password")]
        [DataMember(EmitDefaultValue = false, Name = "password")]
        public string SerializeablePassword { get; set; }
        [JsonIgnore]
        public bool SerializeablePasswordSpecified { get { return SerializeablePassword != null; } } // Do only serialize when set

        /// <summary>
        /// Gets or sets the url for login if necessary, otherwise this is null.
        /// </summary>
        [XmlElement("url")]
        [JsonProperty("url")]
        [DataMember(EmitDefaultValue = false, Name = "url")]
        public string SerializeableUrl { get; set; }
        [JsonIgnore]
        public bool SerializeableUrlSpecified { get { return SerializeableUrl != null; } } // Do only serialize when set

        /// <summary>
        /// Gets or sets a value indicating whether an encrypted SSL connection should be used.
        /// This value is currently used onyl by the FTP provider, others will ignore this value.
        /// </summary>
        [XmlElement("secure")]
        [JsonProperty("secure")]
        [DataMember(EmitDefaultValue = false, Name = "secure")]
        public bool SerializeableSecure { get; set; }
        [JsonIgnore]
        public bool SerializeableSecureSpecified { get { return SerializeableSecure == true; } } // Do only serialize when set

        private string EncryptProperty(string plainText, Func<string, string> encrypt)
        {
            return (plainText == null) ? null : encrypt(plainText);
        }

        private string DecryptProperty(string cipherText, Func<string, string> decrypt)
        {
            return (cipherText == null) ? null : decrypt(cipherText);
        }
    }
}
