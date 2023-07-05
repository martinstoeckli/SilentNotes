// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace SilentNotes.Models
{
    /// <summary>
    /// List of notes.
    /// </summary>
    public class NoteListModel : List<NoteModel>
    {
        /// <summary>
        /// Searches for a note with a given id and returns its index.
        /// </summary>
        /// <param name="id">Search for the note with this id.</param>
        /// <returns>Returns the index of the found note, or -1 if the note could not be found.</returns>
        public int IndexOfById(Guid id)
        {
            for (int index = 0; index < Count; index++)
            {
                if (id == this[index].Id)
                    return index;
            }
            return -1;
        }

        /// <summary>
        /// Determines whether a note with a given id is in the repository.
        /// </summary>
        /// <param name="id">Search for the note with this id.</param>
        /// <returns>Returns true if the note is in the repository, otherwise false.</returns>
        public bool ContainsById(Guid id)
        {
            int index = IndexOfById(id);
            return index >= 0;
        }

        /// <summary>
        /// Searches for a note with a given id in the repository and returns the found note.
        /// </summary>
        /// <param name="id">Search for the note with this id.</param>
        /// <returns>Found note, or null if no such note exists.</returns>
        public NoteModel FindById(Guid id)
        {
            return Find(item => item.Id == id);
        }

        /// <summary>
        /// Searches for the first note with <see cref="NoteModel.IsPinned"/> is false, and returns
        /// the zero based index of this note.
        /// </summary>
        /// <returns>Returns index of first unpinned note, or -1 id no such note was found.</returns>
        public int IndexOfFirstUnpinnedNote()
        {
            for (int index = 0; index < Count; index++)
            {
                if (!this[index].IsPinned)
                    return index;
            }
            return -1;
        }
    }
}
