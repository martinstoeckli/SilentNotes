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
using SilentNotes.Controllers;
using SilentNotes.Crypto;
using SilentNotes.HtmlView;
using SilentNotes.Models;
using SilentNotes.Services;

namespace SilentNotes.ViewModels
{
    public class ExportViewModel : ViewModelBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IRepositoryStorageService _repositoryService;
        private readonly IFolderPickerService _folderPickerService;
        private readonly ICryptor _noteCryptor;
        private bool _exportUnprotectedNotes;
        private bool _exportProtectedNotes;
        private bool _ignoreFutureNavigations; // Because Android restarts the main activity in the folder picker.

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoViewModel"/> class.
        /// </summary>
        public ExportViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IThemeService themeService,
            IBaseUrlService webviewBaseUrl,
            IFeedbackService feedbackService,
            ICryptoRandomSource randomSource,
            IRepositoryStorageService repositoryService,
            IFolderPickerService folderPickerService)
            : base(navigationService, languageService, svgIconService, themeService, webviewBaseUrl)
        {
            _feedbackService = feedbackService;
            _repositoryService = repositoryService;
            _folderPickerService = folderPickerService;
            _noteCryptor = new Cryptor(NoteModel.CryptorPackageName, randomSource);

            GoBackCommand = new RelayCommand(GoBack);
            CancelCommand = new RelayCommand(Cancel);
            OkCommand = new RelayCommand(Ok);
            ExportUnprotectedNotes = true;
        }

        /// <inheritdoc/>
        public override void OnClosing()
        {
            _ignoreFutureNavigations = true;
            base.OnClosing();
        }

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand GoBackCommand { get; private set; }

        private void GoBack()
        {
            _navigationService.Navigate(new Navigation(ControllerNames.NoteRepository));
        }

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand CancelCommand { get; private set; }

        private void Cancel()
        {
            GoBack();
        }

        /// <summary>
        /// Gets the command to create the service.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand OkCommand { get; private set; }

        private async void Ok()
        {
            if (await _folderPickerService.PickFolder())
            {
                bool success;
                try
                {
                    byte[] zipContent = CreateZipArchive();
                    success = await _folderPickerService.TrySaveFileToPickedFolder(
                        CreateFilename(), zipContent);
                }
                catch (Exception)
                {
                    success = false;
                }

                if (success)
                {
                    _feedbackService.ShowToast(Language.LoadText("export_success"));
                    if (!_ignoreFutureNavigations)
                        _navigationService.Navigate(new Navigation(ControllerNames.NoteRepository));
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
        [VueDataBinding(VueBindingMode.OneWayToView)]
        public bool OkCommandDisabled
        {
            get { return !ExportUnprotectedNotes && !ExportProtectedNotes; }
        }

        public byte[] CreateZipArchive()
        {
            _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel repository);

            byte[] result;
            using (MemoryStream zipContent = new MemoryStream())
            {
                using (ZipArchive zipArchive = new ZipArchive(zipContent, ZipArchiveMode.Create))
                {
                    List<string> allTags = repository.CollectActiveTags();
                    foreach (NoteModel note in EnumerateNotesToExport(repository, ExportUnprotectedNotes, ExportProtectedNotes))
                    {
                        NoteViewModel noteViewModel = new NoteViewModel(null, null, null, null, null, null, null, null, null, null, _noteCryptor, repository.Safes, allTags, note);

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

        internal static IEnumerable<NoteModel> EnumerateNotesToExport(
            NoteRepositoryModel repository, bool exportUnprotectedNotes, bool exportProtectedNotes)
        {
            foreach (NoteModel note in repository.Notes)
            {
                // Ignore deleted notes
                if (note.InRecyclingBin)
                    continue;

                if (!note.SafeIdSpecified)
                {
                    if (exportUnprotectedNotes)
                        yield return note; // Unprotected note
                }
                else
                {
                    if (exportProtectedNotes && repository.Safes.FindById(note.SafeId).IsOpen)
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
        [VueDataBinding(VueBindingMode.TwoWay)]
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
        [VueDataBinding(VueBindingMode.TwoWay)]
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
        [VueDataBinding(VueBindingMode.OneWayToView)]
        public bool HasExportableProtectedNotes
        {
            get
            {
                _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);
                bool hasAtLeastOneOpenSafe = noteRepository.Safes.Any(safe => safe.IsOpen);
                return hasAtLeastOneOpenSafe;
            }
        }

        /// <inheritdoc/>
        public override void OnGoBackPressed(out bool handled)
        {
            handled = true;
            GoBack();
        }

        private string CreateFilename()
        {
            string dateTimePart = DateTime.Now.ToString("yyyyMMdd_HHmm");
            return string.Format("silentnotes_export_{0}.zip", dateTimePart);
        }
    }
}
