// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using SilentNotes.Models;
using SilentNotes.Services;
using VanillaCloudStorageClient;

namespace SilentNotes.StoryBoards.SynchronizationStory
{
    /// <summary>
    /// This step belongs to the <see cref="SynchronizationStoryBoard"/>. It checks whether a
    /// repository exists in the cloud storage.
    /// </summary>
    public class ExistsCloudRepositoryStep : SynchronizationStoryBoardStepBase
    {
        protected readonly ILanguageService _languageService;
        protected readonly IFeedbackService _feedbackService;
        protected readonly ISettingsService _settingsService;
        protected readonly ICloudStorageClientFactory _cloudStorageClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExistsCloudRepositoryStep"/> class.
        /// </summary>
        public ExistsCloudRepositoryStep(
            Enum stepId,
            IStoryBoard storyBoard,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            ISettingsService settingsService,
            ICloudStorageClientFactory cloudStorageClientFactory)
            : base(stepId, storyBoard)
        {
            _languageService = languageService;
            _feedbackService = feedbackService;
            _settingsService = settingsService;
            _cloudStorageClientFactory = cloudStorageClientFactory;
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            StoryBoardStepResult result = await RunSilent(
                StoryBoard.Mode,
                StoryBoard.Session, _settingsService,
                _languageService,
                _cloudStorageClientFactory);
            await StoryBoard.ShowFeedback(result, _feedbackService, _languageService);
            if (result.HasNextStep)
                await StoryBoard.ContinueWith(result.NextStepId);
        }

        /// <summary>
        /// Executes the parts of the step which can be run silently without UI in a background service.
        /// </summary>
        public static async Task<StoryBoardStepResult> RunSilent(
            StoryBoardMode mode,
            IStoryBoardSession session,
            ISettingsService settingsService,
            ILanguageService languageService,
            ICloudStorageClientFactory cloudStorageClientFactory)
        {
            try
            {
                SerializeableCloudStorageCredentials credentials = session.Load<SerializeableCloudStorageCredentials>(SynchronizationStorySessionKey.CloudStorageCredentials);
                ICloudStorageClient cloudStorageClient = cloudStorageClientFactory.GetByKey(credentials.CloudStorageId);

                bool stopBecauseManualOAuthLoginIsRequired = false;
                if ((cloudStorageClient is OAuth2CloudStorageClient oauthStorageClient) &&
                    credentials.Token.NeedsRefresh())
                {
                    try
                    {
                        // Get a new access token by using the refresh token
                        credentials.Token = await oauthStorageClient.RefreshTokenAsync(credentials.Token);
                        SaveCredentialsToSettings(settingsService, credentials);
                    }
                    catch (RefreshTokenExpiredException)
                    {
                        // Refresh-token cannot be used to get new access-tokens anymore, a new
                        // authorization by the user is required.
                        stopBecauseManualOAuthLoginIsRequired = true;
                    }
                }

                if (stopBecauseManualOAuthLoginIsRequired)
                {
                    switch (mode)
                    {
                        case StoryBoardMode.Gui:
                            // If GUI is allowed, ask for new login
                            return new StoryBoardStepResult(SynchronizationStoryStepId.ShowCloudStorageAccount);
                        case StoryBoardMode.Silent:
                            return new StoryBoardStepResult(null, languageService["sync_error_oauth_refresh"]);
                        default:
                            throw new ArgumentOutOfRangeException(nameof(mode));
                    }
                }
                else
                {
                    bool repositoryExists = await cloudStorageClient.ExistsFileAsync(Config.RepositoryFileName, credentials);

                    // If no error occured the credentials are ok and we can safe them
                    SaveCredentialsToSettings(settingsService, credentials);

                    if (repositoryExists)
                        return new StoryBoardStepResult(SynchronizationStoryStepId.DownloadCloudRepository);
                    else
                        return new StoryBoardStepResult(SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit);
                }
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                return new StoryBoardStepResult(ex);
            }
        }

        protected static void SaveCredentialsToSettings(ISettingsService settingsService, SerializeableCloudStorageCredentials credentials)
        {
            SettingsModel settings = settingsService.LoadSettingsOrDefault();
            if (!credentials.AreEqualOrNull(settings.Credentials))
            {
                settings.Credentials = credentials;
                settingsService.TrySaveSettingsToLocalDevice(settings);
            }
        }
    }
}
