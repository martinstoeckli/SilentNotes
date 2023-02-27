// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.StoryBoards
{
    /// <summary>
    /// Session of a story board, which can pass data from one story step to another story step.
    /// </summary>
    public interface IStoryBoardSession
    {
        /// <summary>
        /// Adds user input, required by the story.
        /// </summary>
        /// <param name="key">The name of the user input.</param>
        /// <param name="value">The user input.</param>
        void Store(Enum key, object value);

        /// <summary>
        /// Removes user input from the session. If no such user input can be found, nothing happens.
        /// </summary>
        /// <param name="key">The name of the user input.</param>
        void Remove(Enum key);

        /// <summary>
        /// Tries to find a user input object, previously stored with <see cref="StoreToSession(int,object)"/>.
        /// </summary>
        /// <typeparam name="T">Expected type of user input.</typeparam>
        /// <param name="key">The name of the user input.</param>
        /// <param name="value">The stored user input, or a default value in case the user input
        /// was not stored.</param>
        /// <returns>Returns true if the user input was found and is of the expected type,
        /// otherwise false.</returns>
        bool TryLoad<T>(Enum key, out T value);

        /// <summary>
        /// Loads a user input object, previously stored with <see cref="StoreToSession(int,object)"/>.
        /// </summary>
        /// <typeparam name="T">Expected type of user input.</typeparam>
        /// <param name="key">The name of the user input.</param>
        /// <returns>The stored user input.</returns>
        /// <exception cref="ArgumentOutOfRangeException">This exception is thrown if no such
        /// object is stored in the session.</exception>
        T Load<T>(Enum key);

        /// <summary>
        /// Can be called if the story ended, to clean up the session variables.
        /// </summary>
        void Clear();
    }
}
