// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// The language service can load localized text resources.
    /// </summary>
    public interface ILanguageService
    {
        /// <summary>
        /// Gets a given localized text resource.
        /// </summary>
        /// <param name="id">Id of the text resource.</param>
        /// <returns>Localized text resource.</returns>
        string this[string id] { get; }

        /// <summary>
        /// Loads a given localized text resource.
        /// </summary>
        /// <param name="id">Id of the text resource.</param>
        /// <returns>Localized text resource.</returns>
        string LoadText(string id);

        /// <summary>
        /// Loads a given localized text resource and formats it with parameters.
        /// </summary>
        /// <param name="id">Id of the text resource.</param>
        /// <param name="args">Arguments used by the format function.</param>
        /// <returns>Localized formatted text resource.</returns>
        string LoadTextFmt(string id, params object[] args);
    }
}
