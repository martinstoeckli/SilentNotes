// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.HtmlView;

namespace SilentNotes.Services
{
    /// <summary>
    /// A service which can get the WebView of the current main window. Use this service to
    /// dynamically get the WebView and do not store a reference to it, see <see cref="SilentNotes.Android.Services.IAppContextService"/>
    /// for more information.
    /// </summary>
    public interface IHtmlViewService
    {
        /// <summary>
        /// Gets the webview of the current main window.
        /// </summary>
        IHtmlView HtmlView { get; }
    }
}
