// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;

namespace SilentNotes.StoryBoards
{
    /// <summary>
    /// Storyboards describe long-term actions with several steps involved.
    /// Storyboards can span over multiple user dialogs, can run non linear making decisions, and
    /// can be canceled by the user.
    /// </summary>
    public interface IStoryBoard
    {
        /// <summary>
        /// Registers a single step in the story board. Registration of all steps is usually done
        /// in the constructor of the story.
        /// </summary>
        /// <param name="step">Step to register.</param>
        void RegisterStep(IStoryBoardStep step);

        /// <summary>
        /// Gets a value indicating, whether the story runs silently in the background. If this
        /// value is true, no GUI should be involved and missing information should stop the story.
        /// An example for this mode could be a data synchronization at startup/shutdown.
        /// </summary>
        StoryBoardMode Mode { get; }

        /// <summary>
        /// Starts the story at the first step, which was registered with <see cref="RegisterStep(IStoryBoardStep)"/>.
        /// </summary>
        /// <returns>An asynchronous task.</returns>
        Task Start();

        /// <summary>
        /// Continues with the next step of the story. The story continues until a new user
        /// interaction is necessary, or to the end.
        /// </summary>
        /// <param name="stepId">The next expected step. Passing the expected step allows to
        /// go forwards and backwards in the story.</param>
        /// <returns>An asynchronous task.</returns>
        Task ContinueWith(Enum stepId);

        /// <summary>
        /// Gets the session of the story board, where data can be passed from step to step.
        /// </summary>
        IStoryBoardSession Session { get; }
    }
}
