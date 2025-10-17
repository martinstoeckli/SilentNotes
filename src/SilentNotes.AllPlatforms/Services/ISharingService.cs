// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// Interface of a service which can share/send data to other apps.
    /// </summary>
    public interface ISharingService
    {
        /// <summary>
        /// Shares the HTML content of a note to other apps.
        /// </summary>
        /// <param name="htmlText">The note content.</param>
        /// <param name="plainText">The note content formatted as plain text.</param>
        /// <returns>Task for async calls.</returns>
        Task ShareHtmlText(string htmlText, string plainText);
    }
}
