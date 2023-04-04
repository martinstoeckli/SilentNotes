// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using AndroidX.Core.Content;
using SilentNotes.Services;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// Implementation of the <see cref="IRepositoryStorageService"/> interface for the Android platform.
    /// </summary>
    internal class RepositoryStorageService : RepositoryStorageServiceBase, IRepositoryStorageService
    {
        private readonly IAppContextService _appContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryStorageService"/> class.
        /// </summary>
        public RepositoryStorageService(IAppContextService appContextService, IXmlFileService xmlFileService, ILanguageService languageService)
            : base(xmlFileService, languageService)
        {
            _appContext = appContextService;
        }

        /// <inheritdoc/>
        protected override string GetDirectoryPath()
        {
            return ContextCompat.GetNoBackupFilesDir(_appContext.Context).AbsolutePath;
        }
    }
}
