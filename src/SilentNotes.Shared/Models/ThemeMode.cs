// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Models
{
    /// <summary>
    /// Enumeration of the settings for dark theme mode.
    /// </summary>
    public enum ThemeMode
    {
        /// <summary>Choose mode automatically depending of the OS settings.</summary>
        Auto,

        /// <summary>Always use dark mode.</summary>
        Dark,

        /// <summary>Always use light mode.</summary>
        Light,
    }
}
