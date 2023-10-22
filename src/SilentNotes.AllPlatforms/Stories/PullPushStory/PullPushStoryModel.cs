// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using SilentNotes.Stories.SynchronizationStory;

namespace SilentNotes.Stories.PullPushStory
{
    internal class PullPushStoryModel : SynchronizationStoryModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullPushStoryModel"/> class.
        /// </summary>
        /// <param name="noteId">Sets the <see cref="NoteId"/> property.</param>
        /// <param name="direction">Sets the <see cref="Direction"/> property.</param>
        public PullPushStoryModel(Guid noteId, PullPushDirection direction)
        {
            NoteId = noteId;
            Direction = direction;
        }

        /// <summary>
        /// Gets the id of the note which should be pushed or pulled.
        /// </summary>
        public Guid NoteId { get; }

        /// <summary>
        /// Gets the direction, the indicator whether the note should be pulled or pushed.
        /// </summary>
        public PullPushDirection Direction { get; }
    }

    /// <summary>
    /// Enumeration of the direction of the synchronization, either from online storage to local
    /// or reverse.
    /// </summary>
    internal enum PullPushDirection
    {
        /// <summary>Pull the note from the server and overwrite the local note</summary>
        PullFromServer,

        /// <summary>Use the local note and overwrite the note on the server</summary>
        PushToServer
    }
}
