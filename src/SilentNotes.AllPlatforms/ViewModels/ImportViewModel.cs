// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotes.ViewModels
{
    public class ImportViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IFeedbackService _feedbackService;
        private readonly IRepositoryStorageService _repositoryService;
        private readonly IFilePickerService _filePickerService;
        private readonly IMarkdownConverter _markdownConverter;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoViewModel"/> class.
        /// </summary>
        public ImportViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            IRepositoryStorageService repositoryService,
            IFilePickerService filePickerService,
            IMarkdownConverter markdownConverter)
        {
            Language = languageService;
            _navigationService = navigationService;
            _feedbackService = feedbackService;
            _repositoryService = repositoryService;
            _filePickerService = filePickerService;
            _markdownConverter = markdownConverter;

            ImportFormat = "jex";
            Strategy = ImportStrategy.IgnoreExisting;
            OkCommand = new RelayCommand(Ok);
        }

        private ILanguageService Language { get; }

        /// <summary>
        /// Gets or sets the output type.
        /// </summary>
        public string ImportFormat { get; set; }

        /// <summary>
        /// Gets or sets the import strategy.
        /// </summary>
        public ImportStrategy Strategy { get; set; }

        /// <summary>
        /// Gets the command to create the service.
        /// </summary>
        public ICommand OkCommand { get; private set; }

        private async void Ok()
        {
            string[] fileExtensions;
            switch (ImportFormat)
            {
                case "jex":
                    fileExtensions = new[] { ".jex" };
                    break;
                default:
                    fileExtensions = null;
                    break;
            }

            if (await _filePickerService.PickFile(fileExtensions))
            {
                bool success;
                try
                {
                    _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel repository);
                    byte[] fileContent = await _filePickerService.ReadPickedFile();
                    switch (ImportFormat)
                    {
                        case "jex":
                            success = await LoadNotesFromJexArchive(repository, fileContent, Strategy);
                            if (success)
                                _repositoryService.TrySaveRepository(repository);
                            success = true;
                            break;
                        default:
                            success = false;
                            break;
                    }
                }
                catch (Exception)
                {
                    success = false;
                }

                if (success)
                {
                    _feedbackService.ShowToast(Language.LoadText("import_success"));
                    _navigationService.NavigateTo(RouteNames.NoteRepository);
                }
                else
                {
                    _feedbackService.ShowToast(Language.LoadText("import_error"));
                }
            }
        }

        private async Task<bool> LoadNotesFromJexArchive(NoteRepositoryModel repository, byte[] fileContent, ImportStrategy strategy)
        {
            var importer = new JexImporterExporter(_markdownConverter);
            using (MemoryStream archiveContent = new MemoryStream(fileContent))
            {
                if (importer.TryReadFromJexFile(archiveContent, out List<JexFileEntry> jexFileEntries))
                {
                    NoteListModel importNotes = await importer.CreateRepositoryFromJexFileEntries(jexFileEntries);
                    await LoadNotesFromImportedNoteList(repository, importNotes, strategy);
                    return true;
                }
            }
            return false;
        }

        internal static async Task LoadNotesFromImportedNoteList(NoteRepositoryModel repository, NoteListModel importNotes, ImportStrategy strategy)
        {
            foreach (NoteModel importNote in importNotes)
            {
                // Check whether a note with the same id already exists
                int existingNotePos = repository.Notes.IndexOfById(importNote.Id);
                if ((existingNotePos >= 0) && (strategy == ImportStrategy.IgnoreExisting))
                    continue;

                // The modification date should be the most recent, so a synchronization
                // won't the imported note afterwards.
                importNote.RefreshMetaModifiedAt();

                // todo: handle DeletedNotes list
                // repository.DeletedNotes

                if (existingNotePos >= 0)
                    repository.Notes[existingNotePos] = importNote;
                else
                    repository.Notes.Add(importNote);
            }
        }
    }
}
