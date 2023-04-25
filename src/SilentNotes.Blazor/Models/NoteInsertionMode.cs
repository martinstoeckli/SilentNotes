// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Models
{
    /// <summary>
    /// Enumeration of all possible places where a new note can be inserted.
    /// </summary>
    public enum NoteInsertionMode
    {
        /// <summary>A new note will be inserted as the first note.</summary>
        AtTop,

        /// <summary>A new note will be appended as the last note.</summary>
        AtBottom,
    }
}
