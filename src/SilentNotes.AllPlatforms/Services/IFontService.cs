// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace SilentNotes.Services
{
    /// <summary>
    /// The font service can generate a list of available font families.
    /// </summary>
    public interface IFontService
    {
        /// <summary>
        /// Gets a sorted list of font families available on the running OS.
        /// </summary>
        /// <returns>Enumeration of font family names.</returns>
        List<string> ListFontFamilies();
    }
}
