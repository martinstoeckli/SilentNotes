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
        public async ValueTask RunStory(TModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            var stepResult = await RunStep(model, serviceProvider, uiMode);
            if (stepResult == null)
                return;

            // Show feedback of this step if not in silent mode
            if (uiMode != StoryMode.Silent)
                await ShowFeedback(stepResult, serviceProvider);

            // Continue with next step if available
            if (stepResult?.NextStep != null)
                await stepResult.NextStep.RunStory(model, serviceProvider, uiMode);
        }

        /// <inheritdoc/>
        public abstract ValueTask<StoryStepResult<TModel>> RunStep(TModel model, IServiceProvider serviceProvider, StoryMode uiMode);

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
        /// <param name="serviceProvider">Service provider to get the feedback service from.</param>
        /// <returns>Task for async calling.</returns>
        protected async ValueTask ShowFeedback(StoryStepResult<TModel> stepResult, IServiceProvider serviceProvider)
        {
            if (!stepResult.HasError && !stepResult.HasMessage && !stepResult.HasToast)
                return;

            IFeedbackService feedbackService = serviceProvider.GetService<IFeedbackService>();

            if (stepResult.HasError)
            {
                string errorMessage = TranslateException(stepResult.Error, serviceProvider);
                if (!string.IsNullOrEmpty(errorMessage))
                    feedbackService.ShowToast(errorMessage);
                return;
            }

            if (stepResult.HasMessage)
            {
                if (feedbackService != null)
                    await feedbackService.ShowMessageAsync(stepResult.Message, null, MessageBoxButtons.Ok, false);
            }

            if (stepResult.HasToast)
            {
                if (feedbackService != null)
                    feedbackService.ShowToast(stepResult.Toast);
            }
        }

        /// <summary>
        /// Helper function to create the return value of a running step/story.
        /// </summary>
        /// <param name="nextStep">Next step, or null if the story ends.</param>
        /// <returns>Task with the story result.</returns>
        protected ValueTask<StoryStepResult<TModel>> FromResult(StoryStepBase<TModel> nextStep)
        {
            return ValueTask.FromResult<StoryStepResult<TModel>>(new StoryStepResult<TModel>(nextStep));
        }

        /// <summary>
        /// Helper function to create the return value of a finished story.
        /// </summary>
        /// <returns>Task with the story result.</returns>
        protected ValueTask<StoryStepResult<TModel>> FromResultEndOfStory()
        {
            return FromResult(null);
        }
    }
}
