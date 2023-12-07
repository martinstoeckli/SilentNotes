// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Services;

namespace SilentNotes.Stories
{
    /// <summary>
    /// Base class for all implementations of <see cref="IStoryStep{TModel}"/>.
    /// </summary>
    /// <typeparam name="TModel">The class which represents the model which takes the collected
    /// informations of the story.</typeparam>
    public abstract class StoryStepBase<TModel> : IStoryStep<TModel> where TModel : class
    {
        /// <inheritdoc/>
        public async Task RunStory(TModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            var stepResult = await RunStep(model, serviceProvider, uiMode);
            if (stepResult == null)
                return;

            // Show feedback of this step if not in silent mode
            if (stepResult.HasFeedback)
                await ShowFeedback(stepResult, uiMode, serviceProvider);

            // Continue with next step if available
            if (stepResult.NextStep != null)
                await stepResult.NextStep.RunStory(model, serviceProvider, uiMode);
        }

        /// <inheritdoc/>
        public abstract Task<StoryStepResult<TModel>> RunStep(TModel model, IServiceProvider serviceProvider, StoryMode uiMode);

        /// <summary>
        /// Takes an exception and gets a error message which can be presented to the user.
        /// </summary>
        /// <param name="ex">The exception to translate.</param>
        /// <param name="serviceProvider">The service provider which can be used to get the
        /// language service.</param>
        /// <returns>Localized error message.</returns>
        protected abstract string TranslateException(Exception ex, IServiceProvider serviceProvider);

        /// <summary>
        /// Shows messages or toasts if necessary. This function is called automatically by
        /// <see cref="RunStory(TModel, IServiceProvider, StoryMode)"/>.
        /// </summary>
        /// <param name="stepResult">The step result, which may contain a message to show.</param>
        /// <param name="uiMode">Defines what UI feedback is given.</param>
        /// <param name="serviceProvider">Service provider to get the feedback service from.</param>
        /// <returns>Task for async calling.</returns>
        public async Task ShowFeedback(StoryStepResult<TModel> stepResult, StoryMode uiMode, IServiceProvider serviceProvider)
        {
            IFeedbackService feedbackService = serviceProvider.GetService<IFeedbackService>();

            if (stepResult.HasError && uiMode.HasFlag(StoryMode.Toasts))
            {
                string errorMessage = TranslateException(stepResult.Error, serviceProvider);
                if (!string.IsNullOrEmpty(errorMessage))
                    feedbackService.ShowToast(errorMessage);
                return;
            }

            if (stepResult.HasMessage && uiMode.HasFlag(StoryMode.Messages))
            {
                if (feedbackService != null)
                    await feedbackService.ShowMessageAsync(stepResult.Message, null, MessageBoxButtons.Ok, false);
            }

            if (stepResult.HasToast && uiMode.HasFlag(StoryMode.Toasts))
            {
                if (feedbackService != null)
                    feedbackService.ShowToast(stepResult.Toast);
            }
        }

        /// <summary>
        /// Simplifies creation of a story step result.
        /// </summary>
        /// <param name="nextStep">Sets the <see cref="StoryStepResult{TModel}.NextStep"/> property.</param>
        /// <param name="toast">Sets the <see cref="StoryStepResult{TModel}.Toast"/> property.</param>
        /// <param name="message">Sets the <see cref="StoryStepResult{TModel}.Message"/> property.</param>
        /// <returns>New instance of a story step result.</returns>
        protected static StoryStepResult<TModel> ToResult(StoryStepBase<TModel> nextStep, string toast = null, string message = null)
        {
            return new StoryStepResult<TModel>(nextStep, toast, message);
        }

        /// <summary>
        /// Simplifies creation of a story step result with no next step.
        /// </summary>
        /// <param name="toast">Sets the <see cref="StoryStepResult{TModel}.Toast"/> property.</param>
        /// <param name="message">Sets the <see cref="StoryStepResult{TModel}.Message"/> property.</param>
        /// <returns>New instance of a story step result.</returns>
        protected static StoryStepResult<TModel> ToResultEndOfStory(string toast = null, string message = null)
        {
            return new StoryStepResult<TModel>(null, toast, message);
        }

        /// <summary>
        /// Simplifies creation of a story step result with an exception.
        /// </summary>
        /// <param name="error">Sets the <see cref="StoryStepResult{TModel}.Error"/> property.</param>
        /// <returns>New instance of a story step result.</returns>
        protected static StoryStepResult<TModel> ToResult(Exception error)
        {
            return new StoryStepResult<TModel>(error);
        }

        /// <summary>
        /// Encloses a story step result in a Task so functions without async can return a task.
        /// </summary>
        /// <param name="storyStepResult">The story step to enclose.</param>
        /// <returns>Task containing the story step result.</returns>
        protected static Task<StoryStepResult<TModel>> ToTask(StoryStepResult<TModel> storyStepResult)
        {
            return Task.FromResult<StoryStepResult<TModel>>(storyStepResult);
        }
    }
}
