using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SilentNotes.ViewModels;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Helper class to move notes to a new position.
    /// </summary>
    public static class NoteMover
    {
        /// <summary>
        /// Determines the new positions, a note gets when moving upwards or downwards.
        /// </summary>
        /// <param name="allNotes">List containing all notes.</param>
        /// <param name="filteredNotes">List containing only filtered notes, a subset of <paramref name="allNotes"/>.</param>
        /// <param name="selectedNote">The selected note to move.</param>
        /// <param name="upwards">Value indicating whether the note should be moved upwards (true)
        /// or downwards (false).</param>
        /// <param name="singleStep">If true it determines the position next to the current position,
        /// otherwise it determines the position at the begin/end of the collection.</param>
        /// <returns>An object holding the determined positions.</returns>
        public static NotePositions GetNotePositions(
            IList<NoteViewModel> allNotes,
            IList<NoteViewModel> filteredNotes,
            NoteViewModel selectedNote,
            bool upwards,
            bool singleStep)
        {
            if ((selectedNote == null) || (filteredNotes.Count < 2))
                return null;

            int oldIndexInUnfilteredList = allNotes.IndexOf(selectedNote);
            int oldIndexInFilteredList = filteredNotes.IndexOf(selectedNote);
            int newIndexInUnfilteredList = oldIndexInUnfilteredList;
            int newIndexInFilteredList = oldIndexInFilteredList;

            if (singleStep)
            {
                // move one step.
                int step = upwards ? -1 : +1;
                if (!IsInRange(oldIndexInFilteredList + step, 0, filteredNotes.Count - 1))
                    return null;

                newIndexInFilteredList = oldIndexInFilteredList + step;
                newIndexInUnfilteredList = allNotes.IndexOf(filteredNotes[newIndexInFilteredList]);
            }
            else
            {
                if (upwards)
                {
                    // upwards, go to the top of the visible list.
                    newIndexInUnfilteredList = allNotes.IndexOf(filteredNotes.First());
                    newIndexInFilteredList = 0;
                }
                else
                {
                    // downwards, go to the end of the visible list.
                    newIndexInUnfilteredList = allNotes.IndexOf(filteredNotes.Last());
                    newIndexInFilteredList = filteredNotes.Count - 1;
                }
            }

            if ((oldIndexInUnfilteredList == newIndexInUnfilteredList)
                || !IsInRange(newIndexInUnfilteredList, 0, allNotes.Count - 1))
                return null;

            return new NotePositions 
            {
                OldAllNotesPos = oldIndexInUnfilteredList,
                OldFilteredNotesPos = oldIndexInFilteredList,
                NewAllNotesPos = newIndexInUnfilteredList,
                NewFilteredNotesPos = newIndexInFilteredList,
            };
        }

        /// <summary>
        /// Moves an item in a list to a new position.
        /// </summary>
        /// <typeparam name="T">Type of list items.</typeparam>
        /// <param name="list">The list to reorder.</param>
        /// <param name="oldIndex">Move the item at this position.</param>
        /// <param name="newIndex">Move the item to this new position.</param>
        public static void ListMove<T>(List<T> list, int oldIndex, int newIndex)
        {
            T item = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, item);
        }

        public static void AdjustPinStatusAfterMoving(IList<NoteViewModel> allNotes, int newIndex)
        {
            NoteViewModel note = allNotes[newIndex];
            if (note.IsPinned)
            {
                // Check if previous item is not pinned and if so adjust note to close the gap
                NoteViewModel previousNote = allNotes.ElementAtOrDefault(newIndex - 1);
                if ((previousNote != null) && (!previousNote.IsPinned))
                    note.IsPinned = false;
            }
            else
            {
                // Check if next item is pinned and if so adjust note to close the gap
                NoteViewModel nextNote = allNotes.ElementAtOrDefault(newIndex + 1);
                if ((nextNote != null) && (nextNote.IsPinned))
                    note.IsPinned = true;
            }
        }

        private static bool IsInRange(int candidate, int min, int max)
        {
            return (candidate >= min) && (candidate <= max);
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate,
        /// and returns the zero-based index of the first occurrence within the entire <see cref="Collection{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">The collection to search.</param>
        /// <param name="match">The delegate that defines the conditions of the element to search for.</param>
        /// <returns>The zero-based index of the first occurrence of an element that matches the
        /// conditions defined by <paramref name="match"/>, if found; otherwise, -1.</returns>
        private static int FindIndex<T>(this IList<T> collection, Predicate<T> match)
        {
            for (int index = 0; index < collection.Count; index++)
            {
                T item = collection[index];
                if (match(item))
                    return index;
            }
            return -1;
        }

        public class NotePositions
        {
            public int OldAllNotesPos { get; set; }

            public int OldFilteredNotesPos { get; set; }

            public int NewAllNotesPos { get; set; }

            public int NewFilteredNotesPos { get; set; }
        }
    }
}
