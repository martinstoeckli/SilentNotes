// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// Allows the app to work with an external browser (standard browser of the platform).
    /// </summary>
    public interface INativeBrowserService
    {
        /// <summary>
        /// Opens a website in the external browser.
        /// </summary>
        /// <param name="url">Url to the website.</param>
        void OpenWebsite(string url);

        /// <summary>
        /// Opens a website using the external browser inside the app if possible.
        /// </summary>
        /// <remarks>
        /// OAuth2 authentication requires a login in an external browser, but we want to avoid
        /// switching between app and browser. With Androids "custom tabs" we can start the browser
        /// inside the app, thus the app remains active and the browser can be closed afterwards.
        /// </remarks>
        /// <param name="url">Url to the website.</param>
        void OpenWebsiteInApp(string url);
    }
}
