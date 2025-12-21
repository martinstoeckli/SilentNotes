// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SilentNotes.Crypto;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotes.ViewModels
{
    public class ExportViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IFeedbackService _feedbackService;
        private readonly IRepositoryStorageService _repositoryService;
        private readonly IFolderPickerService _folderPickerService;
        private readonly ISafeKeyService _keyService;
        private readonly IMarkdownConverter _markdownConverter;
        private readonly ICryptor _noteCryptor;
        private bool _exportUnprotectedNotes;
        private bool _exportProtectedNotes;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoViewModel"/> class.
        /// </summary>
        public ExportViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            ICryptoRandomSource randomSource,
            IRepositoryStorageService repositoryService,
            IFolderPickerService folderPickerService,
            ISafeKeyService safeKeyService,
            IMarkdownConverter markdownConverter)
        {
            Language = languageService;
            _navigationService = navigationService;
            _feedbackService = feedbackService;
            _repositoryService = repositoryService;
            _folderPickerService = folderPickerService;
            _keyService = safeKeyService;
            _markdownConverter = markdownConverter;
            _noteCryptor = new Cryptor(NoteModel.CryptorPackageName, randomSource);

            ExportFormat = "html";
            OkCommand = new RelayCommand(Ok);
            ExportUnprotectedNotes = true;
        }

        private ILanguageService Language { get; }

        /// <summary>
        /// Gets or sets the output type.
        /// </summary>
        public string ExportFormat { get; set; }

        /// <summary>
        /// Gets the command to create the service.
        /// </summary>
        public ICommand OkCommand { get; private set; }

        private async void Ok()
        {
            if (await _folderPickerService.PickFolder())
            {
                bool success;
                try
                {
                    switch (ExportFormat)
                    {
                        case "html":
                            byte[] zipContent = CreateHtmlZipArchive();
                            success = await _folderPickerService.TrySaveFileToPickedFolder(
                                CreateFilename(".zip"), zipContent);
                            break;
                        case "jex":
                            byte[] jexContent = await CreateJexArchive();
                            success = await _folderPickerService.TrySaveFileToPickedFolder(
                                CreateFilename(".jex"), jexContent);
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
                    _feedbackService.ShowToast(Language.LoadText("export_success"));
                    _navigationService.NavigateTo(RouteNames.NoteRepository);
                }
                else
                {
                    _feedbackService.ShowToast(Language.LoadText("export_error"));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="OkCommand"/> is disabled.
        /// </summary>
        public bool OkCommandDisabled
        {
            get { return !ExportUnprotectedNotes && !ExportProtectedNotes; }
        }

        public byte[] CreateHtmlZipArchive()
        {
            _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel repository);

            byte[] result;
            using (MemoryStream zipContent = new MemoryStream())
            {
                using (ZipArchive zipArchive = new ZipArchive(zipContent, ZipArchiveMode.Create))
                {
                    List<string> allTags = repository.CollectActiveTags();
                    foreach (NoteModel note in EnumerateNotesToExport(repository, _keyService, ExportUnprotectedNotes, ExportProtectedNotes))
                    {
                        var noteViewModel = new NoteViewModelReadOnly(note, null, null, null, _keyService, _noteCryptor, repository.Safes);

                        string filename = CreateFilenameForNote(note.Id);
                        string html = AddHtmlSkeleton(note.Id, noteViewModel.UnlockedHtmlContent);
                        byte[] content = CryptoUtils.StringToBytes(html);

                        ZipArchiveEntry zipEntry = zipArchive.CreateEntry(filename);
                        using (Stream writer = zipEntry.Open())
                        {
                            writer.Write(content, 0, content.Length);
                        }
                    }
                }
                result = zipContent.ToArray();
            }
            return result;
        }

        public async Task<byte[]> CreateJexArchive()
        {
            _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel repository);

            byte[] result;
            using (MemoryStream archiveContent = new MemoryStream())
            {
                var exporter = new JexImporterExporter(_markdownConverter);
                var notes = EnumerateNotesToExport(repository, _keyService, ExportUnprotectedNotes, ExportProtectedNotes);
                var jexFileEntries = await exporter.CreateJexFileEntriesFromRepository(repository.Id, notes);
                exporter.WriteToJexFile(jexFileEntries, archiveContent);
                result = archiveContent.ToArray();
            }
            return result;
        }

        internal static IEnumerable<NoteModel> EnumerateNotesToExport(
            NoteRepositoryModel repository, ISafeKeyService keyService, bool exportUnprotectedNotes, bool exportProtectedNotes)
        {
            foreach (NoteModel note in repository.Notes)
            {
                // Ignore deleted notes
                if (note.InRecyclingBin)
                    continue;

                if (!note.SafeId.HasValue)
                {
                    if (exportUnprotectedNotes)
                        yield return note; // Unprotected note
                }
                else
                {
                    if (exportProtectedNotes && keyService.IsSafeOpen(note.SafeId.Value))
                        yield return note; // Protected note
                }
            }
        }

        private static string CreateFilenameForNote(Guid noteId)
        {
            return string.Format("{0}.html", noteId.ToString("D"));
        }

        private static string AddHtmlSkeleton(Guid noteId, string content)
        {
            const string HtmlSkeleton = @"<!DOCTYPE html>
<html>
  <head>
    <meta charset='utf-8'>
    <title>SilentNotes note {0}</title>
  </head>
  <body>
{1}  
  </body>
</html>";
            return string.Format(HtmlSkeleton, noteId.ToString("D"), content);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the notes outside of a safe should be exported.
        /// </summary>
        public bool ExportUnprotectedNotes 
        {
            get { return _exportUnprotectedNotes; }

            set 
            {
                if (SetProperty(ref _exportUnprotectedNotes, value))
                    OnPropertyChanged(nameof(OkCommandDisabled));
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the notes residing in a safe should be exported.
        /// Only open safes are considered.
        /// </summary>
        public bool ExportProtectedNotes
        {
            get { return _exportProtectedNotes; }

            set
            {
                if (SetProperty(ref _exportProtectedNotes, value))
                    OnPropertyChanged(nameof(OkCommandDisabled));
            }
        }

        /// <summary>
        /// Gets a value indicating whether there are notes residing in an open safe.
        /// </summary>
        public bool HasExportableProtectedNotes
        {
            get
            {
                _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);
                bool hasAtLeastOneOpenSafe = noteRepository.Safes.Any(safe => _keyService.IsSafeOpen(safe.Id));
                return hasAtLeastOneOpenSafe;
            }
        }

        /// <summary>
        /// Creates a filename containing a timestamp.
        /// </summary>
        /// <param name="extension">The file extension to use.</param>
        /// <returns>Time based filename.</returns>
        private string CreateFilename(string extension)
        {
            string dateTimePart = DateTime.Now.ToString("yyyyMMdd_HHmm");
            return string.Format("silentnotes_export_{0}.{1}", dateTimePart, extension.TrimStart('.'));
        }
    }
}
