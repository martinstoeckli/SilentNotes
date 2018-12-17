// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Helper class to filter notes by a search pattern.
    /// </summary>
    public class NoteFilter
    {
        private readonly string _pattern;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteFilter"/> class.
        /// </summary>
        /// <param name="pattern">User defined search filter.</param>
        public NoteFilter(string pattern)
        {
            _pattern = pattern;
        }

        /// <summary>
        /// Checks whether a note matches with a given search pattern.
        /// </summary>
        /// <param name="searchableNoteContent">Note content to test.</param>
        /// <returns>Returns true if the note matches the pattern, otherwise false.</returns>
        public bool ContainsPattern(string searchableNoteContent)
        {
            if (string.IsNullOrEmpty(_pattern))
                return true;

            // Search in content
            if (!string.IsNullOrEmpty(searchableNoteContent) && (searchableNoteContent.IndexOf(_pattern, StringComparison.OrdinalIgnoreCase) >= 0))
                return true;

            return false;
        }
    }
}
