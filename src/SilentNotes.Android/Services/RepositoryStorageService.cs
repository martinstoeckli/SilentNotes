// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
using Android.Content;
using AndroidX.Core.Content;
using SilentNotes.Services;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// Implementation of the <see cref="IRepositoryStorageService"/> interface for the Android platform.
    /// </summary>
    public class RepositoryStorageService : RepositoryStorageServiceBase, IRepositoryStorageService
    {
        private readonly Context _appContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryStorageService"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public RepositoryStorageService(Context appContext, IXmlFileService xmlFileService, ILanguageService languageService)
            : base(xmlFileService, languageService)
        {
            _appContext = appContext;
        }

        /// <inheritdoc/>
        protected override string GetDirectoryPath()
        {
            return ContextCompat.GetNoBackupFilesDir(_appContext).AbsolutePath;
        }
    }
}
