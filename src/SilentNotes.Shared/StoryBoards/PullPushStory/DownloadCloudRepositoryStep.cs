// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using SilentNotes.Services;
using SilentNotes.StoryBoards.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotes.StoryBoards.PullPushStory
{
    /// <summary>
    /// This step belongs to the <see cref="PullPushStoryBoard"/>. It downloads the
    /// repository from the cloud storage.
    /// </summary>
    public class DownloadCloudRepositoryStep : SynchronizationStory.DownloadCloudRepositoryStep
    {
        /// <inheritdoc/>
        public DownloadCloudRepositoryStep(
            Enum stepId,
            IStoryBoard storyBoard,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            ICloudStorageClientFactory cloudStorageClientFactory)
            : base(stepId, storyBoard, languageService, feedbackService, cloudStorageClientFactory)
        {
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            // Reuse SynchronizationStory
            StoryBoardStepResult result = await SynchronizationStory.DownloadCloudRepositoryStep.RunSilent(StoryBoard.Session, _cloudStorageClientFactory);

            // Instead of reimplementing the whole story, we require a manual sync in case of a problem.
            if (result.NextStepIs(SynchronizationStoryStepId.ExistsTransferCode))
                await StoryBoard.ContinueWith(PullPushStoryStepId.DecryptCloudRepository);
            else
                _feedbackService.ShowToast(_languageService["pushpull_error_need_sync_first"]);
        }
    }
}
