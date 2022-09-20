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
    /// This step belongs to the <see cref="SynchronizationStoryBoard"/>.
    /// Entrypoint after the external browser redirects back to SilentNotes after an OAuth2 login.
    /// </summary>
    public class HandleOAuthRedirectStep : SynchronizationStoryBoardStepBase
    {
        private readonly ILanguageService _languageService;
        private readonly IFeedbackService _feedbackService;
        private readonly ISettingsService _settingsService;
        private readonly ICloudStorageClientFactory _cloudStorageClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandleOAuthRedirectStep"/> class.
        /// </summary>
        public HandleOAuthRedirectStep(
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

        public override async Task Run()
        {
            try
            {
                if (!StoryBoard.TryLoadFromSession(SynchronizationStorySessionKey.CloudStorageCredentials, out SerializeableCloudStorageCredentials credentials))
                    throw new ArgumentNullException(nameof(credentials));
                if (!StoryBoard.TryLoadFromSession(SynchronizationStorySessionKey.OauthState, out string oauthState))
                    throw new ArgumentNullException(nameof(oauthState));
                if (!StoryBoard.TryLoadFromSession(SynchronizationStorySessionKey.OauthCodeVerifier, out string oauthCodeVerifier))
                    throw new ArgumentNullException(nameof(oauthState));
                if (!StoryBoard.TryLoadFromSession(SynchronizationStorySessionKey.OauthRedirectUrl, out string redirectUrl))
                    throw new ArgumentNullException(nameof(redirectUrl));

                StoryBoard.RemoveFromSession(SynchronizationStorySessionKey.OauthState);
                StoryBoard.RemoveFromSession(SynchronizationStorySessionKey.OauthCodeVerifier);
                StoryBoard.RemoveFromSession(SynchronizationStorySessionKey.OauthRedirectUrl);

                ICloudStorageClient cloudStorageClient = _cloudStorageClientFactory.GetOrCreate(credentials.CloudStorageId);
                if (cloudStorageClient is IOAuth2CloudStorageClient oauthStorageClient)
                {
                    CloudStorageToken token = await oauthStorageClient.FetchTokenAsync(redirectUrl, oauthState, oauthCodeVerifier);
                    if (token != null)
                    {
                        // User has granted access.
                        credentials.Token = token;

                        // The new/refreshed tokens have to been replaced in any case, so we don't
                        // have to wait on ExistsCloudRepositoryStep for storing them. See issue #186
                        SettingsModel settings = _settingsService.LoadSettingsOrDefault();
                        settings.Credentials = credentials;
                        _settingsService.TrySaveSettingsToLocalDevice(settings);

                        await StoryBoard.ContinueWith(SynchronizationStoryStepId.ExistsCloudRepository);
                    }
                    else
                    {
                        // User has rejected access.
                        _feedbackService.ShowToast(_languageService.LoadText("sync_reject"));
                        await StoryBoard.ContinueWith(SynchronizationStoryStepId.StopAndShowRepository);
                    }
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
