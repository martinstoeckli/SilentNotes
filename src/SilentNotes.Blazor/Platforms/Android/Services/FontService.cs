// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Xml.Linq;
using SilentNotes.Services;

namespace SilentNotes.Platforms.Services
{
    /// <summary>
    /// Implementation of the <see cref="IFontService"/> interface for the Android platform.
    /// </summary>
    public class FontService : IFontService
    {
        /// <summary>
        /// Gets a sorted list of font families available on the running OS.
        /// </summary>
        /// <remarks>
        /// Android, since OperatingSystem.IsAndroidVersionAtLeast(29), allows to list the system
        /// font files via SystemFonts.AvailableFonts, but unfortunately a WebView cannot access
        /// and use them to display text.
        /// Therefore, in lack of an alternative, we do the same a the SkiaSharp library does to
        /// get the "SKFontManager.Default.FontFamilies", we parse Androids fonts.xml file.
        /// </remarks>
        /// <returns>Enumeration of font family names.</returns>
        public List<string> ListFontFamilies()
        {
            try
            {
                var result = new List<string>();
                var xml = XDocument.Load(@"/system/etc/fonts.xml");

                var familyElements = xml.Root.Descendants("family");
                var familyNames = familyElements.Select(element => element.Attribute("name")?.Value).Where(name => !string.IsNullOrEmpty(name));
                result.AddRange(familyNames);

                var aliasElements = xml.Root.Descendants("alias");
                var aliasNames = aliasElements.Select(element => element.Attribute("name")?.Value).Where(name => !string.IsNullOrEmpty(name));
                result.AddRange(aliasNames);

                result.Sort();
                return result;
            }
            catch (Exception)
            {
                return new List<string>();
            }
        }
    }
}
