﻿// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Xml.Serialization;
using SilentNotes.Crypto.SymmetricEncryption;
using SilentNotes.Services.CloudStorageServices;

namespace SilentNotes.Models
{
    /// <summary>
    /// Serializeable model of the applications settings.
    /// </summary>
    [XmlRoot(ElementName = "silentnotes_settings")]
    public class SettingsModel
    {
        /// <summary>The highest revision of the settings which can be handled by this application.</summary>
        public const int NewestSupportedRevision = 2;
        private string _selectedEncryptionAlgorithm;
        private string _transferCode;
        private List<string> _transferCodeHistory;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsModel"/> class.
        /// </summary>
        public SettingsModel()
        {
            Revision = NewestSupportedRevision;
            AdoptCloudEncryptionAlgorithm = true;
            AutoSyncMode = AutoSynchronizationMode.CostFreeInternetOnly;
            ShowCursorArrowKeys = true;
        }

        /// <summary>
        /// Gets or sets the revision, which was used to create the settings.
        /// </summary>
        [XmlAttribute(AttributeName = "revision")]
        public int Revision { get; set; }

        /// <summary>
        /// Gets or sets the settings for the cloud storage account if available, otherwise this
        /// property is null.
        /// </summary>
        [XmlElement("cloud_storage_account")]
        public CloudStorageAccount CloudStorageAccount { get; set; }

        /// <summary>
        /// Gets or sets the default encryption algorithm, used to encrypt the repository
        /// before sending it to the cloud.
        /// </summary>
        [XmlElement("selected_encryption_algorithm")]
        public string SelectedEncryptionAlgorithm
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_selectedEncryptionAlgorithm))
                    _selectedEncryptionAlgorithm = GetDefaultEncryptionAlgorithmName();
                return _selectedEncryptionAlgorithm;
            }

            set { _selectedEncryptionAlgorithm = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the encryption algorithm from the cloud storage
        /// should replace the selected encryption algorithm.
        /// </summary>
        [XmlElement("adopt_cloud_encryption_algorithm")]
        public bool AdoptCloudEncryptionAlgorithm { get; set; }

        /// <summary>
        /// Gets or sets a value describing whether auto synchronization with the online storage
        /// shoudl be done or not.
        /// </summary>
        [XmlElement("auto_sync_mode")]
        public AutoSynchronizationMode AutoSyncMode { get; set; }

        /// <summary>
        /// Gets or sets the transfer code corresponding with the current repository.
        /// Manages the transfercode history, when adding a new transfer code.
        /// </summary>
        [XmlElement("transfer_code")]
        public string TransferCode
        {
            get { return _transferCode; }

            set
            {
                if (string.IsNullOrWhiteSpace(value) || string.Equals(_transferCode, value))
                    return;

                // Remove the new transfer code from the archive, if existing
                TransferCodeHistory.Remove(value);

                // Archive the current transfer code if existing
                if (!string.IsNullOrWhiteSpace(_transferCode))
                {
                    TransferCodeHistory.Remove(_transferCode);
                    TransferCodeHistory.Insert(0, _transferCode); // bring to top
                }

                // Replace current transfer code
                _transferCode = value;
            }
        }

        /// <summary>
        /// Gets or sets a history of old transfercodes.
        /// </summary>
        [XmlArray("transfer_code_history")]
        [XmlArrayItem("transfer_code")]
        public List<string> TransferCodeHistory
        {
            get { return LazyCreator.GetOrCreate(ref _transferCodeHistory); }
            set { _transferCodeHistory = value; }
        }

        /// <summary>
        /// Gets a value indicating whether the virtual arrow keys should be displayed.
        /// </summary>
        [XmlElement("show_cursor_keys")]
        public bool ShowCursorArrowKeys { get; set; }

        /// <summary>
        /// Gets the name of the algorithm to use, if the selected algorithm is not yet stored.
        /// </summary>
        /// <returns>The name of the default encryption algorithm.</returns>
        public static string GetDefaultEncryptionAlgorithmName()
        {
            return BouncyCastleAesGcm.CryptoAlgorithmName;
        }
    }
}
