﻿// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
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
#if (DEBUG)
        public readonly string InstanceId = Guid.NewGuid().ToString();
#endif

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
        public RepositoryStorageServiceBase(IXmlFileService xmlFileService, ILanguageService languageService)
        {
            _xmlFileService = xmlFileService;
            _languageService = languageService;
            _updater = new NoteRepositoryUpdater();

#if (DEBUG && FORCE_REPO_DEMO)
            _cachedRepository = new DemoNoteRepositoryModel();
#endif
        }

        /// <inheritdoc/>
        public RepositoryStorageLoadResult LoadRepositoryOrDefault(out NoteRepositoryModel repositoryModel)
        {
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
                string xmlFilePath = Path.Combine(GetLocation(), NoteRepositoryModel.RepositoryFileName);

                // A new repository is created only if it does not yet exist, we won't overwrite
                // an invalid repository.
                if (_xmlFileService.Exists(xmlFilePath))
                {
                    if (!_xmlFileService.TryLoad(xmlFilePath, out XDocument xml))
                    {
                        if (!TryRecoverRepositoryFromLegacyWriter(_xmlFileService, xmlFilePath, out xml))
                        {
                            throw new Exception("Invalid XML");
                        }
                    }

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
                repositoryModel = NoteRepositoryModel.InvalidRepository;
                modelWasUpdated = false;
            }

            // Automatically save settings if they where modified by an update
            if (modelWasUpdated)
                TrySaveRepository(repositoryModel);
            _cachedRepository = repositoryModel;
            return result;
        }

        /// <summary>
        /// This method should be removed in future, it tries to recover the repository, if Android
        /// caused a 0 byte file by killing the process while writing, before the
        /// <see cref="AtomicFileWriter"/> (v6.1.4) was able to handle this situation.
        /// </summary>
        private bool TryRecoverRepositoryFromLegacyWriter(IXmlFileService xmlFileService, string xmlFilePath, out XDocument xml)
        {
            bool result = false;
            xml = null;
            long minValidFileSize = 22; // 0 byte files are not accepted
            if (FileExistsAndHasValidSize(xmlFilePath + ".old", minValidFileSize))
            {
                File.Copy(xmlFilePath + ".old", xmlFilePath, true);
                result = _xmlFileService.TryLoad(xmlFilePath, out xml);
            }
            if (!result && FileExistsAndHasValidSize(xmlFilePath + ".new", minValidFileSize))
            {
                File.Copy(xmlFilePath + ".new", xmlFilePath, true);
                result = _xmlFileService.TryLoad(xmlFilePath, out xml);
            }
            return result;
        }

        private static bool FileExistsAndHasValidSize(string filePath, long minValidFileSize)
        {
            if (!File.Exists(filePath))
                return false;

            long fileSize = new FileInfo(filePath).Length;
            return fileSize >= minValidFileSize;
        }

        /// <inheritdoc/>
        public bool TrySaveRepository(NoteRepositoryModel repositoryModel)
        {
#if (DEBUG && FORCE_REPO_DEMO)
            return true;
#endif

            if (Object.ReferenceEquals(NoteRepositoryModel.InvalidRepository, repositoryModel))
                return false;

            try
            {
                string xmlFilePath = Path.Combine(GetLocation(), NoteRepositoryModel.RepositoryFileName);
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

        /// <inheritdoc/>
        public void ClearCache()
        {
            _cachedRepository = null;
        }

        /// <inheritdoc/>
        public byte[] LoadRepositoryFile()
        {
            try
            {
                string xmlFilePath = Path.Combine(GetLocation(), NoteRepositoryModel.RepositoryFileName);
                return File.ReadAllBytes(xmlFilePath);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public bool TryLoadRepositoryFromFile(byte[] fileContent, out NoteRepositoryModel repositoryModel)
        {
            try
            {
                using (Stream xmlStream = new MemoryStream(fileContent))
                {
                    XDocument xml = XDocument.Load(xmlStream, LoadOptions.None);
                    _updater.Update(xml);
                    repositoryModel = XmlUtils.DeserializeFromXmlDocument<NoteRepositoryModel>(xml);
                }
                return true;
            }
            catch (Exception ex)
            {
                repositoryModel = null;
                return false;
            }
        }

        /// <summary>
        /// Sub classes should override this method to change the directory, where the config is stored.
        /// </summary>
        /// <returns>The full directory path for storing the config.</returns>
        public abstract string GetLocation();

        private void AddWelcomeNote(NoteRepositoryModel repositoryModel)
        {
            NoteModel[] notes = new NoteModel[]
            {
                new NoteModel { HtmlContent = _languageService.LoadText("welcome_note"), BackgroundColorHex = "#fbf4c1" },
                new NoteModel { HtmlContent = _languageService.LoadText("welcome_note_2"), BackgroundColorHex = "#d9f8c8" },
                new NoteModel { HtmlContent = _languageService.LoadText("welcome_note_3"), BackgroundColorHex = "#d0f8f9", NoteType = NoteType.Checklist, Tags = new List<string> { _languageService.LoadText("welcome_note_tag") } },
            };
            repositoryModel.Notes.AddRange(notes);
        }
    }
}
