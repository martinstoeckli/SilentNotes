// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Text;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.AspNetCore.Components;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoViewModel"/> class.
        /// </summary>
        public InfoViewModel(
            IVersionService versionService,
            INativeBrowserService nativeBrowserService,
            IRepositoryStorageService repositoryStorageService)
        {
            _versionService = versionService;
            _nativeBrowserService = nativeBrowserService;
            _repositoryStorageService = repositoryStorageService;
            OpenHomepageCommand = new RelayCommand(OpenHomepage);
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
    }
}
