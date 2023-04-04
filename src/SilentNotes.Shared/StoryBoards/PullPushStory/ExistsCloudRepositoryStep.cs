// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.StoryBoards.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotes.StoryBoards.PullPushStory
{
    /// <summary>
    /// This step belongs to the <see cref="PullPushStoryBoard"/>. It checks whether a
    /// repository exists in the cloud storage.
    /// </summary>
    public class ExistsCloudRepositoryStep : SynchronizationStory.ExistsCloudRepositoryStep
    {
        /// <inheritdoc/>
        public ExistsCloudRepositoryStep(
            Enum stepId,
            IStoryBoard storyBoard,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            ISettingsService settingsService,
            ICloudStorageClientFactory cloudStorageClientFactory)
            : base(stepId, storyBoard, languageService, feedbackService, settingsService, cloudStorageClientFactory)
        {
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            // Instead of reimplementing the whole story, we require a previous manual sync.
            SettingsModel settings = _settingsService.LoadSettingsOrDefault();
            if (!settings.HasCloudStorageClient || !settings.HasTransferCode)
            {
                _feedbackService.ShowToast(_languageService["pushpull_error_need_sync_first"]);
                return;
            }

            // Reuse SynchronizationStory
            StoryBoard.Session.Store(SynchronizationStorySessionKey.CloudStorageCredentials, settings.Credentials);
            StoryBoardStepResult result = await SynchronizationStory.ExistsCloudRepositoryStep.RunSilent(StoryBoardMode.Gui, StoryBoard.Session, _settingsService, _languageService, _cloudStorageClientFactory);

            // Instead of reimplementing the whole story, we require a manual sync in case of a problem.
            if (result.NextStepIs(SynchronizationStoryStepId.DownloadCloudRepository))
                await StoryBoard.ContinueWith(PullPushStoryStepId.DownloadCloudRepository);
            else
                _feedbackService.ShowToast(_languageService["pushpull_error_need_sync_first"]);
        }
    }
}
