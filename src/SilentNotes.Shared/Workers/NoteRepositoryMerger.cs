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
        /// not used by any note.</param>
        /// <returns>The merged repository.</returns>
        public NoteRepositoryModel Merge(NoteRepositoryModel localRepository, NoteRepositoryModel remoteRepository)
        {
            if (localRepository == null)
                throw new ArgumentNullException(nameof(localRepository));
            if (remoteRepository == null)
                throw new ArgumentNullException(nameof(remoteRepository));
            localRepository.ClearMaintainedAtIfObsolete();
            remoteRepository.ClearMaintainedAtIfObsolete();

            List<Guid> deletedNotes = BuildMergedListOfDeletedNotes(localRepository, remoteRepository);
            deletedNotes.Sort(); // to allow binary search
            NoteListModel localLivingNotes = BuildListOfLivingNotes(localRepository, deletedNotes);
            NoteListModel remoteLivingNotes = BuildListOfLivingNotes(remoteRepository, deletedNotes);

            // Create new merge repository
            NoteRepositoryModel result = new NoteRepositoryModel();
            result.Revision = NoteRepositoryModel.NewestSupportedRevision;
            result.Id = remoteRepository.Id;
            result.DeletedNotes = deletedNotes;
            result.Safes = BuildMergedListOfSafes(remoteRepository.Safes, localRepository.Safes);

            bool orderInLocalRepo = localRepository.OrderModifiedAt > remoteRepository.OrderModifiedAt;
            if (orderInLocalRepo)
            {
                result.Notes = BuildMergedListOfNotes(localLivingNotes, remoteLivingNotes);
                result.OrderModifiedAt = localRepository.OrderModifiedAt;
            }
            else
            {
                result.Notes = BuildMergedListOfNotes(remoteLivingNotes, localLivingNotes);
                result.OrderModifiedAt = remoteRepository.OrderModifiedAt;
            }

            result.RemoveUnusedSafes();
            result.ClearMaintainedAtIfObsolete();
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

            // Add note ids from the local repository whose notes exist in the remote repository.
            // As long as they are not part of the remote repository, they existed only locally,
            // where deleted, and we can forget about them.
            foreach (Guid locallyDeletedNoteId in localRepository.DeletedNotes)
            {
                bool noteStillExistsInRemoteRepo = remoteRepository.Notes.ContainsById(locallyDeletedNoteId);
                if (noteStillExistsInRemoteRepo && !result.Contains(locallyDeletedNoteId))
                {
                    result.Add(locallyDeletedNoteId);
                }
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
        /// Build a merged list of notes.
        /// </summary>
        /// <param name="leftItems">List of notes from the left repository, whose order has precedence.</param>
        /// <param name="rightItems">List of notes from the right side repository.</param>
        /// <returns>List of notes for the new merged repository.</returns>
        private static NoteListModel BuildMergedListOfNotes(NoteListModel leftItems, NoteListModel rightItems)
        {
            NoteListModel result = new NoteListModel();

            var map = OuterJoin(leftItems, rightItems, item => item.Id);
            foreach (Tuple<NoteModel, NoteModel> pair in map)
            {
                if (pair.Item1 == null)
                {
                    // Only available on the right side
                    result.Add(pair.Item2.Clone());
                }
                else if (pair.Item2 == null)
                {
                    // Only available on the left side
                    result.Add(pair.Item1.Clone());
                }
                else
                {
                    // Take the more recent
                    NoteModel lastModifiedItem = ChooseLastModified(
                        pair.Item1, pair.Item2, item => item.ModifiedAt, item => item.MaintainedAt);
                    result.Add(lastModifiedItem.Clone());
                }
            }
            return result;
        }

        /// <summary>
        /// Build a merged list of safes.
        /// </summary>
        /// <param name="leftItems">List of safes from the left repository, whose order has precedence.</param>
        /// <param name="rightItems">List of safes from the right side repository.</param>
        /// <returns>List of safes for the new merged repository.</returns>
        private static SafeListModel BuildMergedListOfSafes(SafeListModel leftItems, SafeListModel rightItems)
        {
            SafeListModel result = new SafeListModel();

            var map = OuterJoin(leftItems, rightItems, item => item.Id);
            foreach (Tuple<SafeModel, SafeModel> pair in map)
            {
                if (pair.Item1 == null)
                {
                    // Only available on the right side
                    result.Add(pair.Item2.Clone());
                }
                else if (pair.Item2 == null)
                {
                    // Only available on the left side
                    result.Add(pair.Item1.Clone());
                }
                else
                {
                    // Take the more recent
                    SafeModel lastModifiedItem = ChooseLastModified(
                        pair.Item1, pair.Item2, item => item.ModifiedAt, item => item.MaintainedAt);
                    result.Add(lastModifiedItem.Clone());
                }
            }
            return result;
        }

        /// <summary>
        /// Builds a relation map between items from <paramref name="leftItems"/> and <paramref name="rightItems"/>.
        /// The pairs of the map can contain a null value, if the item exists only on one of the sides.
        /// The order of the items is preserved, the order of the left side has precedence.
        /// </summary>
        /// <typeparam name="TItem">Type of the list items.</typeparam>
        /// <typeparam name="TKey">Type of the key which is used to find corresponding pairs.</typeparam>
        /// <param name="leftItems">Left list, whose items are joined.</param>
        /// <param name="rightItems">Right list, whose items are joined.</param>
        /// <param name="keySelector">A function which gets the key from an item.</param>
        /// <returns>Map with pairs of matching items from the left and from the right side.</returns>
        private static List<Tuple<TItem, TItem>> OuterJoin<TItem, TKey>(
            IList<TItem> leftItems,
            IList<TItem> rightItems,
            Func<TItem, TKey> keySelector) where TItem : class
        {
            List<Tuple<TItem, TItem>> map = new List<Tuple<TItem, TItem>>();

            // Prepare dictionaries for fast access by key
            Dictionary<TKey, int> leftKeyToPos = new Dictionary<TKey, int>();
            Dictionary<TKey, int> rightKeyToPos = new Dictionary<TKey, int>();
            for (int index = 0; index < leftItems.Count; index++)
                leftKeyToPos.Add(keySelector(leftItems[index]), index);
            for (int index = 0; index < rightItems.Count; index++)
                rightKeyToPos.Add(keySelector(rightItems[index]), index);

            int leftPos = 0;
            int rightPos = 0;
            AddAdjacentSingles(map, leftItems, ref leftPos, true, keySelector, rightKeyToPos);
            AddAdjacentSingles(map, rightItems, ref rightPos, false, keySelector, leftKeyToPos);

            while (leftPos < leftItems.Count)
            {
                TItem leftItem = leftItems[leftPos];
                rightPos = rightKeyToPos[keySelector(leftItem)];
                map.Add(new Tuple<TItem, TItem>(leftItem, rightItems[rightPos]));

                leftPos++;
                rightPos++;
                AddAdjacentSingles(map, leftItems, ref leftPos, true, keySelector, rightKeyToPos);
                AddAdjacentSingles(map, rightItems, ref rightPos, false, keySelector, leftKeyToPos);
            }
            return map;
        }

        /// <summary>
        /// Helper function to <see cref="OuterJoin{TItem, TKey}(IList{TItem}, IList{TItem}, Func{TItem, TKey})"/>.
        /// Searches for items without a corresponding partner, starting from a given position.
        /// As long as singles are found, they are added to the map and the pos is increased.
        /// </summary>
        /// <typeparam name="TItem">Type of the list items.</typeparam>
        /// <typeparam name="TKey">Type of the key which is used to find corresponding pairs.</typeparam>
        /// <param name="map">Map to add found singles to.</param>
        /// <param name="candidates">List with items to search.</param>
        /// <param name="pos">Start the search with the item at this index. This parameter will be
        /// increased by each found single item.</param>
        /// <param name="fromLeftSide">True if the candidates are from the left side.</param>
        /// <param name="keySelector">A function which gets the key from an item.</param>
        /// <param name="partnerKeys">Dictionary containing the keys from the other list.</param>
        private static void AddAdjacentSingles<TItem, TKey>(
            List<Tuple<TItem, TItem>> map,
            IList<TItem> candidates,
            ref int pos,
            bool fromLeftSide,
            Func<TItem, TKey> keySelector,
            Dictionary<TKey, int> partnerKeys) where TItem : class
        {
            bool isSingle = true;
            while (isSingle && (pos < candidates.Count))
            {
                TItem candidate = candidates[pos];
                isSingle = !partnerKeys.ContainsKey(keySelector(candidate));
                if (isSingle)
                {
                    if (fromLeftSide)
                        map.Add(new Tuple<TItem, TItem>(candidate, null));
                    else
                        map.Add(new Tuple<TItem, TItem>(null, candidate));
                    pos++;
                }
            }
        }

        /// <summary>
        /// Chooses the more recent item of two. Both items must have a ModifiedAt and a MaintainedAt
        /// DateTime value.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="item1">The first item to compare.</param>
        /// <param name="item2">The second item to compare</param>
        /// <param name="modifiedAtSelector">A function which gets the ModifiedAt from an item.</param>
        /// <param name="maintainedAtSelector">A function which gets the MaintainedAt from an item.</param>
        /// <returns>The more recent item which should be written to a new merged repository.</returns>
        internal static TItem ChooseLastModified<TItem>(
            TItem item1,
            TItem item2,
            Func<TItem, DateTime> modifiedAtSelector,
            Func<TItem, DateTime?> maintainedAtSelector)
        {
            int comparisonResult = DateTime.Compare(modifiedAtSelector(item1), modifiedAtSelector(item2));

            // If both are modified at the same time, maybe the system tried to maintain the note.
            if (comparisonResult == 0)
                comparisonResult = Nullable.Compare(maintainedAtSelector(item1), maintainedAtSelector(item2));

            return (comparisonResult >= 0) ? item1 : item2;
        }
    }
}
