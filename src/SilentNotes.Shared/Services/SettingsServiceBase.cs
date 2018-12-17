// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Linq;
using SilentNotes.Crypto;
using SilentNotes.Models;
using SilentNotes.Services.CloudStorageServices;
using SilentNotes.Workers;

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
            {
                UpdateSettingsFrom1To2(root);
            }

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
        /// Sub classes can optionally override this method to edit the model before it is saved to
        /// the file system.
        /// </summary>
        /// <param name="settings">The settings model which will be stored.</param>
        protected virtual void BeforeSaving(SettingsModel settings)
        {
            // encrypt password
            CloudStorageAccount account = settings.CloudStorageAccount;
            if (account != null)
            {
                if (account.Password == null)
                {
                    account.ProtectedPassword = null;
                }
                else
                {
                    byte[] unprotectedPasswordBytes = SecureStringExtensions.SecureStringToUnicodeBytes(account.Password);
                    account.ProtectedPassword = _dataProtectionService.Protect(unprotectedPasswordBytes);
                    CryptoUtils.CleanArray(unprotectedPasswordBytes);
                }
            }
        }

        /// <summary>
        /// Sub classes can optionally override this method to edit the model after it has been
        /// loaded from the file system.
        /// </summary>
        /// <param name="settings">The settings model which was loaded.</param>
        protected virtual void AfterLoading(SettingsModel settings)
        {
            // decrypt password
            CloudStorageAccount account = settings.CloudStorageAccount;
            if (!string.IsNullOrEmpty(account?.ProtectedPassword))
            {
                byte[] passwordBytes = _dataProtectionService.Unprotect(account.ProtectedPassword);
                account.Password = SecureStringExtensions.UnicodeBytesToSecureString(passwordBytes);
                CryptoUtils.CleanArray(passwordBytes);
            }
        }
    }
}
