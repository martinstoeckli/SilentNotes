// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
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
        private readonly ILanguageService _languageService;
        private readonly IFeedbackService _feedbackService;
        private readonly ISettingsService _settingsService;
        private readonly ICloudStorageClientFactory _cloudStorageClientFactory;

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
            SerializeableCloudStorageCredentials credentials = StoryBoard.LoadFromSession<SerializeableCloudStorageCredentials>(SynchronizationStorySessionKey.CloudStorageCredentials.ToInt());
            ICloudStorageClient cloudStorageClient = _cloudStorageClientFactory.GetOrCreate(credentials.CloudStorageId);
            try
            {
                bool stopBecauseNewOAuthLoginIsRequired = false;
                if ((cloudStorageClient is OAuth2CloudStorageClient oauthStorageClient) &&
                    credentials.Token.NeedsRefresh())
                {
                    try
                    {
                        // Get a new access token by using the refresh token
                        credentials.Token = await oauthStorageClient.RefreshTokenAsync(credentials.Token);
                        SaveCredentialsToSettings(credentials);
                    }
                    catch (InvalidGrantException)
                    {
                        // Refresh-token cannot be used to get new access-tokens anymore, a new
                        // authorization by the user is required.
                        stopBecauseNewOAuthLoginIsRequired = true;
                        switch (StoryBoard.Mode)
                        {
                            case StoryBoardMode.GuiAndToasts:
                                await StoryBoard.ContinueWith(SynchronizationStoryStepId.ShowCloudStorageAccount.ToInt());
                                break;
                            case StoryBoardMode.ToastsOnly:
                                _feedbackService.ShowToast(_languageService["sync_error_generic"]);
                                break;
                        }
                    }
                }

                if (!stopBecauseNewOAuthLoginIsRequired)
                {
                    bool repositoryExists = await cloudStorageClient.ExistsFileAsync(Config.RepositoryFileName, credentials);

                    // If no error occured the credentials are ok and we can safe them
                    SaveCredentialsToSettings(credentials);

                    if (repositoryExists)
                        await StoryBoard.ContinueWith(SynchronizationStoryStepId.DownloadCloudRepository.ToInt());
                    else
                        await StoryBoard.ContinueWith(SynchronizationStoryStepId.StoreLocalRepositoryToCloudAndQuit.ToInt());
                }
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                ShowExceptionMessage(ex, _feedbackService, _languageService);
            }
        }

        private void SaveCredentialsToSettings(SerializeableCloudStorageCredentials credentials)
        {
            SettingsModel settings = _settingsService.LoadSettingsOrDefault();
            if (!credentials.AreEqualOrNull(settings.Credentials))
            {
                settings.Credentials = credentials;
                _settingsService.TrySaveSettingsToLocalDevice(settings);
            }
        }
    }
}
