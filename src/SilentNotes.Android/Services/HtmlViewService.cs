// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.HtmlView;
using SilentNotes.Services;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// A service which can get the WebView of the current main window. Use this service to
    /// dynamically get the WebView and do not store a reference to it, see <see cref="IAppContextService"/>.
    /// </summary>
    internal class HtmlViewService: IHtmlViewService
    {
        private readonly IAppContextService _appContextService;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlViewService"/> class.
        /// </summary>
        /// <param name="appContextService">A service which knows about the current main activity.</param>
        public HtmlViewService(IAppContextService appContextService)
        {
            _appContextService = appContextService;
        }

        /// <inheritdoc/>
        public IHtmlView HtmlView 
        {
            get { return _appContextService.RootActivity as IHtmlView; }
        }
    }
}