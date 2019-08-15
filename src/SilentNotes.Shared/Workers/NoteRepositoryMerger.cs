// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using SilentNotes.Models;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Can merge two repositories into a new repository, containing notes from both.
    /// </summary>
    public class NoteRepositoryMerger
    {
        /// <summary>
        /// Merges the local repository with the server repository.
        /// The resulting repository can be used to replace local and the server repository.
        /// </summary>
        /// <param name="localRepository">Repository from the local device.</param>
        /// <param name="remoteRepository">Repository stored on the server for synchronization.</param>
        /// <returns>The merged repository.</returns>
        public NoteRepositoryModel Merge(NoteRepositoryModel localRepository, NoteRepositoryModel remoteRepository)
        {
            if (localRepository == null)
                throw new ArgumentNullException(nameof(localRepository));
            if (remoteRepository == null)
                throw new ArgumentNullException(nameof(remoteRepository));

            List<Guid> deletedNotes = BuildMergedListOfDeletedNotes(localRepository, remoteRepository);
            deletedNotes.Sort(); // to allow binary search
            NoteListModel localLivingNotes = BuildListOfLivingNotes(localRepository, deletedNotes);
            NoteListModel remoteLivingNotes = BuildListOfLivingNotes(remoteRepository, deletedNotes);
            bool orderInLocalRepo = localRepository.OrderModifiedAt > remoteRepository.OrderModifiedAt;

            // Create new merge repository
            NoteRepositoryModel result = new NoteRepositoryModel();
            result.Revision = NoteRepositoryModel.NewestSupportedRevision;
            result.Id = remoteRepository.Id;
            result.DeletedNotes = deletedNotes;

            if (orderInLocalRepo)
            {
                var map = BuildMapOfLivingNotes(localLivingNotes, remoteLivingNotes);
                result.Notes = CreateNoteList(localLivingNotes, remoteLivingNotes, map);
                result.OrderModifiedAt = localRepository.OrderModifiedAt;
            }
            else
            {
                var map = BuildMapOfLivingNotes(remoteLivingNotes, localLivingNotes);
                result.Notes = CreateNoteList(remoteLivingNotes, localLivingNotes, map);
                result.OrderModifiedAt = remoteRepository.OrderModifiedAt;
            }
            return result;
        }

        /// <summary>
        /// Builds a list with all notes which should be marked as deleted in the new repository.
        /// </summary>
        /// <param name="localRepository">The repository stored on the device.</param>
        /// <param name="remoteRepository">The repository loaded from the cloud storage.</param>
        /// <returns>A list with ids of all deleted notes.</returns>
        private List<Guid> BuildMergedListOfDeletedNotes(NoteRepositoryModel localRepository, NoteRepositoryModel remoteRepository)
        {
            List<Guid> result = new List<Guid>();

            // Add ids from remote repository
            result.AddRange(remoteRepository.DeletedNotes);

            // Add ids from the local repository which exist in the remote repository
            foreach (Guid locallyDeletedNoteId in localRepository.DeletedNotes)
            {
                if (remoteRepository.Notes.ContainsById(locallyDeletedNoteId))
                    result.Add(locallyDeletedNoteId);
            }
            return result;
        }

        /// <summary>
        /// Creates a list of notes and adds all notes which are not deleted.
        /// </summary>
        /// <param name="repository">The repository to get the notes from.</param>
        /// <param name="deletedNotes">Sorted list of guids of deleted notes.</param>
        /// <returns>A list of non-deleted notes.</returns>
        private NoteListModel BuildListOfLivingNotes(NoteRepositoryModel repository, List<Guid> deletedNotes)
        {
            NoteListModel result = new NoteListModel();
            foreach (NoteModel note in repository.Notes)
            {
                bool containedInDeletedNotes = deletedNotes.BinarySearch(note.Id) >= 0;
                if (!containedInDeletedNotes)
                    result.Add(note);
            }
            return result;
        }

        /// <summary>
        /// Builds a relation map between the note indexes of the locale and the remote repository.
        /// Deleted notes are not part of the map.
        /// </summary>
        /// <param name="orderedLivingNotes">Repository whose order is more recent.</param>
        /// <param name="otherLivingNotes">Repository whose order is less important.</param>
        /// <returns>Map with corresponding indexes of the notes of both repositories.</returns>
        private Map BuildMapOfLivingNotes(NoteListModel orderedLivingNotes, NoteListModel otherLivingNotes)
        {
            var result = new Map();
            int orderedIndex = 0;
            int otherIndex = 0;

            AddFollowingLonelyNotesToMap(result, orderedLivingNotes, otherLivingNotes, ref orderedIndex, otherIndex);
            while (orderedIndex < orderedLivingNotes.Count)
            {
                NoteModel orderedNote = orderedLivingNotes[orderedIndex];
                otherIndex = otherLivingNotes.IndexOfById(orderedNote.Id);
                result.Add(new Tuple<int, int>(orderedIndex, otherIndex));

                orderedIndex++;
                otherIndex++;
                AddFollowingLonelyNotesToMap(result, orderedLivingNotes, otherLivingNotes, ref orderedIndex, otherIndex);
            }
            return result;
        }

        /// <summary>
        /// Helper function to <see cref="BuildMapOfLivingNotes(NoteListModel, NoteListModel)"/>.
        /// Searches forward in both repositories for notes which do not have a corresponding
        /// note in the other repository (lonely notes). Found loners are added to the map,
        /// to keep their relative position.
        /// </summary>
        /// <param name="map">Found notes are added to this map.</param>
        /// <param name="orderedNotes">Repository whose order is more recent.</param>
        /// <param name="otherNotes">Repository whose order is less important.</param>
        /// <param name="orderedIndex">Position in ordered notes repository.</param>
        /// <param name="otherIndex">Positoin in other notes repository.</param>
        private static void AddFollowingLonelyNotesToMap(Map map, NoteListModel orderedNotes, NoteListModel otherNotes, ref int orderedIndex, int otherIndex)
        {
            // Check for lonely notes in ordered repo
            bool pairFound = false;
            while (!pairFound && (orderedIndex < orderedNotes.Count))
            {
                NoteModel orderedNote = orderedNotes[orderedIndex];
                pairFound = otherNotes.ContainsById(orderedNote.Id);
                if (!pairFound)
                {
                    map.Add(new Tuple<int, int>(orderedIndex, -1));
                    orderedIndex++;
                }
            }

            // Check for lonely notes in other repo
            pairFound = false;
            while (!pairFound && (otherIndex < otherNotes.Count))
            {
                NoteModel otherNote = otherNotes[otherIndex];
                pairFound = orderedNotes.ContainsById(otherNote.Id);
                if (!pairFound)
                {
                    map.Add(new Tuple<int, int>(-1, otherIndex));
                    otherIndex++;
                }
            }
        }

        /// <summary>
        /// Build a combined list of notes, using the map.
        /// </summary>
        /// <param name="orderedNotes">Same ordered repository used to build the map.</param>
        /// <param name="otherNotes">Same other repository used to build the map.</param>
        /// <param name="map">Map with corresponding indexes of the notes of both repositories.</param>
        /// <returns>List of notes for the new merged repository.</returns>
        private static NoteListModel CreateNoteList(NoteListModel orderedNotes, NoteListModel otherNotes, Map map)
        {
            NoteListModel result = new NoteListModel();

            foreach (var pair in map)
            {
                if (pair.Item1 == -1)
                {
                    // Only available in otherNotes
                    NoteModel otherNote = otherNotes[pair.Item2];
                    result.Add(otherNote.Clone());
                }
                else if (pair.Item2 == -1)
                {
                    // Only available in orderedNotes
                    NoteModel orderedNote = orderedNotes[pair.Item1];
                    result.Add(orderedNote.Clone());
                }
                else
                {
                    // Take the more recent note
                    NoteModel orderedNote = orderedNotes[pair.Item1];
                    NoteModel otherNote = otherNotes[pair.Item2];
                    NoteModel newNote;
                    if (orderedNote.ModifiedAt >= otherNote.ModifiedAt)
                        newNote = orderedNote.Clone();
                    else
                        newNote = otherNote.Clone();
                    result.Add(newNote);
                }
            }
            return result;
        }

        private class Map : List<Tuple<int, int>>
        {
        }
    }
}
