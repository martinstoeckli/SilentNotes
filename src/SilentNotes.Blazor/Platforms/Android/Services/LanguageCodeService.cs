// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.Services;

namespace SilentNotes.Platforms.Services
{
    /// <summary>
    /// Implementation of the <see cref="ILanguageCodeProvider"/> interface for the Android platform.
    /// </summary>
    internal class LanguageCodeService : ILanguageCodeProvider
    {
        /// <inheritdoc/>
        public string GetSystemLanguageCode()
        {
            string languageCode = Java.Util.Locale.Default.Language;
            return languageCode.Substring(0, 2).ToLowerInvariant();
        }
    }
}