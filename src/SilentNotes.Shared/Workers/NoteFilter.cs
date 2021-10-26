// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Helper class to filter notes by a search pattern.
    /// </summary>
    public class NoteFilter
    {
        private readonly string _pattern;
        private readonly string _tag;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteFilter"/> class.
        /// </summary>
        /// <param name="pattern">User defined search filter.</param>
        /// <param name="tag">User defined tag to search for.</param>
        public NoteFilter(string pattern, string tag)
        {
            _pattern = pattern;
            _tag = tag;
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

        /// <summary>
        /// Checks whether a note contains a given tag.
        /// </summary>
        /// <param name="noteTags">Tags of the note to test.</param>
        /// <returns>Returns true if the note contains the tag, otherwise false.</returns>
        public bool ContainsTag(IEnumerable<string> noteTags)
        {
            // There is no tag to search for
            if (String.IsNullOrEmpty(_tag))
                return true;

            // There is a tag to search for, but no tags in the list
            if (noteTags == null)
                return false;

            return noteTags.Contains(_tag, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
