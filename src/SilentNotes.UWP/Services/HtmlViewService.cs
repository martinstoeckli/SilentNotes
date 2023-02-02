// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.HtmlView;
using SilentNotes.Services;

namespace SilentNotes.UWP.Services
{
    /// <summary>
    /// Implementation of the <see cref="IHtmlViewService"/> for the UWP platform.
    /// </summary>
    internal class HtmlViewService : IHtmlViewService
    {
        private readonly IMainWindowService _mainWindow;

        /// <summary>
        /// Initialize a new instance of the <see cref="HtmlViewService"/> class.
        /// </summary>
        /// <param name="mainWindowService">A service which knows about the main window.</param>
        public HtmlViewService(IMainWindowService mainWindowService)
        {
            _mainWindow = mainWindowService;
        }

        /// <inheritdoc/>
        public IHtmlView HtmlView 
        { 
            get { return _mainWindow.MainPage as IHtmlView; }
        }
    }
}
