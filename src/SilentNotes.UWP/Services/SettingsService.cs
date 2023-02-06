// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.Services;
using Windows.Storage;

namespace SilentNotes.UWP.Services
{
    /// <summary>
    /// Implements the <see cref="ISettingsService"/> interface for the UWP platform.
    /// </summary>
    internal class SettingsService : SettingsServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsService"/> class.
        /// </summary>
        public SettingsService(IXmlFileService xmlFileService, IDataProtectionService dataProtectionService)
            : base(xmlFileService, dataProtectionService)
        {
        }

        /// <summary>
        /// Gets the path to the directory where the settings are stored. We want to keep the
        /// settings private, so we go for the local folder instead of a roaming one, which is
        /// sometimes synchronized with the cloud.
        /// </summary>
        /// <returns>The full directory path for storing the config.</returns>
        protected override string GetDirectoryPath()
        {
            return ApplicationData.Current.LocalFolder.Path;
        }
    }
}
