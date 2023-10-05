// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Stories
{
    /// <summary>
    /// Describes the result of a single step of story.
    /// </summary>
    public class StoryStepResult<TModel> where TModel : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StoryStepResult{TModel}"/> class.
        /// </summary>
        /// <param name="nextStep">Sets the <see cref="NextStep"/> property.</param>
        /// <param name="toast">Sets the <see cref="Toast"/> property.</param>
        /// <param name="message">Sets the <see cref="Message"/> property.</param>
        public StoryStepResult(IStoryStep<TModel> nextStep, string toast = null, string message = null)
            : this(nextStep, toast, message, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoryStepResult{TModel}"/> class.
        /// </summary>
        /// <param name="error">Sets the <see cref="Error"/> property.</param>
        public StoryStepResult(Exception error)
            : this(null, null, null, error)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StoryStepResult{TModel}"/> class.
        /// </summary>
        /// <param name="nextStep">Sets the <see cref="NextStep"/> property.</param>
        /// <param name="toast">Sets the <see cref="Toast"/> property.</param>
        /// <param name="message">Sets the <see cref="Message"/> property.</param>
        /// <param name="error">Sets the <see cref="Error"/> property.</param>
        private StoryStepResult(IStoryStep<TModel> nextStep, string toast, string message, Exception error)
        {
            NextStep = nextStep;
            Toast = toast;
            Message = message;
            Error = error;
        }

        /// <summary>
        /// Gets the next step to perform. This value can be null if the story has been finished
        /// and no next step should be executed.
        /// </summary>
        public IStoryStep<TModel> NextStep { get; }

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
        /// Returns a value indicating whether any of <see cref="Error"/>, <see cref="Message"/>
        /// or <see cref="Toast"/> are set.
        /// </summary>
        public bool HasFeedback
        {
            get { return HasError || HasMessage || HasToast; }
        }
    }
}
