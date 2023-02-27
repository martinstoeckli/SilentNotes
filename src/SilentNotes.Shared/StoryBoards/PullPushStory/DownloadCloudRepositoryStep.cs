// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using SilentNotes.Services;
using VanillaCloudStorageClient;

namespace SilentNotes.StoryBoards.PullPushStory
{
    /// <summary>
    /// This step belongs to the <see cref="PullPushStoryBoard"/>. It downloads the
    /// repository from the cloud storage.
    /// </summary>
    public class DownloadCloudRepositoryStep : SynchronizationStory.DownloadCloudRepositoryStep
    {
        private readonly ISettingsService _settingsService;

        /// <inheritdoc/>
        public DownloadCloudRepositoryStep(
            Enum stepId,
            IStoryBoard storyBoard,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            ICloudStorageClientFactory cloudStorageClientFactory,
            ISettingsService settingsService)
            : base(stepId, storyBoard, languageService, feedbackService, cloudStorageClientFactory)
        {
            _settingsService = settingsService;
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            SerializeableCloudStorageCredentials credentials = _settingsService.LoadSettingsOrDefault().Credentials;
            ICloudStorageClient cloudStorageClient = _cloudStorageClientFactory.GetByKey(credentials.CloudStorageId);

            try
            {
                // The repository can be cached for this story, download the repository only once.
                byte[] binaryCloudRepository = await cloudStorageClient.DownloadFileAsync(Config.RepositoryFileName, credentials);
                StoryBoard.Session.Store(PullPushStorySessionKey.BinaryCloudRepository, binaryCloudRepository);
                await StoryBoard.ContinueWith(PullPushStoryStepId.DecryptCloudRepository);
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                ShowExceptionMessage(ex, _feedbackService, _languageService);
            }
        }
    }
}
