using SilentNotes.Services;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
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
        private readonly ICloudStorageClientFactory _cloudStorageClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandleOAuthRedirectStep"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public HandleOAuthRedirectStep(
            Enum stepId,
            IStoryBoard storyBoard,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            ICloudStorageClientFactory cloudStorageClientFactory)
            : base(stepId, storyBoard)
        {
            _languageService = languageService;
            _feedbackService = feedbackService;
            _cloudStorageClientFactory = cloudStorageClientFactory;
        }

        public override async Task Run()
        {
            try
            {
                if (!StoryBoard.TryLoadFromSession(SynchronizationStorySessionKey.CloudStorageCredentials.ToInt(), out SerializeableCloudStorageCredentials credentials))
                    throw new ArgumentNullException(nameof(credentials));
                if (!StoryBoard.TryLoadFromSession(SynchronizationStorySessionKey.OauthState.ToInt(), out string oauthState))
                    throw new ArgumentNullException(nameof(oauthState));
                if (!StoryBoard.TryLoadFromSession(SynchronizationStorySessionKey.OauthCodeVerifier.ToInt(), out string oauthCodeVerifier))
                    throw new ArgumentNullException(nameof(oauthState));
                if (!StoryBoard.TryLoadFromSession(SynchronizationStorySessionKey.OauthRedirectUrl.ToInt(), out string redirectUrl))
                    throw new ArgumentNullException(nameof(redirectUrl));

                StoryBoard.RemoveFromSession(SynchronizationStorySessionKey.OauthState.ToInt());
                StoryBoard.RemoveFromSession(SynchronizationStorySessionKey.OauthCodeVerifier.ToInt());
                StoryBoard.RemoveFromSession(SynchronizationStorySessionKey.OauthRedirectUrl.ToInt());

                ICloudStorageClient cloudStorageClient = _cloudStorageClientFactory.GetOrCreate(credentials.CloudStorageId);
                if (cloudStorageClient is IOAuth2CloudStorageClient oauthStorageClient)
                {
                    CloudStorageToken token = await oauthStorageClient.FetchTokenAsync(redirectUrl, oauthState, oauthCodeVerifier);
                    if (token != null)
                    {
                        // User has granted access.
                        credentials.Token = token;
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
