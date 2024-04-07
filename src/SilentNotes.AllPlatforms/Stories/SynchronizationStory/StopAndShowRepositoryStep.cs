// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Services;

namespace SilentNotes.Stories.SynchronizationStory
{
    /// <summary>
    /// This step is an end point of the <see cref="SynchronizationStory"/>. It aborts the
    /// story and goes back to the repository overview.
    /// </summary>
    internal class StopAndShowRepositoryStep : SynchronizationStoryStepBase
    {
        /// <inheritdoc/>
        public override Task<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            System.Diagnostics.Debug.WriteLine("** " + nameof(StopAndShowRepositoryStep) + " " + uiMode.ToString());

            if (uiMode.HasFlag(StoryMode.BusyIndicator))
            {
                var feedbackService = serviceProvider.GetService<IFeedbackService>();
                feedbackService.SetBusyIndicatorVisible(false, true);
            }

            if (uiMode.HasFlag(StoryMode.Dialogs))
            {
                var synchronizationService = serviceProvider.GetService<ISynchronizationService>();
                synchronizationService.FinishedManualSynchronization(serviceProvider);

                var navigation = serviceProvider.GetService<INavigationService>();
                navigation.NavigateTo(RouteNames.NoteRepository, true);
            }
            return ToTask(ToResultEndOfStory());
        }
    }
}
