// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.StoryBoards.SynchronizationStory;

namespace SilentNotes.StoryBoards.PullPushStory
{
    /// <summary>
    /// This step belongs to the <see cref="PullPushStoryBoard"/>. It checks whether the
    /// downloaded repository is the same repository as the one stored locally (has the same id).
    /// </summary>
    public class IsSameRepositoryStep : SynchronizationStory.IsSameRepositoryStep
    {
        private readonly ILanguageService _languageService;
        private readonly IFeedbackService _feedbackService;

        /// <inheritdoc/>
        public IsSameRepositoryStep(
            Enum stepId,
            IStoryBoard storyBoard,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            IRepositoryStorageService repositoryStorageService)
            : base(stepId, storyBoard, repositoryStorageService)
        {
            _languageService = languageService;
            _feedbackService = feedbackService;
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            // Reuse SynchronizationStory
            StoryBoardStepResult result = SynchronizationStory.IsSameRepositoryStep.RunSilent(StoryBoard.Session, _repositoryStorageService);

            // Instead of reimplementing the whole story, we require a manual sync in case of a problem.
            if (result.NextStepIs(SynchronizationStoryStepId.StoreMergedRepositoryAndQuit))
                await StoryBoard.ContinueWith(PullPushStoryStepId.StoreMergedRepositoryAndQuit);
            else
                _feedbackService.ShowToast(_languageService["pushpull_error_need_sync_first"]);
        }
    }
}
