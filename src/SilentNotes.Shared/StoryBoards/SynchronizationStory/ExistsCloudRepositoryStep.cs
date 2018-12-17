// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Services.CloudStorageServices;

namespace SilentNotes.StoryBoards.SynchronizationStory
{
    /// <summary>
    /// This step belongs to the <see cref="SynchronizationStoryBoard"/>. It checks whether a
    /// repository exists in the cloud storage.
    /// </summary>
    public class ExistsCloudRepositoryStep : StoryBoardStepBase
    {
        private readonly ILanguageService _languageService;
        private readonly IFeedbackService _feedbackService;
        private readonly ISettingsService _settingsService;
        private readonly ICloudStorageServiceFactory _cloudStorageServiceFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExistsCloudRepositoryStep"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public ExistsCloudRepositoryStep(
            int stepId,
            IStoryBoard storyBoard,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            ISettingsService settingsService,
            ICloudStorageServiceFactory cloudStorageServiceFactory)
            : base(stepId, storyBoard)
        {
            _languageService = languageService;
            _feedbackService = feedbackService;
            _settingsService = settingsService;
            _cloudStorageServiceFactory = cloudStorageServiceFactory;
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            CloudStorageAccount account = StoryBoard.LoadFromSession<CloudStorageAccount>(SynchronizationStorySessionKey.CloudStorageAccount.ToInt());
            ICloudStorageService cloudStorageService = _cloudStorageServiceFactory.Create(account);
            try
            {
                // Future OAuth2 services should follow the code flow instead.
                bool repositoryExists = await cloudStorageService.ExistsRepositoryAsync();

                // If no error occured the credentials are ok and we can safe them
                SettingsModel settings = _settingsService.LoadSettingsOrDefault();
                if (!account.Equals(settings.CloudStorageAccount))
                {
                    settings.CloudStorageAccount = account;
                    _settingsService.TrySaveSettingsToLocalDevice(settings);
                }

                if (repositoryExists)
                {
                    await StoryBoard.ContinueWith(SynchronizationStoryStepId.DownloadCloudRepository.ToInt());
                }
                else
                {
                    await StoryBoard.ContinueWith(SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit.ToInt());
                }
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                SynchronizationStoryBoard.ShowExceptionMessage(ex, _feedbackService, _languageService);
            }
        }
    }
}
