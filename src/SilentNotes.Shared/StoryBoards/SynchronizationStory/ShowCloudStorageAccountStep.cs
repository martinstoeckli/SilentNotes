// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SilentNotes.Controllers;
using SilentNotes.Services;
using SilentNotes.Services.CloudStorageServices;

namespace SilentNotes.StoryBoards.SynchronizationStory
{
    /// <summary>
    /// This step belongs to the <see cref="SynchronizationStoryBoard"/>. It shows the dialog to
    /// enter credentials for the cloud storage. In case of an OAuth2 login, it opens the external
    /// browser and waits for the confirmation of the open auth login.
    /// </summary>
    public class ShowCloudStorageAccountStep : SynchronizationStoryBoardStepBase
    {
        private readonly INavigationService _navigationService;
        private readonly ILanguageService _languageService;
        private readonly IFeedbackService _feedbackService;
        private readonly ICloudStorageServiceFactory _cloudStorageServiceFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowCloudStorageAccountStep"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public ShowCloudStorageAccountStep(
            int stepId,
            IStoryBoard storyBoard,
            INavigationService navigationService,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            ICloudStorageServiceFactory cloudStorageServiceFactory)
            : base(stepId, storyBoard)
        {
            _navigationService = navigationService;
            _languageService = languageService;
            _cloudStorageServiceFactory = cloudStorageServiceFactory;
            _feedbackService = feedbackService;
        }

        /// <inheritdoc/>
        public override Task Run()
        {
            if (!IsRunningInSilentMode)
            {
                CloudStorageAccount account = StoryBoard.LoadFromSession<CloudStorageAccount>(SynchronizationStorySessionKey.CloudStorageAccount.ToInt());
                ICloudStorageService cloudStorageService = _cloudStorageServiceFactory.Create(account);
                if (cloudStorageService is IOauth2CloudStorageService oauthStorageService)
                {
                    // show waiting page
                    _navigationService.Navigate(ControllerNames.CloudStorageOauthWaiting);

                    StoryBoard.StoreToSession(SynchronizationStorySessionKey.OauthCloudStorageService.ToInt(), oauthStorageService);
                    oauthStorageService.Redirected += OauthStorageRedirectedEventHandler;
                    oauthStorageService.ShowOauth2LoginPage();
                }
                else
                {
                    _navigationService.Navigate(ControllerNames.CloudStorageAccount);
                }
            }
            return GetCompletedDummyTask();
        }

        private async void OauthStorageRedirectedEventHandler(object sender, RedirectedEventArgs e)
        {
            StoryBoard.RemoveFromSession(SynchronizationStorySessionKey.OauthCloudStorageService.ToInt());
            switch (e.RedirectResult)
            {
                case Oauth2RedirectResult.Permitted:
                    CloudStorageAccount account = StoryBoard.LoadFromSession<CloudStorageAccount>(SynchronizationStorySessionKey.CloudStorageAccount.ToInt());
                    account.OauthAccessToken = e.OauthAccessToken;
                    await StoryBoard.ContinueWith(SynchronizationStoryStepId.ExistsCloudRepository.ToInt());
                    break;
                case Oauth2RedirectResult.Rejected:
                    await StoryBoard.ContinueWith(SynchronizationStoryStepId.StopAndShowRepository.ToInt());
                    _feedbackService.ShowToast(_languageService.LoadText("sync_reject"));
                    break;
            }
        }
    }
}
