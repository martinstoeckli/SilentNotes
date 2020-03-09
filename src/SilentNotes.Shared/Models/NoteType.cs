// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Models
{
    /// <summary>
    /// Enumeration of all note types.
    /// </summary>
    public enum NoteType
    {
        /// <summary>The note contains a classic formatted text note.</summary>
        Text,

        /// <summary>The note contains a checklist</summary>
        Checklist
    }
}
