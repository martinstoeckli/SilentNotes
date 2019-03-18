// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml.Linq;
using SilentNotes.Models;
using SilentNotes.Workers;

namespace SilentNotes.Services
{
    /// <summary>
    /// Abstract base implementation of the <see cref="IRepositoryStorageService"/> interface.
    /// </summary>
    public abstract class RepositoryStorageServiceBase : IRepositoryStorageService
    {
        /// <summary>Gets the injected dependency to an xml file service.</summary>
        protected readonly IXmlFileService _xmlFileService;

        /// <summary>Gets the injected dependency to a language service.</summary>
        protected readonly ILanguageService _languageService;

        /// <summary>Gets the updater which can update old repositories.</summary>
        protected readonly INoteRepositoryUpdater _updater;

        private NoteRepositoryModel _cachedRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="RepositoryStorageServiceBase"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public RepositoryStorageServiceBase(IXmlFileService xmlFileService, ILanguageService languageService)
        {
            _xmlFileService = xmlFileService;
            _languageService = languageService;
            _updater = new NoteRepositoryUpdater();
        }

        /// <inheritdoc/>
        public bool RepositoryExists()
        {
            string repositoryFilePath = Path.Combine(GetDirectoryPath(), Config.RepositoryFileName);
            return _xmlFileService.Exists(repositoryFilePath);
        }

        /// <inheritdoc/>
        public RepositoryStorageLoadResult LoadRepositoryOrDefault(out NoteRepositoryModel repositoryModel)
        {
#if (ENV_DEMO && DEBUG)
            if ((_cachedRepository == null) && (Config.RunningMode == Config.RunningModes.Demo))
                _cachedRepository = new DemoNoteRepositoryModel();
#endif

            if (_cachedRepository != null)
            {
                repositoryModel = _cachedRepository;
                return RepositoryStorageLoadResult.SuccessfullyLoaded;
            }

            RepositoryStorageLoadResult result;
            repositoryModel = null;
            bool modelWasUpdated = false;
            try
            {
                // A new repository is created only if it does not yet exist, we won't overwrite
                // an invalid repository.
                if (RepositoryExists())
                {
                    string xmlFilePath = Path.Combine(GetDirectoryPath(), Config.RepositoryFileName);
                    if (!_xmlFileService.TryLoad(xmlFilePath, out XDocument xml))
                        throw new Exception("Invalid XML");

                    result = RepositoryStorageLoadResult.SuccessfullyLoaded;
                    modelWasUpdated = _updater.Update(xml);
                    repositoryModel = XmlUtils.DeserializeFromXmlDocument<NoteRepositoryModel>(xml);
                }
                else
                {
                    result = RepositoryStorageLoadResult.CreatedNewEmptyRepository;
                    repositoryModel = new NoteRepositoryModel();
                    repositoryModel.Revision = NoteRepositoryModel.NewestSupportedRevision;
                    AddWelcomeNote(repositoryModel);
                    modelWasUpdated = true;
                }
            }
            catch (Exception)
            {
                result = RepositoryStorageLoadResult.InvalidRepository;
                repositoryModel = null;
                modelWasUpdated = false;
            }

            // Automatically save settings if they where modified by an update
            if (modelWasUpdated)
                TrySaveRepository(repositoryModel);
            _cachedRepository = repositoryModel;
            return result;
        }

        /// <inheritdoc/>
        public bool TrySaveRepository(NoteRepositoryModel repositoryModel)
        {
            try
            {
                string xmlFilePath = Path.Combine(GetDirectoryPath(), Config.RepositoryFileName);
                bool success = _xmlFileService.TrySerializeAndSave(xmlFilePath, repositoryModel);
                if (success)
                    _cachedRepository = repositoryModel;
                return success;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Sub classes should override this method to change the directory, where the config is stored.
        /// </summary>
        /// <returns>The full directory path for storing the config.</returns>
        protected abstract string GetDirectoryPath();

        private void AddWelcomeNote(NoteRepositoryModel repositoryModel)
        {
            NoteModel welcomeNote = new NoteModel
            {
                HtmlContent = _languageService.LoadText("welcome_note"),
            };
            repositoryModel.Notes.Add(welcomeNote);
        }
    }
}
