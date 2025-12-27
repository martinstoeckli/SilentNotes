// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Models
{
    /// <summary>
    /// Specifies the available strategies for importing notes from other apps.
    /// </summary>
    public enum ImportStrategy
    {
        /// <summary>Already existing notes with the same id will be preserved and the notes are
        /// not imported.</summary>
        IgnoreExisting,

        /// <summary>Already existing notes with the same id will be overwritten by the imported
        /// notes.</summary>
        OverwriteExisting
    }
}
