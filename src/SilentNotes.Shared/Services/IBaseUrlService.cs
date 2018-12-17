// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// Allows to define different base urls for specific platforms.
    /// </summary>
    public interface IBaseUrlService
    {
        /// <summary>
        /// Gets the base url for the current platform, as it should be inserted in the
        /// Html "base" tag.
        /// </summary>
        string HtmlBase { get; }
    }
}
