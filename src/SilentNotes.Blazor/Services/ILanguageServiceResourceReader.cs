// Copyright © 2021 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.IO;
using System.Threading.Tasks;

namespace SilentNotes.Services
{
    /// <summary>
    /// Used by a <see cref="ILanguageService"/> to read the resources.
    /// </summary>
    public interface ILanguageServiceResourceReader
    {
        /// <summary>
        /// Tries to open a language resource stream for a given language file.
        /// </summary>
        /// <param name="domain">The domain name defines the pattern of the language resource files
        /// which can be found by the lanugage service. The format is a preceding "Lng.", followed
        /// by the <paramref name="domain"/> and terminated by the <paramref name="languageCode"/>.
        /// Example: Lng.MyApp.en</param>
        /// <param name="languageCode">Two digit language code.</param>
        /// <returns>Open resource stream, or null if no such resource could be found.</returns>
        Task<Stream> TryOpenResourceStream(string domain, string languageCode);
    }
}
