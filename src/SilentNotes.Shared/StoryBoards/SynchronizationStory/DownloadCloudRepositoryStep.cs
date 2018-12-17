// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SilentNotes.Services;
using SilentNotes.Services.CloudStorageServices;

namespace SilentNotes.StoryBoards.SynchronizationStory
{
    /// <summary>
    /// This step belongs to the <see cref="SynchronizationStoryBoard"/>. It downloads the
    /// repository from the cloud storage.
    /// </summary>
    public class DownloadCloudRepositoryStep : StoryBoardStepBase
    {
        private readonly ILanguageService _languageService;
        private readonly IFeedbackService _feedbackService;
        private readonly ICloudStorageServiceFactory _cloudStorageServiceFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="DownloadCloudRepositoryStep"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public DownloadCloudRepositoryStep(
            int stepId,
            IStoryBoard storyBoard,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            ICloudStorageServiceFactory cloudStorageServiceFactory)
            : base(stepId, storyBoard)
        {
            _languageService = languageService;
            _feedbackService = feedbackService;
            _cloudStorageServiceFactory = cloudStorageServiceFactory;
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            CloudStorageAccount account = StoryBoard.LoadFromSession<CloudStorageAccount>(SynchronizationStorySessionKey.CloudStorageAccount.ToInt());
            ICloudStorageService cloudStorageService = _cloudStorageServiceFactory.Create(account);
            if (cloudStorageService == null)
                return;

            try
            {
                // The repository can be cached for this story, download the repository only once.
                byte[] binaryCloudRepository;
                if (!StoryBoard.TryLoadFromSession(SynchronizationStorySessionKey.BinaryCloudRepository.ToInt(), out binaryCloudRepository))
                {
                    binaryCloudRepository = await cloudStorageService.DownloadRepositoryAsync();
                    StoryBoard.StoreToSession(SynchronizationStorySessionKey.BinaryCloudRepository.ToInt(), binaryCloudRepository);
                }
                await StoryBoard.ContinueWith(SynchronizationStoryStepId.ExistsTransferCode.ToInt());
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                SynchronizationStoryBoard.ShowExceptionMessage(ex, _feedbackService, _languageService);
            }
        }
    }
}
