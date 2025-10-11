// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.Models;
using SilentNotes.Services;

namespace SilentNotes.Platforms.Services
{
    /// <summary>
    /// Implementation of the <see cref="IRepositoryStorageService"/> interface for a PWA.
    /// </summary>
    public class RepositoryStorageService : RepositoryStorageServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryStorageService"/> class.
        /// </summary>
        public RepositoryStorageService(IXmlFileService xmlFileService, ILanguageService languageService)
            : base(xmlFileService, languageService)
        {
        }

        /// <inheritdoc/>
        public override string GetLocation()
        {
            return string.Empty;
        }
    }
}
