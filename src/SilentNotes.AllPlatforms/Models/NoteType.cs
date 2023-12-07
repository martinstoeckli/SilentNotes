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

    /// <summary>
    /// Extension methods to the <see cref="NoteType"/> class.
    /// </summary>
    public static class NoteTypeExtensions
    {
        /// <summary>
        /// Gets the value of the <see cref="RouteNames"/> constants matching the note type.
        /// </summary>
        /// <param name="noteType">Note type to get the route name from.</param>
        /// <returns>Route name.</returns>
        public static string GetRouteName(this NoteType noteType)
        {
            switch (noteType)
            {
                case NoteType.Text:
                    return RouteNames.Note;
                case NoteType.Checklist:
                    return RouteNames.Checklist;
                default:
                    throw new ArgumentOutOfRangeException(nameof(NoteType));
            }
        }
    }
}
