// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Security;
using System.Xml.Serialization;
using SilentNotes.Crypto;

namespace SilentNotes.Models
{
    /// <summary>
    /// A safe can be used to encrypt one or more notes.
    /// </summary>
    public class SafeModel : IDisposable
    {
        /// <summary>The package name used for encryption, see <see cref="CryptoHeader.PackageName"/></summary>
        public const string CryptorPackageName = "SilentSafe";
        private Guid _id;

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeModel"/> class.
        /// </summary>
        public SafeModel()
        {
            CreatedAt = DateTime.UtcNow;
            ModifiedAt = CreatedAt;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="SafeModel"/> class.
        /// </summary>
        ~SafeModel()
        {
            Dispose();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Close();
        }

        /// <summary>
        /// Gets or sets the id of the safe.
        /// </summary>
        [XmlAttribute(AttributeName = "id")]
        public Guid Id
        {
            get { return (_id != Guid.Empty) ? _id : (_id = Guid.NewGuid()); }
            set { _id = value; }
        }

        /// <summary>
        /// Gets or sets the time in UTC, when the note was first created.
        /// </summary>
        [XmlAttribute(AttributeName = "created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the time in UTC, when the password was last updated.
        /// </summary>
        [XmlAttribute(AttributeName = "modified_at")]
        public DateTime ModifiedAt { get; set; }

        /// <summary>
        /// Gets or sets the time in UTC, when the object was last updated by the system, instead of
        /// the user. This way the system can clean up deprecated functions, but does not interfere
        /// with more important user changes.
        /// </summary>
        [XmlIgnore]
        public DateTime? MaintainedAt { get; set; }

        [XmlAttribute(AttributeName = "maintained_at")]
        public DateTime MaintainedAtSerializeable
        { 
            get { return MaintainedAt.Value; }
            set { MaintainedAt = value; }
        }
        public bool MaintainedAtSerializeableSpecified { get { return MaintainedAt != null && MaintainedAt > ModifiedAt; } } // Serialize only when set

        /// <summary>
        /// Clears <see cref="MaintainedAt"/> if it was modified later.
        /// </summary>
        public void ClearMaintainedAtIfObsolete()
        {
            if ((MaintainedAt != null) && (MaintainedAt < ModifiedAt))
                MaintainedAt = null;
        }

        /// <summary>
        /// Gets or sets the serializable <see cref="Key"/>.
        /// </summary>
        [XmlElement("key")]
        public string SerializeableKey { get; set; }

        /// <summary>
        /// Gets the key of the safe, which can be used to encrypt/decrypt data.
        /// This key is only available if the safe is open, otherwise it is null.
        /// </summary>
        [XmlIgnore]
        public byte[] Key { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether the safe is open and its <see cref="Key"/> can be used
        /// to encrypt/decrypt data.
        /// </summary>
        [XmlIgnore]
        public bool IsOpen
        {
            get { return Key != null; }
        }

        /// <summary>
        /// Sets the <see cref="ModifiedAt"/> property to the current UTC time.
        /// </summary>
        public void RefreshModifiedAt()
        {
            ModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Tries to open the safe. The <see cref="SerializeableKey"/> will be decrypted and the
        /// <see cref="Key"/> will be set.
        /// </summary>
        /// <param name="password">User password to open the safe.</param>
        /// <returns>Returns true if the safe could be opened or was already open, otherwise false.</returns>
        public bool TryOpen(SecureString password)
        {
            if (!IsOpen)
            {
                if (TryDecryptKey(SerializeableKey, password, out byte[] decryptedKey))
                    Key = decryptedKey;
                else
                    Close();
            }
            return IsOpen;
        }

        /// <summary>
        /// Tries to decrypt the key with a password, regardless whether the safe is already open.
        /// </summary>
        /// <param name="serializeableKey">Serializeable key to decrypt.</param>
        /// <param name="password">User password to decrypt the key.</param>
        /// <param name="key">Retrieves the decrypted key if the decryption was successful.</param>
        /// <returns>Returns true if the decryption was successful, otherwise false.</returns>
        public static bool TryDecryptKey(string serializeableKey, SecureString password, out byte[] key)
        {
            try
            {
                byte[] encryptedKey = CryptoUtils.Base64StringToBytes(serializeableKey);
                ICryptor encryptor = new Cryptor(CryptorPackageName, null);
                key = encryptor.Decrypt(encryptedKey, password);
                return true;
            }
            catch (Exception)
            {
                key = null;
                return false;
            }
        }

        /// <summary>
        /// Closes the safe if it was open. The <see cref="Key"/> is removed from memory.
        /// </summary>
        public void Close()
        {
            CryptoUtils.CleanArray(Key);
            Key = null;
        }

        /// <summary>
        /// Generates a new <see cref="Key"/> for the safe.
        /// </summary>
        /// <param name="password">The user password to encrypt the key for serialization.</param>
        /// <param name="randomSource">A cryptographically random source.</param>
        /// <param name="encryptionAlgorithm">The encryption algorithm to encrypt the key.</param>
        public void GenerateNewKey(SecureString password, ICryptoRandomSource randomSource, string encryptionAlgorithm)
        {
            Close();

            // We generate a 256 bit key, this is required by all available symmetric encryption
            // algorithms and is more than big enough even for future algorithms.
            Key = randomSource.GetRandomBytes(32);
            SerializeableKey = EncryptKey(Key, password, randomSource, encryptionAlgorithm);
        }

        /// <summary>
        /// Tries to encrypt the key with a password, so it can be stored as <see cref="SerializeableKey"/>.
        /// </summary>
        /// <param name="key">Key to encrypt.</param>
        /// <param name="password">The user password to encrypt the key with.</param>
        /// <param name="randomSource">A cryptographically random source.</param>
        /// <param name="encryptionAlgorithm">The encryption algorithm to encrypt the key.</param>
        /// <returns>Returns the encrypted serializeable key.</returns>
        public static string EncryptKey(byte[] key, SecureString password, ICryptoRandomSource randomSource, string encryptionAlgorithm)
        {
            ICryptor encryptor = new Cryptor(CryptorPackageName, randomSource);
            byte[] encryptedKey = encryptor.Encrypt(
                key, password, Crypto.KeyDerivation.KeyDerivationCostType.High, encryptionAlgorithm);
            return CryptoUtils.BytesToBase64String(encryptedKey);
        }

        /// <summary>
        /// Makes a deep copy of the safe.
        /// </summary>
        /// <returns>Copy of the note.</returns>
        public SafeModel Clone()
        {
            SafeModel result = new SafeModel();
            CloneTo(result);
            return result;
        }

        /// <summary>
        /// Makes <paramref name="target"/> a deep copy of the safe.
        /// </summary>
        /// <param name="target">Copy all properties to this note.</param>
        public void CloneTo(SafeModel target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            target.Id = this.Id;
            target.SerializeableKey = this.SerializeableKey;
            target.Key = null; // A cloned safe is closed by default, but can be opened
            target.CreatedAt = this.CreatedAt;
            target.ModifiedAt = this.ModifiedAt;
            target.MaintainedAt = this.MaintainedAt;
        }
    }
}
