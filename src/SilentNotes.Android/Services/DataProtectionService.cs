// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Android.Content;
using Android.OS;
using Android.Security;
using Android.Security.Keystore;
using Java.Math;
using Java.Security;
using Java.Util;
using Javax.Crypto;
using Javax.Security.Auth.X500;
using SilentNotes.Crypto;
using SilentNotes.Crypto.SymmetricEncryption;
using SilentNotes.Services;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// Implementation of the <see cref="IDataProtectionService"/> interface for the Android platform.
    /// Credits to:
    ///   https://msicc.net/xamarin-android-asymmetric-encryption-without-any-user-input-or-hardcoded-values/
    ///   https://proandroiddev.com/secure-data-in-android-encryption-in-android-part-2-991a89e55a23
    /// </summary>
    public class DataProtectionService : IDataProtectionService
    {
        private const string KeyStoreName = "AndroidKeyStore";
        private const string KeyAlias = "ch.martinstoeckli.silentnotes";
        private const string Obcake = "StyleCop.CSharp.DocumentationRules|SA1611:ElementParametersMustBeDocumented|{5de6e1f4-c4f7-4bd6-bfb7-129a272b2cd3}";
        private const int KeySize = 2048;
        private const char Separator = '$';

        private readonly Context _applicationContext;
        private readonly ICryptoRandomService _randomService;
        private KeyStore _androidKeyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataProtectionService"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public DataProtectionService(Context applicationContext, ICryptoRandomService randomService)
        {
            _applicationContext = applicationContext;
            _randomService = randomService;
        }

        /// <inheritdoc/>
        public string Protect(byte[] unprotectedData)
        {
            // Encrypt the data with a new random key
            BouncyCastleTwofishGcm encryptor = new BouncyCastleTwofishGcm();
            byte[] randomKey = _randomService.GetRandomBytes(encryptor.ExpectedKeySize);
            byte[] nonce = _randomService.GetRandomBytes(encryptor.ExpectedNonceSize);
            byte[] protectedData = encryptor.Encrypt(unprotectedData, randomKey, nonce);

            // Protect the random key with the OS support
            byte[] encryptedRandomKey;
            try
            {
                if (!KeysExistInKeyStore())
                    CreateKeyPairInKeyStore();
                Cipher cipher = Cipher.GetInstance("RSA/ECB/PKCS1Padding"); // ECB mode is not used by RSA
                IKey publicKey = GetPublicKeyFromKeyStore();
                cipher.Init(CipherMode.EncryptMode, publicKey);
                encryptedRandomKey = cipher.DoFinal(randomKey);
            }
            catch (Exception)
            {
                // Seems there are exotic devices, which do not support the keystore properly.
                // The least we can do is obfuscating the key.
                encryptedRandomKey = CryptoUtils.Obfuscate(randomKey, Obcake, _randomService);
            }

            // Combine the encrypted random key and the encrypted data
            StringBuilder result = new StringBuilder();
            result.Append(CryptoUtils.BytesToBase64String(encryptedRandomKey));
            result.Append(Separator);
            result.Append(CryptoUtils.BytesToBase64String(nonce));
            result.Append(Separator);
            result.Append(CryptoUtils.BytesToBase64String(protectedData));
            return result.ToString();
        }

        /// <inheritdoc/>
        public byte[] Unprotect(string protectedData)
        {
            // Extract encrypted key and encrypted data
            string[] parts = protectedData.Split(Separator);
            if (parts.Length != 3)
                throw new Exception("Invalid format of protected data");
            byte[] encryptedStoredKey = CryptoUtils.Base64StringToBytes(parts[0]);
            byte[] nonce = CryptoUtils.Base64StringToBytes(parts[1]);
            byte[] protectedDataPart = CryptoUtils.Base64StringToBytes(parts[2]);

            // Unprotect the random key with OS support
            byte[] storedKey = null;
            try
            {
                if (KeysExistInKeyStore())
                {
                    Cipher cipher = Cipher.GetInstance("RSA/ECB/PKCS1Padding"); // ECB mode is not used by RSA
                    IKey privateKey = GetPrivateKeyFromKeyStore();
                    cipher.Init(CipherMode.DecryptMode, privateKey);
                    storedKey = cipher.DoFinal(encryptedStoredKey);
                }
            }
            catch (Exception)
            {
                storedKey = null;
            }

            // Fallback to obfuscated key
            if (storedKey == null)
                storedKey = CryptoUtils.Deobfuscate(encryptedStoredKey, Obcake);

            // Decrypt the data with the random key
            BouncyCastleTwofishGcm decryptor = new BouncyCastleTwofishGcm();
            byte[] result = decryptor.Decrypt(protectedDataPart, storedKey, nonce);
            return result;
        }

        /// <summary>
        /// Gets or creates the Android key store.
        /// </summary>
        private KeyStore AndroidKeyStore
        {
            get
            {
                // Lazy creation of the KeyStore
                if (_androidKeyStore == null)
                {
                    _androidKeyStore = KeyStore.GetInstance(KeyStoreName);
                    _androidKeyStore.Load(null);
                }
                return _androidKeyStore;
            }
        }

        /// <summary>
        /// Creates a new public-private key pair. An already existing key will be deleted, so
        /// make sure to call <see cref="KeysExistInKeyStore"/> before.
        /// </summary>
        private void CreateKeyPairInKeyStore()
        {
            RemoveKeyFromKeyStore();
            KeyPairGenerator keyGenerator =
                KeyPairGenerator.GetInstance(KeyProperties.KeyAlgorithmRsa, KeyStoreName);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr2 &&
                Build.VERSION.SdkInt <= BuildVersionCodes.LollipopMr1)
            {
                Calendar startDateCalendar = Calendar.GetInstance(Locale.Default);
                startDateCalendar.Add(CalendarField.Year, -1);
                Calendar endDateCalendar = Calendar.GetInstance(Locale.Default);
                endDateCalendar.Add(CalendarField.Year, 100);
                string certificateName = string.Format("CN={0} CA Certificate", KeyAlias);

                // this API is obsolete after Android M, but we are supporting Android L
#pragma warning disable 618
                var builder = new KeyPairGeneratorSpec.Builder(_applicationContext)
                    .SetAlias(KeyAlias)
                    .SetSerialNumber(BigInteger.One)
                    .SetSubject(new X500Principal(certificateName))
                    .SetStartDate(startDateCalendar.Time)
                    .SetEndDate(endDateCalendar.Time)
                    .SetKeySize(KeySize);
#pragma warning restore 618

                keyGenerator.Initialize(builder.Build());
            }
            else if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                Calendar endDateCalendar = Calendar.GetInstance(Locale.Default);
                endDateCalendar.Add(CalendarField.Year, 100);

                var builder = new KeyGenParameterSpec.Builder(KeyAlias, KeyStorePurpose.Encrypt | KeyStorePurpose.Decrypt)
                    .SetBlockModes(KeyProperties.BlockModeEcb)
                    .SetEncryptionPaddings(KeyProperties.EncryptionPaddingRsaPkcs1)
                    .SetCertificateNotAfter(endDateCalendar.Time)
                    .SetKeySize(KeySize);
                keyGenerator.Initialize(builder.Build());
            }

            // Key generator is initialized, generate the key
            keyGenerator.GenerateKeyPair();
        }

        /// <summary>
        /// Checks whether the public-private key pair already exists.
        /// </summary>
        /// <returns>Returns true if the key exists, otherwise false.</returns>
        private bool KeysExistInKeyStore()
        {
            return AndroidKeyStore.ContainsAlias(KeyAlias);
        }

        private IKey GetPublicKeyFromKeyStore()
        {
            if (KeysExistInKeyStore())
                return AndroidKeyStore.GetCertificate(KeyAlias)?.PublicKey;
            return null;
        }

        private IKey GetPrivateKeyFromKeyStore()
        {
            if (KeysExistInKeyStore())
                return AndroidKeyStore.GetKey(KeyAlias, null);
            return null;
        }

        private bool RemoveKeyFromKeyStore()
        {
            bool containsAlias = KeysExistInKeyStore();
            if (containsAlias)
                AndroidKeyStore.DeleteEntry(KeyAlias);
            return containsAlias;
        }
    }
}