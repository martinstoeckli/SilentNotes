// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.IO;
using System.Windows.Input;
using SilentNotes.HtmlView;
using SilentNotes.Services;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// Viewmodel to present the stop dialog, when an invalid repository was found.
    /// </summary>
    public class StopViewModel : ViewModelBase
    {
        private readonly IRepositoryStorageService _repositoryService;
        private readonly IFolderPickerService _folderPickerService;
        private readonly IVersionService _versionService;

        /// <summary>
        /// Initializes a new instance of the <see cref="StopViewModel"/> class.
        /// </summary>
        public StopViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IThemeService themeService,
            IBaseUrlService webviewBaseUrl,
            IVersionService versionService,
            IRepositoryStorageService repositoryService,
            IFolderPickerService folderPickerService)
            : base(navigationService, languageService, svgIconService, themeService, webviewBaseUrl)
        {
            _repositoryService = repositoryService;
            _folderPickerService = folderPickerService;
            _versionService = versionService;
            RecoverRepositoryCommand = new RelayCommand(RecoverRepository);
        }

        /// <inheritdoc/>
        public override void OnGoBackPressed(out bool handled)
        {
            handled = false;
        }

        /// <summary>
        /// Gets the command which can safe the repository to a user selected directory.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand RecoverRepositoryCommand { get; private set; }

        private async void RecoverRepository()
        {
            if (await _folderPickerService.PickFolder())
            {
                byte[] repositoryContent = _repositoryService.LoadRepositoryFile();
                await _folderPickerService.TrySaveFileToPickedFolder(
                    Config.RepositoryFileName, repositoryContent);
            }
        }

        /// <summary>
        /// Gets the current version of the assembly/application
        /// </summary>
        [VueDataBinding(VueBindingMode.OneWayToView)]
        public string VersionFmt
        {
            get { return _versionService.GetApplicationVersion(); }
        }
    }
}
