// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using SilentNotes.Services;
using Windows.System;

namespace SilentNotes.UWP.Services
{
    /// <summary>
    /// Implementation of the <see cref="INativeBrowserService"/> interface for the UWP platform.
    /// </summary>
    internal class NativeBrowserService : INativeBrowserService
    {
        /// <inheritdoc/>
        public void OpenWebsite(string url)
        {
            OpenWebsiteAsync(url);
        }

        /// <inheritdoc/>
        public void OpenWebsiteInApp(string url)
        {
            OpenWebsite(url);
        }

        private async void OpenWebsiteAsync(string url)
        {
            await Launcher.LaunchUriAsync(new Uri(url));
        }
    }
}
