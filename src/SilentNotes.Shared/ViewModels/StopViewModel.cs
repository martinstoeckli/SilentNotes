// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
using SilentNotes.Services;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// Viewmodel to present the stop dialog, when an invalid repository was found.
    /// </summary>
    public class StopViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StopViewModel"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public StopViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IThemeService themeService,
            IBaseUrlService webviewBaseUrl)
            : base(navigationService, languageService, svgIconService, themeService, webviewBaseUrl)
        {
        }

        /// <inheritdoc/>
        public override void OnGoBackPressed(out bool handled)
        {
            handled = false;
        }
    }
}
