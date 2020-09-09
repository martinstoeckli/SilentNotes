// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.Controllers;
using SilentNotes.HtmlView;
using SilentNotes.Models;
using SilentNotes.Services;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Input;

namespace SilentNotes.ViewModels
{
    public class ExportViewModel : ViewModelBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly IRepositoryStorageService _repositoryService;
        private readonly IFolderPickerService _folderPickerService;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoViewModel"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public ExportViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IThemeService themeService,
            IBaseUrlService webviewBaseUrl,
            IFeedbackService feedbackService,
            IRepositoryStorageService repositoryService,
            IFolderPickerService folderPickerService)
            : base(navigationService, languageService, svgIconService, themeService, webviewBaseUrl)
        {
            _feedbackService = feedbackService;
            _repositoryService = repositoryService;
            _folderPickerService = folderPickerService;

            GoBackCommand = new RelayCommand(GoBack);
            CancelCommand = new RelayCommand(Cancel);
            OkCommand = new RelayCommand(Ok);
            ExportUnprotectedNotes = true;
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
                    byte[] content = new byte[] { 88, 99 };
                    success = await _folderPickerService.TrySaveFileToPickedFolder(
                        CreateFilename(), content);
                }
                catch (Exception)
                {
                    success = false;
                }

                if (success)
                {
                    _feedbackService.ShowToast(Language.LoadText("export_success"));
                    _navigationService.Navigate(new Navigation(ControllerNames.NoteRepository));
                }
                else
                {
                    _feedbackService.ShowToast(Language.LoadText("export_error"));
                }
            }
        }

        [VueDataBinding(VueBindingMode.TwoWay)]
        public bool ExportUnprotectedNotes { get; set; }

        [VueDataBinding(VueBindingMode.TwoWay)]
        public bool ExportProtectedNotes { get; set; }

        [VueDataBinding(VueBindingMode.OneWayToView)]
        public bool CanExportProtectedNotes
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
