﻿// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using SilentNotes.Controllers;
using SilentNotes.Services;

namespace SilentNotes.StoryBoards.SynchronizationStory
{
    /// <summary>
    /// This step belongs to the <see cref="SynchronizationStoryBoard"/>. It shows the dialog to
    /// choose the merge method.
    /// </summary>
    public class ShowMergeChoiceStep : SynchronizationStoryBoardStepBase
    {
        private readonly INavigationService _navigationService;
        private readonly IFeedbackService _feedbackService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowMergeChoiceStep"/> class.
        /// </summary>
        public ShowMergeChoiceStep(
            Enum stepId,
            IStoryBoard storyBoard,
            INavigationService navigationService,
            IFeedbackService feedbackService)
            : base(stepId, storyBoard)
        {
            _navigationService = navigationService;
            _feedbackService = feedbackService;
        }

        /// <inheritdoc/>
        public override Task Run()
        {
            if (StoryBoard.Mode.ShouldUseGui())
            {
                _feedbackService.ShowBusyIndicator(false);
                _navigationService.Navigate(new Navigation(ControllerNames.MergeChoice));
            }
            return Task.CompletedTask;
        }
    }
}
