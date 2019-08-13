// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;
using System.Text;
using System.Xml.Linq;
using SilentNotes.Models;
using SilentNotes.Workers;
using VanillaCloudStorageClient;

namespace SilentNotes.Services
{
    /// <summary>
    /// Overrideable base implementation of the <see cref="ISettingsService"/> interface.
    /// </summary>
    public abstract class SettingsServiceBase : ISettingsService
    {
        /// <summary>Gets the injected dependency to an xml file service.</summary>
        protected readonly IXmlFileService _xmlFileService;

        /// <summary>Gets the injected dependency to a data protection service</summary>
        protected readonly IDataProtectionService _dataProtectionService;

        private SettingsModel _cachedSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsServiceBase"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public SettingsServiceBase(IXmlFileService xmlFileService, IDataProtectionService dataProtectionService)
        {
            _xmlFileService = xmlFileService;
            _dataProtectionService = dataProtectionService;
        }

        /// <inheritdoc/>
        public SettingsModel LoadSettingsOrDefault()
        {
            if (_cachedSettings != null)
                return _cachedSettings; // Take it form cache if available

            SettingsModel result = null;
            bool modelWasUpdated = false;
            try
            {
                string xmlFilePath = Path.Combine(GetDirectoryPath(), Config.UserSettingsFileName);
                if (_xmlFileService.TryLoad(xmlFilePath, out XDocument xml))
                {
                    modelWasUpdated = UpdateSettings(xml);
                    result = XmlUtils.DeserializeFromXmlDocument<SettingsModel>(xml);
                    AfterLoading(result);
                }
            }
            catch (Exception)
            {
                result = null;
            }

            // Create default settings if result is still null
            if (result == null)
            {
                result = new SettingsModel();
                modelWasUpdated = true;
            }

            // Automatically save settings if they where modified by an update
            if (modelWasUpdated)
                TrySaveSettingsToLocalDevice(result);
            _cachedSettings = result;
            return _cachedSettings;
        }

        /// <inheritdoc/>
        public bool TrySaveSettingsToLocalDevice(SettingsModel model)
        {
            try
            {
                BeforeSaving(model);
                string xmlFilePath = Path.Combine(GetDirectoryPath(), Config.UserSettingsFileName);
                bool success = _xmlFileService.TrySerializeAndSave(xmlFilePath, model);
                if (success)
                    _cachedSettings = model;
                return success;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Sub classes should override this method to change the directory, where the config is stored.
        /// </summary>
        /// <returns>The full directory path for storing the config.</returns>
        protected abstract string GetDirectoryPath();

        /// <summary>
        /// Updates previous versions of the config to the current one.
        /// </summary>
        /// <param name="xml">XML document to update.</param>
        /// <returns>Returns true if an update was done, otherwise false.</returns>
        protected bool UpdateSettings(XDocument xml)
        {
            XElement root = xml.Root;

            // A missing revision attribute means revision 1
            XAttribute revisionAttribute = root.Attribute("revision");
            if (revisionAttribute == null)
            {
                revisionAttribute = new XAttribute("revision", 1);
                root.Add(revisionAttribute);
            }
            int oldRevision = int.Parse(revisionAttribute.Value);

            // Check for necessary update steps
            if (oldRevision <= 1)
                UpdateSettingsFrom1To2(root);
            if (oldRevision <= 2)
                UpdateSettingsFrom2To3(root);

            bool updated = oldRevision < SettingsModel.NewestSupportedRevision;
            if (updated)
                root.SetAttributeValue("revision", SettingsModel.NewestSupportedRevision);
            return updated;
        }

        /// <summary>
        /// Converts the XML document which contains a version 1 config, to a version 2 config.
        /// </summary>
        /// <param name="root">Root node of the XML document.</param>
        protected virtual void UpdateSettingsFrom1To2(XElement root)
        {
            XElement cloudStorageSettings = root.Element("cloud_storage");
            if (cloudStorageSettings != null)
            {
                XElement cloudStorageAccountElement = new XElement("cloud_storage_account");

                XElement cloudTypeElement = cloudStorageSettings.Element("cloud_type");
                cloudStorageAccountElement.Add(new XElement("cloud_type", cloudTypeElement.Value));

                XElement urlElement = cloudStorageSettings.Element("cloud_url");
                if (urlElement != null)
                    cloudStorageAccountElement.Add(new XElement("url", urlElement.Value));

                XElement userElement = cloudStorageSettings.Element("cloud_username");
                if (userElement != null)
                    cloudStorageAccountElement.Add(new XElement("username", userElement.Value));

                // The attribute "cloud_password" is protected and can be converted only by a
                // platform specific implementation by overriding this method.
                root.AddFirst(cloudStorageAccountElement);
            }
        }

        /// <summary>
        /// Converts the XML document which contains a version 2 config, to a version 3 config.
        /// </summary>
        /// <param name="root">Root node of the XML document.</param>
        protected virtual void UpdateSettingsFrom2To3(XElement root)
        {
            XElement cloudStorageAcount = root.Element("cloud_storage_account");
            if (cloudStorageAcount != null)
            {
                XElement cloudStorageCredentialsElement = new XElement("cloud_storage_credentials");

                XElement cloudTypeElement = cloudStorageAcount.Element("cloud_type");
                if (cloudTypeElement != null)
                    cloudStorageCredentialsElement.Add(new XElement("cloud_storage_id", cloudTypeElement.Value.ToLowerInvariant()));

                XElement userElement = cloudStorageAcount.Element("username");
                if (userElement != null)
                    cloudStorageCredentialsElement.Add(new XElement("username", EncryptProperty(userElement.Value)));

                XElement passwordElement = cloudStorageAcount.Element("protected_password");
                if (passwordElement != null)
                {
                    byte[] passwordBytes = _dataProtectionService.Unprotect(passwordElement.Value);
                    SecureString password = SecureStringExtensions.BytesToSecureString(passwordBytes, Encoding.Unicode);
                    cloudStorageCredentialsElement.Add(new XElement("password", EncryptProperty(password.SecureStringToString())));
                }

                XElement urlElement = cloudStorageAcount.Element("url");
                if (urlElement != null)
                    cloudStorageCredentialsElement.Add(new XElement("url", urlElement.Value));

                XElement accessTokenElement = cloudStorageAcount.Element("oauth_access_token");
                if (accessTokenElement != null)
                    cloudStorageCredentialsElement.Add(new XElement("access_token", EncryptProperty(accessTokenElement.Value)));

                root.AddFirst(cloudStorageCredentialsElement);
            }
        }

        /// <summary>
        /// Sub classes can optionally override this method to edit the model before it is saved to
        /// the file system.
        /// </summary>
        /// <param name="settings">The settings model which will be stored.</param>
        protected virtual void BeforeSaving(SettingsModel settings)
        {
            settings.Credentials?.EncryptBeforeSerialization(EncryptProperty);
        }

        /// <summary>
        /// Sub classes can optionally override this method to edit the model after it has been
        /// loaded from the file system.
        /// </summary>
        /// <param name="settings">The settings model which was loaded.</param>
        protected virtual void AfterLoading(SettingsModel settings)
        {
            settings.Credentials?.DecryptAfterDeserialization(DecryptProperty);
        }

        private string EncryptProperty(string plainText)
        {
            return (plainText == null) ? null : _dataProtectionService.Protect(Encoding.UTF8.GetBytes(plainText));
        }

        private string DecryptProperty(string cipherText)
        {
            return (cipherText == null) ? null : Encoding.UTF8.GetString(_dataProtectionService.Unprotect(cipherText));
        }
    }
}
