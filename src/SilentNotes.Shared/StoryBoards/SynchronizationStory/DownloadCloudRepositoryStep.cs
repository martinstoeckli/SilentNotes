// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using SilentNotes.Services;
using VanillaCloudStorageClient;

namespace SilentNotes.StoryBoards.SynchronizationStory
{
    /// <summary>
    /// This step belongs to the <see cref="SynchronizationStoryBoard"/>. It downloads the
    /// repository from the cloud storage.
    /// </summary>
    public class DownloadCloudRepositoryStep : SynchronizationStoryBoardStepBase
    {
        protected readonly ILanguageService _languageService;
        protected readonly IFeedbackService _feedbackService;
        protected readonly ICloudStorageClientFactory _cloudStorageClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadCloudRepositoryStep"/> class.
        /// </summary>
        public DownloadCloudRepositoryStep(
            Enum stepId,
            IStoryBoard storyBoard,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            ICloudStorageClientFactory cloudStorageClientFactory)
            : base(stepId, storyBoard)
        {
            _languageService = languageService;
            _feedbackService = feedbackService;
            _cloudStorageClientFactory = cloudStorageClientFactory;
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            StoryBoardStepResult result = await RunSilent(
                StoryBoard.Session,
                _cloudStorageClientFactory);
            await StoryBoard.ShowFeedback(result, _feedbackService, _languageService);
            if (result.HasNextStep)
                await StoryBoard.ContinueWith(result.NextStepId);
        }

        /// <summary>
        /// Executes the parts of the step which can be run silently without UI in a background service.
        /// </summary>
        public static async Task<StoryBoardStepResult> RunSilent(
            IStoryBoardSession session,
            ICloudStorageClientFactory cloudStorageClientFactory)
        {
            try
            {
                SerializeableCloudStorageCredentials credentials = session.Load<SerializeableCloudStorageCredentials>(SynchronizationStorySessionKey.CloudStorageCredentials);
                ICloudStorageClient cloudStorageClient = cloudStorageClientFactory.GetByKey(credentials.CloudStorageId);

                // The repository can be cached for this story, download the repository only once.
                byte[] binaryCloudRepository;
                if (!session.TryLoad(SynchronizationStorySessionKey.BinaryCloudRepository, out binaryCloudRepository))
                {
                    binaryCloudRepository = await cloudStorageClient.DownloadFileAsync(Config.RepositoryFileName, credentials);
                    session.Store(SynchronizationStorySessionKey.BinaryCloudRepository, binaryCloudRepository);
                }
                return new StoryBoardStepResult(SynchronizationStoryStepId.ExistsTransferCode);
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                return new StoryBoardStepResult(ex);
            }
        }
    }
}
