// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// Prevents taking screenshots from the application window.
    /// </summary>
    public interface IScreenshots
    {
        /// <summary>
        /// Sets a value indicating whether screenshots should be forbidden or allowed.
        /// </summary>
        /// <param name="value">Set true to forbid screenshots, false to allow screenshots.</param>
        bool PreventScreenshots { set; }
    }
}
