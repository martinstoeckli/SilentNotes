using System;
using System.Threading.Tasks;
using SilentNotes.Models;
using SilentNotes.Services;
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
            int stepId,
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
            SettingsModel settings = _settingsService.LoadSettingsOrDefault();
            SerializeableCloudStorageCredentials credentials = settings.Credentials;
            if (!settings.HasCloudStorageClient || !settings.HasTransferCode)
            {
                _feedbackService.ShowToast(_languageService["pushpull_error_need_sync_first"]);
                return;
            }

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
                    catch (RefreshTokenExpiredException)
                    {
                        // Refresh-token cannot be used to get new access-tokens anymore, a new
                        // authorization by the user is required.
                        stopBecauseNewOAuthLoginIsRequired = true;
                        _feedbackService.ShowToast(_languageService["pushpull_error_need_sync_first"]);
                    }
                }

                if (!stopBecauseNewOAuthLoginIsRequired)
                {
                    bool repositoryExists = await cloudStorageClient.ExistsFileAsync(Config.RepositoryFileName, credentials);
                    if (repositoryExists)
                        await StoryBoard.ContinueWith(PullPushStoryStepId.DownloadCloudRepository.ToInt());
                    else
                        _feedbackService.ShowToast(_languageService["pushpull_error_need_sync_first"]);
                }
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                ShowExceptionMessage(ex, _feedbackService, _languageService);
            }
        }
    }
}
