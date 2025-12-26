// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;

namespace SilentNotes.Models
{
    /// <summary>
    /// List of <see cref="DeletedNoteModel"/> objects, used by a <see cref="NoteRepositoryModel"/>.
    /// </summary>
    public class DeletedNoteListModel : List<DeletedNoteModel>
    {
        /// <summary>
        /// Searches for an item with a given id in the list and returns the found item.
        /// </summary>
        /// <param name="id">Search for the item with this id.</param>
        /// <returns>Found item, or null if no such item exists.</returns>
        public DeletedNoteModel FindById(Guid id)
        {
            return Find(item => item.Id == id);
        }

        /// <summary>
        /// Creates and adds a new deleted note, or updates the deletion date if an item with this
        /// id already exists.
        /// </summary>
        /// <param name="id">The id of the note to add to the list.</param>
        public void AddIdOrRefreshDeletedAt(Guid id)
        {
            DeletedNoteModel deletedNote = FindById(id);
            if (deletedNote != null)
            {
                // Item with id already exists, update the deletion date.
                deletedNote.DeletedAt = DateTime.UtcNow;
            }
            else
            {
                // Add new item.
                Add(new DeletedNoteModel(id));
            }
        }
    }
}
