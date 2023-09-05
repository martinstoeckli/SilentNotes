// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.Stories.SynchronizationStory;

namespace SilentNotes.Services
{
    /// <summary>
    /// Container to access the current story board.
    /// </summary>
    public interface IStoryBoardService
    {
        /// <summary>
        /// Gets or sets the active synchronization story, or null if no story is active.
        /// </summary>
        SynchronizationStoryModel SynchronizationStory { get; set;}
    }
}
