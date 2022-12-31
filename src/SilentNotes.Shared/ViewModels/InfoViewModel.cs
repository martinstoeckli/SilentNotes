// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SilentNotes.Controllers;
using SilentNotes.HtmlView;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoViewModel"/> class.
        /// </summary>
        public InfoViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IThemeService themeService,
            IBaseUrlService webviewBaseUrl,
            IVersionService versionService,
            INativeBrowserService nativeBrowserService)
            : base(navigationService, languageService, svgIconService, themeService, webviewBaseUrl)
        {
            _versionService = versionService;
            _nativeBrowserService = nativeBrowserService;
            GoBackCommand = new RelayCommand(GoBack);
            OpenHomepageCommand = new RelayCommand(OpenHomepage);
        }

        /// <summary>
        /// Gets the current version of the assembly/application
        /// </summary>
        [VueDataBinding(VueBindingMode.OneWayToView)]
        public string VersionFmt
        {
            get { return _versionService.GetApplicationVersion(); }
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

        /// <inheritdoc/>
        public override void OnGoBackPressed(out bool handled)
        {
            handled = true;
            GoBack();
        }

        /// <summary>
        /// Gets the command to open the applications homepage.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand OpenHomepageCommand { get; private set; }

        private void OpenHomepage()
        {
            _nativeBrowserService.OpenWebsite("https://www.martinstoeckli.ch/silentnotes");
        }
    }
}
