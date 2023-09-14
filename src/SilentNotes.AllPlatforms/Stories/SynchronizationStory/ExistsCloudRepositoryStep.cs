// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Models;
using SilentNotes.Services;
using VanillaCloudStorageClient;

namespace SilentNotes.Stories.SynchronizationStory
{
    /// <summary>
    /// This step belongs to the <see cref="SynchronizationStory"/>. It checks whether a
    /// repository exists in the cloud storage.
    /// </summary>
    internal class ExistsCloudRepositoryStep : SynchronizationStoryStepBase
    {
        /// <inheritdoc/>
        public override async ValueTask<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            var cloudStorageClientFactory = serviceProvider.GetService<ICloudStorageClientFactory>();
            var settingsService = serviceProvider.GetService<ISettingsService>();
            var languageService = serviceProvider.GetService<ILanguageService>();
            try
            {
                SerializeableCloudStorageCredentials credentials = model.Credentials;
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
                    switch (uiMode)
                    {
                        case StoryMode.Gui:
                            // If GUI is allowed, ask for new login
                            return ToResult(new ShowCloudStorageAccountStep());
                        default:
                            return ToResultEndOfStory(languageService["sync_error_oauth_refresh"]);
                    }
                }
                else
                {
                    bool repositoryExists = await cloudStorageClient.ExistsFileAsync(Config.RepositoryFileName, credentials);

                    // If no error occured the credentials are ok and we can safe them
                    SaveCredentialsToSettings(settingsService, credentials);

                    if (repositoryExists)
                        return ToResult(new DownloadCloudRepositoryStep());
                    else
                        return ToResult(new StoreLocalRepositoryToCloudAndQuitStep());
                }
            }
            catch (Exception ex)
            {
                if (uiMode == StoryMode.Gui)
                    serviceProvider.GetService<IFeedbackService>().SetBusyIndicatorVisible(false, true);

                // Keep the current page open and show the error message
                return ToResult(ex);
            }
        }

        protected static void SaveCredentialsToSettings(ISettingsService settingsService, SerializeableCloudStorageCredentials credentials)
        {
            // todo:
            //SettingsModel settings = settingsService.LoadSettingsOrDefault();
            //if (!credentials.AreEqualOrNull(settings.Credentials))
            //{
            //    settings.Credentials = credentials;
            //    settingsService.TrySaveSettingsToLocalDevice(settings);
            //}
        }
    }
}
