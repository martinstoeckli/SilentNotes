// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.StoryBoards.SynchronizationStory;
using SilentNotes.Workers;

namespace SilentNotes.StoryBoards.PullPushStory
{
    /// <summary>
    /// This step belongs to the <see cref="PullPushStoryBoard"/>. It tries to decrypt an
    /// already downloaded cloud repository with the known transfer codes.
    /// </summary>
    public class DecryptCloudRepositoryStep : SynchronizationStory.DecryptCloudRepositoryStep
    {
        /// <inheritdoc/>
        public DecryptCloudRepositoryStep(
            Enum stepId,
            IStoryBoard storyBoard,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            ISettingsService settingsService,
            INoteRepositoryUpdater noteRepositoryUpdater)
            : base(stepId, storyBoard, languageService, feedbackService, settingsService, noteRepositoryUpdater)
        {
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            // Reuse SynchronizationStory
            StoryBoardStepResult result = SynchronizationStory.DecryptCloudRepositoryStep.RunSilent(StoryBoard.Session, _settingsService, _languageService, _noteRepositoryUpdater);

            // Instead of reimplementing the whole story, we require a manual sync in case of a problem.
            if (result.NextStepIs(SynchronizationStoryStepId.IsSameRepository))
                await StoryBoard.ContinueWith(PullPushStoryStepId.IsSameRepository);
            else
                _feedbackService.ShowToast(_languageService["pushpull_error_need_sync_first"]);
        }
    }
}
