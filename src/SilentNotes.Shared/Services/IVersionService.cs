// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// Allows to retrieve the version of the application
    /// </summary>
    public interface IVersionService
    {
        /// <summary>
        /// Reads the current version from the application resources.
        /// </summary>
        /// <param name="format">An optional format string, which may contain placeholders for
        /// 4 integer values, one for each part of the version.</param>
        /// <returns>Formatted version.</returns>
        string GetApplicationVersion(string format = "{0}.{1}.{2}");
    }
}
