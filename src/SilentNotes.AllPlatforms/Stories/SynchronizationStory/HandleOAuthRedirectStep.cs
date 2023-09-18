// Copyright © 2018 Martin Stoeckli.
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
    /// This step belongs to the <see cref="SynchronizationStory"/>.
    /// Entrypoint after the external browser redirects back to SilentNotes after an OAuth2 login.
    /// </summary>
    internal class HandleOAuthRedirectStep : SynchronizationStoryStepBase
    {
        /// <inheritdoc/>
        public override async ValueTask<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            try
            {
                var cloudStorageClientFactory = serviceProvider.GetService<ICloudStorageClientFactory>();
                var settingsService = serviceProvider.GetService<ISettingsService>();
                var languageService = serviceProvider.GetService<ILanguageService>();

                if (model.Credentials == null)
                    throw new ArgumentNullException(nameof(model.Credentials));
                if (string.IsNullOrEmpty(model.OauthState))
                    throw new ArgumentNullException(nameof(model.OauthState));
                if (string.IsNullOrEmpty(model.OauthCodeVerifier))
                    throw new ArgumentNullException(nameof(model.OauthCodeVerifier));
                if (string.IsNullOrEmpty(model.OauthRedirectUrl))
                    throw new ArgumentNullException(nameof(model.OauthRedirectUrl));

                ICloudStorageClient cloudStorageClient = cloudStorageClientFactory.GetByKey(model.Credentials.CloudStorageId);
                if (cloudStorageClient is IOAuth2CloudStorageClient oauthStorageClient)
                {
                    CloudStorageToken token = await oauthStorageClient.FetchTokenAsync(
                        model.OauthRedirectUrl, model.OauthState, model.OauthCodeVerifier);
                    if (token != null)
                    {
                        // User has granted access.
                        model.Credentials.Token = token;

                        // The new/refreshed tokens have to been replaced in any case, so we don't
                        // have to wait on ExistsCloudRepositoryStep for storing them. See issue #186
                        SettingsModel settings = settingsService.LoadSettingsOrDefault();
                        settings.Credentials = model.Credentials;
                        settingsService.TrySaveSettingsToLocalDevice(settings);

                        return ToResult(new ExistsCloudRepositoryStep());
                    }
                    else
                    {
                        // User has rejected access.
                        return ToResult(new StopAndShowRepositoryStep(), languageService.LoadText("sync_reject"), null);
                    }
                }
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                return ToResult(ex);
            }
            return ToResult(new StopAndShowRepositoryStep());
        }
    }
}
