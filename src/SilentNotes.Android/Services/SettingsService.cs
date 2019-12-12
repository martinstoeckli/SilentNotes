// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Linq;
using Android.Content;
using AndroidX.Core.Content;
using SilentNotes.Crypto;
using SilentNotes.Services;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// Implements the <see cref="ISettingsService"/> interface for the Android platform.
    /// </summary>
    public class SettingsService : SettingsServiceBase
    {
        private const string snpsk = "53EC49B1-6600+406b;B84F-0B9CFA1D2BE1"; // backwards compatibility
        private readonly Context _appContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsService"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public SettingsService(Context appContext, IXmlFileService xmlFileService, IDataProtectionService dataProtectionService, IEnvironmentService environmentService)
            : base(xmlFileService, dataProtectionService, environmentService)
        {
            _appContext = appContext;
        }

        /// <summary>
        /// Gets the path to the directory where the settings are stored. We want to keep the
        /// settings private, so we choose the directory which will not be synchronized with the
        /// cloud.
        /// </summary>
        /// <returns>The full directory path for storing the config.</returns>
        protected override string GetDirectoryPath()
        {
            return ContextCompat.GetNoBackupFilesDir(_appContext).AbsolutePath;
        }

        /// <inheritdoc/>
        protected override void UpdateSettingsFrom1To2(XElement root)
        {
            base.UpdateSettingsFrom1To2(root);

            // Handle protected password
            XElement oldPasswortElement = root.Element("cloud_storage")?.Element("cloud_password");
            XElement cloudStorageAccount = root.Element("cloud_storage_account");
            if ((oldPasswortElement != null) && (cloudStorageAccount != null))
            {
                // Deobfuscate old password
                ICryptor decryptor = new Cryptor("snps", null);
                byte[] binaryCipher = CryptoUtils.Base64StringToBytes(oldPasswortElement.Value);
                byte[] unprotectedBinaryPassword = decryptor.Decrypt(binaryCipher, CryptoUtils.StringToSecureString(snpsk));

                // Protect with new data protection service and add to XML
                char[] unprotectedChars = Encoding.UTF8.GetChars(unprotectedBinaryPassword);
                byte[] unprotectedUnicodePassword = Encoding.Unicode.GetBytes(unprotectedChars);
                string protectedPassword = _dataProtectionService.Protect(unprotectedUnicodePassword);
                cloudStorageAccount.Add(new XElement("protected_password", protectedPassword));
                CryptoUtils.CleanArray(unprotectedBinaryPassword);
                CryptoUtils.CleanArray(unprotectedChars);
                CryptoUtils.CleanArray(unprotectedUnicodePassword);
            }
        }
    }
}
