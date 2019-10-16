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
        /// Adds user input, required by the story.
        /// </summary>
        /// <param name="key">The name of the user input.</param>
        /// <param name="value">The user input.</param>
        void StoreToSession(int key, object value);

        /// <summary>
        /// Removes user input from the session. If no such user input can be found, nothing happens.
        /// </summary>
        /// <param name="key">The name of the user input.</param>
        void RemoveFromSession(int key);

        /// <summary>
        /// Tries to find a user input object, previously stored with <see cref="StoreToSession(int,object)"/>.
        /// </summary>
        /// <typeparam name="T">Expected type of user input.</typeparam>
        /// <param name="key">The name of the user input.</param>
        /// <param name="value">The stored user input, or a default value in case the user input
        /// was not stored.</param>
        /// <returns>Returns true if the user input was found and is of the expected type,
        /// otherwise false.</returns>
        bool TryLoadFromSession<T>(int key, out T value);

        /// <summary>
        /// Loads a user input object, previously stored with <see cref="StoreToSession(int,object)"/>.
        /// </summary>
        /// <typeparam name="T">Expected type of user input.</typeparam>
        /// <param name="key">The name of the user input.</param>
        /// <returns>The stored user input.</returns>
        /// <exception cref="ArgumentOutOfRangeException">This exception is thrown if no such
        /// object is stored in the session.</exception>
        T LoadFromSession<T>(int key);

        /// <summary>
        /// Can be called if the story ended, to clean up the session variables.
        /// </summary>
        void ClearSession();
    }
}
