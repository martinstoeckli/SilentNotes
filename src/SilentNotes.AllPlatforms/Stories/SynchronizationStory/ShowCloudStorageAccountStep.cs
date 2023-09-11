// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;
using SilentNotes.Crypto;
using SilentNotes.Services;
using VanillaCloudStorageClient;

namespace SilentNotes.Stories.SynchronizationStory
{
    /// <summary>
    /// This step belongs to the <see cref="SynchronizationStory"/>. It shows the dialog to
    /// enter credentials for the cloud storage. In case of an OAuth2 login, it opens the external
    /// browser and waits for the confirmation of the open auth login.
    /// </summary>
    internal class ShowCloudStorageAccountStep : SynchronizationStoryStepBase
    {
        /// <inheritdoc/>
        public override ValueTask<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            if (uiMode == StoryMode.Gui)
            {
                var cloudStorageClientFactory = serviceProvider.GetService<ICloudStorageClientFactory>();
                var navigation = serviceProvider.GetService<INavigationService>();
                var randomSource = serviceProvider.GetService<ICryptoRandomService>();
                var nativeBrowserService = serviceProvider.GetService<INativeBrowserService>();

                SerializeableCloudStorageCredentials credentials = model.Credentials;
                ICloudStorageClient cloudStorageClient = cloudStorageClientFactory.GetByKey(credentials.CloudStorageId);
                if (cloudStorageClient is IOAuth2CloudStorageClient oauthStorageClient)
                {
                    // Show waiting page
                    navigation.NavigateTo(Routes.CloudStorageOauthWaiting);

                    // Open OAuth2 login page in external browser
                    model.OauthState = CryptoUtils.GenerateRandomBase62String(16, randomSource);
                    model.OauthCodeVerifier = CryptoUtils.GenerateRandomBase62String(64, randomSource);

                    string url = oauthStorageClient.BuildAuthorizationRequestUrl(
                        model.OauthState, model.OauthCodeVerifier);
                    // todo:
                    //nativeBrowserService.OpenWebsiteInApp(url);
                }
                else
                {
                    navigation.NavigateTo(Routes.CloudStorageAccount);
                }
            }
            return FromResultEndOfStory();
        }
    }
}
