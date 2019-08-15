// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;

namespace SilentNotes.StoryBoards
{
    /// <summary>
    /// Base class for all story board steps.
    /// </summary>
    public abstract class StoryBoardStepBase : IStoryBoardStep
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StoryBoardStepBase"/> class.
        /// </summary>
        /// <param name="stepId">Id of this step.</param>
        /// <param name="storyBoard">The story board which owns the step.</param>
        protected StoryBoardStepBase(int stepId, IStoryBoard storyBoard)
        {
            Id = stepId;
            StoryBoard = storyBoard;
        }

        /// <inheritdoc/>
        public int Id { get; private set; }

        /// <inheritdoc/>
        public abstract Task Run();

        /// <summary>
        /// Gets the story board which owns this step.
        /// </summary>
        protected IStoryBoard StoryBoard { get; private set; }
    }
}
