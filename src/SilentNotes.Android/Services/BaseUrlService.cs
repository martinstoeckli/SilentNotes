// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.Services;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// Android specific implementation of the <see cref="IBaseUrlService"/> interface.
    /// </summary>
    internal class BaseUrlService : IBaseUrlService
    {
        /// <inheritdoc/>
        public string HtmlBase
        {
            get { return "file:///android_asset/Html/"; }
        }
    }
}