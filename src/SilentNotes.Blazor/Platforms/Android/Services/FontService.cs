// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using SilentNotes.Services;
using SkiaSharp;

namespace SilentNotes.Platforms.Services
{
    /// <summary>
    /// Implementation of the <see cref="IFontService"/> interface for the Android platform.
    /// </summary>
    public class FontService : IFontService
    {
        /// <inheritdoc/>
        public List<string> ListFontFamilies()
        {
            List<string> result = SKFontManager.Default.FontFamilies.ToList();
            result.Sort(StringComparer.InvariantCultureIgnoreCase);
            return result;
        }
    }
}
