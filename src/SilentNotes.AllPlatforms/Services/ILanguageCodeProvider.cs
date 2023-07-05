// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// The language code provider is used by the <see cref="ILanguageService"/> and provides
    /// the code of the current system language.
    /// </summary>
    public interface ILanguageCodeProvider
    {
        /// <summary>
        /// Gets the current two digit ISO 639-1 language code of the running system.
        /// Examples are "en" for english, "de" for german, "fr" for french.
        /// </summary>
        /// <returns>Lower case two digit language code.</returns>
        string GetSystemLanguageCode();
    }
}
