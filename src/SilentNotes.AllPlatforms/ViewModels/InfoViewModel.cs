// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Text;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
using SilentNotes.Models;
using SilentNotes.Services;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the info dialog to the user.
    /// </summary>
    public class InfoViewModel : ViewModelBase
    {
        private readonly IVersionService _versionService;
        private readonly INativeBrowserService _nativeBrowserService;
        private readonly IRepositoryStorageService _repositoryStorageService;
        private readonly IFilePickerService _filePickerService;
        private readonly IFeedbackService _feedbackService;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoViewModel"/> class.
        /// </summary>
        public InfoViewModel(
            IVersionService versionService,
            INativeBrowserService nativeBrowserService,
            IRepositoryStorageService repositoryStorageService,
            IFilePickerService filePickerService,
            IFeedbackService feedbackService)
        {
            _versionService = versionService;
            _nativeBrowserService = nativeBrowserService;
            _repositoryStorageService = repositoryStorageService;
            _filePickerService = filePickerService;
            _feedbackService = feedbackService;
            OpenHomepageCommand = new RelayCommand(OpenHomepage);
            BackupRepositoryCommand = new RelayCommand(BackupRepository);
            RestoreRepositoryCommand = new RelayCommand(RestoreRepository);
        }

        /// <summary>
        /// Gets the current version of the assembly/application
        /// </summary>
        public string VersionFmt
        {
            get { return _versionService.GetApplicationVersion(); }
        }

        /// <summary>
        /// Gets additonal support informations.
        /// </summary>
        public MarkupString SupportInfos
        {
            get
            {
                StringBuilder result = new StringBuilder();
                result.Append("<table>");

                result.Append("<tr>");
                result.Append("<th>Exe location</th>");
                result.Append("<td>");
                result.Append(System.Reflection.Assembly.GetExecutingAssembly().Location);
                result.Append("</td>");
                result.Append("</tr>");

                result.Append("<tr>");
                result.Append("<th>Repository location</th>");
                result.Append("<td>");
                result.Append((_repositoryStorageService as RepositoryStorageServiceBase).GetLocation());
                result.Append("</td>");
                result.Append("</tr>");

                result.Append("</table>");
                return (MarkupString)result.ToString();
            }
        }

        /// <summary>
        /// Gets the command to open the applications homepage.
        /// </summary>
        public ICommand OpenHomepageCommand { get; private set; }

        private void OpenHomepage()
        {
            _nativeBrowserService.OpenWebsite("https://www.martinstoeckli.ch/silentnotes");
        }

        public ICommand BackupRepositoryCommand { get; private set; }

        private async void BackupRepository()
        {
            await _feedbackService.ShowMessageAsync("This function is deprecated, use the backup functions on the overview page instead.", null, MessageBoxButtons.Ok, false);
        }

        public ICommand RestoreRepositoryCommand { get; private set; }

        private async void RestoreRepository()
        {
            // This function is deprecated, use the backup functions on the overview page instead.
            MessageBoxResult answer = await _feedbackService.ShowMessageAsync(
                "This will override all existing notes with the notes from the backup! Are you really sure you want to continue?",
                "Restore backup",
                MessageBoxButtons.ContinueCancel,
                true);
            if (answer != MessageBoxResult.Continue)
                return;

            if (await _filePickerService.PickFile())
            {
                byte[] fileContent = await _filePickerService.ReadPickedFile();

                if (_repositoryStorageService.TryLoadRepositoryFromFile(fileContent, out NoteRepositoryModel noteRepository))
                    _repositoryStorageService.TrySaveRepository(noteRepository);
                else
                    await _feedbackService.ShowMessageAsync("Could not load a repository from the selected file.", null, MessageBoxButtons.Ok, false);
            }
        }

        private string GetRepositoryFilenameWithDate()
        {
            string datePart = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(NoteRepositoryModel.RepositoryFileName);
            string fileExt = Path.GetExtension(NoteRepositoryModel.RepositoryFileName);
            return string.Format("{0}_{1}{2}", fileNameWithoutExt, datePart, fileExt);
        }
    }
}
