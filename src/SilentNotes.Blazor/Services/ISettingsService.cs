// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.Models;

namespace SilentNotes.Services
{
    /// <summary>
    /// Interface of a service which can load/safe the settings from the locale device
    /// </summary>
    public interface ISettingsService
    {
        /// <summary>
        /// Loads the settings from the locale device. If no settings exists, new empty
        /// settings are returned.
        /// </summary>
        /// <returns>The loaded or new created and updated settings.</returns>
        SettingsModel LoadSettingsOrDefault();

        /// <summary>
        /// Tries to save the settings on the locale device.
        /// </summary>
        /// <param name="model">The settings to save.</param>
        /// <returns>Returns true if the settings could be saved, otherwise false.</returns>
        bool TrySaveSettingsToLocalDevice(SettingsModel model);
    }
}
