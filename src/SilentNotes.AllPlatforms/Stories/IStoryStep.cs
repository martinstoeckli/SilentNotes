// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;

namespace SilentNotes.Stories
{
    /// <summary>
    /// A single step out of a story. A story is a series of steps and can have multiple endings.
    /// </summary>
    /// <remarks>
    /// It should be possible to end/continue a story at any time, even if the app is closed and
    /// restartet in the meantime, thus the story steps are meant to be throw-away objects, while
    /// the model collects all necessary information and can be persisted.
    /// </remarks>
    /// <typeparam name="TModel"></typeparam>
    public interface IStoryStep<TModel> where TModel : class
    {
        /// <summary>
        /// Runs the story step and continues with the next steps, until the story ends or is
        /// halted to show a user interface. Afterwards it shows the last toast, message or error
        /// if available.
        /// </summary>
        /// <param name="model">The model to persist collected information.</param>
        /// <param name="serviceProvider">The service provider. Do not rely on the services to be
        /// the same instances for the whole story.</param>
        /// <param name="uiMode">Can be used to run in silent mode.</param>
        /// <returns></returns>
        Task RunStoryAndShowLastFeedback(TModel model, IServiceProvider serviceProvider, StoryMode uiMode);

        /// <summary>
        /// Runs the story step and continues with the next steps, until the story ends or is
        /// halted to show a user interface.
        /// </summary>
        /// <param name="model">The model to persist collected information.</param>
        /// <param name="serviceProvider">The service provider. Do not rely on the services to be
        /// the same instances for the whole story.</param>
        /// <param name="uiMode">Can be used to run in silent mode.</param>
        /// <returns>A list the results of all steps.</returns>
        Task<List<StoryStepResult<TModel>>> RunStory(TModel model, IServiceProvider serviceProvider, StoryMode uiMode);

        /// <summary>
        /// Runs this single step and returns its result.
        /// </summary>
        /// <param name="model">The model to persist collected information.</param>
        /// <param name="serviceProvider">The service provider. Do not rely on the services to be
        /// the same instances for the whole story.</param>
        /// <param name="uiMode">Can be used to run in silent mode.</param>
        /// <returns>The result of the story step.</returns>
        Task<StoryStepResult<TModel>> RunStep(TModel model, IServiceProvider serviceProvider, StoryMode uiMode);
    }
}
