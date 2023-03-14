// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.StoryBoards
{
    /// <summary>
    /// Describes the result of a single step of the <see cref="IStoryBoard"/>.
    /// </summary>
    public class StoryBoardStepResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StepResult"/> class.
        /// </summary>
        /// <param name="nextStepId">Sets the <see cref="NextStepId"/> property.</param>
        /// <param name="toast">Sets the <see cref="Toast"/> property.</param>
        /// <param name="message">Sets the <see cref="Message"/> property.</param>
        public StoryBoardStepResult(Enum nextStepId, string toast = null, string message = null)
        {
            NextStepId = nextStepId;
            Toast = toast;
            Message = message;
            Error = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StepResult"/> class.
        /// </summary>
        /// <param name="error">Sets the <see cref="Error"/> property.</param>
        public StoryBoardStepResult(Exception error)
        {
            NextStepId = null;
            Toast = null;
            Message = null;
            Error = error;
        }

        /// <summary>
        /// Gets the id of the next step to perform. This value can be null if no next step
        /// should be executed.
        /// </summary>
        public Enum NextStepId { get; }

        /// <summary>
        /// Gets a text which can be displayed as toast message. The caller can decide
        /// whether the message is shown, depending of running in silent mode. This can be null.
        /// </summary>
        public string Toast { get; }

        /// <summary>
        /// Gets a text which can be displayed in a message dialog. The caller can decide
        /// whether the message is shown, depending of running in silent mode. This can be null.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets an exception which can be displayed as error message. The caller decides
        /// whether the message is shown, depending of running in silent mode. This can be null.
        /// </summary>
        public Exception Error { get; }

        /// <summary>
        /// Returns a value indicating whether the <see cref="NextStepId"/> is set (not null).
        /// </summary>
        public bool HasNextStep
        {
            get { return NextStepId != null; }
        }

        /// <summary>
        /// Returns a value indicating whether the <see cref="Toast"/> is set (not null or empty).
        /// </summary>
        public bool HasToast
        {
            get { return !string.IsNullOrEmpty(Toast); }
        }

        /// <summary>
        /// Returns a value indicating whether the <see cref="Message"/> is set (not null or empty).
        /// </summary>
        public bool HasMessage
        {
            get { return !string.IsNullOrEmpty(Message); }
        }

        /// <summary>
        /// Returns a value indicating whether the <see cref="Error"/> is set (not null).
        /// </summary>
        public bool HasError
        {
            get { return Error != null; }
        }

        /// <summary>
        /// Checks whether the <see cref="NextStepId"/> is equal to the <paramref name="otherStepId"/>.
        /// </summary>
        /// <param name="otherStepId">The value of the enum to compare with.</param>
        /// <returns>Returns true if the enum values are equal or if both are null, otherwise false.</returns>
        public bool NextStepIs(Enum otherStepId)
        {
            return Enum.Equals(NextStepId, otherStepId);
        }
    }
}
