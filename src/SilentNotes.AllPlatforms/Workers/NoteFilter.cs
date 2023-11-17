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
        private readonly HashSet<string> _userDefinedTags;
        private readonly FilterOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteFilter"/> class.
        /// </summary>
        /// <param name="pattern">User defined search filter.</param>
        /// <param name="userDefinedTags">List of user defined tag to search for.</param>
        /// <param name="options">Options how to filter the notes.</param>
        public NoteFilter(string pattern, IEnumerable<string> userDefinedTags, FilterOptions options)
        {
            _pattern = pattern;
            if (userDefinedTags == null)
                userDefinedTags = new string[0];
            _userDefinedTags = new HashSet<string>(
                userDefinedTags.Where(tag => !string.IsNullOrWhiteSpace(tag)),
                StringComparer.InvariantCultureIgnoreCase);
            _options = options;
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
        /// Checks whether a note contains all user defined tags.
        /// </summary>
        /// <param name="noteTags">Tags of the note to test.</param>
        /// <returns>Returns true if the note contains the tags, otherwise false.</returns>
        public bool MatchTags(List<string> noteTags)
        {
            switch (_options)
            {
                case FilterOptions.FilterByTagList:
                    // There is no tag to search for (no filtering)
                    if (_userDefinedTags.Count == 0)
                        return true;

                    // There is a tag to search for, but no tags in the note
                    bool hasNoteTags = (noteTags != null) && noteTags.Any();
                    if (!hasNoteTags)
                        return false;

                    // Check whether all required tags exist in the tags of the note
                    int foundTags = 0;
                    foreach (string noteTag in noteTags)
                    {
                        if (_userDefinedTags.Contains(noteTag))
                            foundTags++;
                        if (foundTags >= _userDefinedTags.Count)
                            return true;
                    }
                    return false;
                case FilterOptions.NotesWithoutTags:
                    return (noteTags == null) || !noteTags.Any();
                default:
                    throw new ArgumentOutOfRangeException(nameof(FilterOptions));
            }
        }

        /// <summary>
        /// All possible options for tag filtering
        /// </summary>
        public enum FilterOptions
        {
            /// <summary>Filters by the user defined tags</summary>
            FilterByTagList,

            /// <summary>Finds all notes with no tags attached</summary>
            NotesWithoutTags,
        }
    }
}
