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
        private static readonly Task _completedTask = Task.FromResult(0);

        /// <summary>
        /// Initializes a new instance of the <see cref="StoryBoardStepBase"/> class.
        /// </summary>
        /// <param name="stepId">Id of this step.</param>
        /// <param name="storyBoard">The story board which owns the step.</param>
        public StoryBoardStepBase(int stepId, IStoryBoard storyBoard)
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

        /// <summary>
        /// Gets a dummy task, which signals to be finished already. It can be used as the result
        /// of a method which has a task as result but doesn't use it (e.g. implementation of an
        /// async interface).
        /// </summary>
        /// <returns>Already completed task.</returns>
        protected static Task GetCompletedDummyTask()
        {
            return _completedTask;
        }
    }
}
