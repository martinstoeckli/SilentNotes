// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;

namespace SilentNotes.StoryBoards
{
    /// <summary>
    /// Describes a single step of a story board.
    /// </summary>
    public interface IStoryBoardStep
    {
        /// <summary>
        /// Gets the id/name of this step.
        /// </summary>
        int Id { get; }

        /// <summary>
        /// Executes the step.
        /// </summary>
        /// <returns>Task to be called async.</returns>
        Task Run();
    }
}
