// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.Services;

namespace SilentNotes.Services
{
    /// <summary>
    /// Impelementation of the <see cref="IClipboardService"/> interface.
    /// </summary>
    internal class ClipboardService : IClipboardService
    {
        /// <inheritdoc/>
        public async Task SetTextAsync(string text)
        {
            await Clipboard.SetTextAsync(text);
        }
    }
}
