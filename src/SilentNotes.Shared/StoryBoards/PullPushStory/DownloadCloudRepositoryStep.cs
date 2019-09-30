using System;
using System.Threading.Tasks;
using SilentNotes.Services;
using VanillaCloudStorageClient;

namespace SilentNotes.StoryBoards.PullPushStory
{
    /// <summary>
    /// This step belongs to the <see cref="PullPushStoryBoard"/>. It downloads the
    /// repository from the cloud storage.
    /// </summary>
    public class DownloadCloudRepositoryStep : SynchronizationStory.DownloadCloudRepositoryStep
    {
        protected readonly ISettingsService _settingsService;

        /// <inheritdoc/>
        public DownloadCloudRepositoryStep(
            int stepId,
            IStoryBoard storyBoard,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            ICloudStorageClientFactory cloudStorageClientFactory,
            ISettingsService settingsService)
            : base(stepId, storyBoard, languageService, feedbackService, cloudStorageClientFactory)
        {
            _settingsService = settingsService;
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            SerializeableCloudStorageCredentials credentials = _settingsService.LoadSettingsOrDefault().Credentials;
            ICloudStorageClient cloudStorageClient = _cloudStorageClientFactory.GetOrCreate(credentials.CloudStorageId);

            try
            {
                // The repository can be cached for this story, download the repository only once.
                byte[] binaryCloudRepository = await cloudStorageClient.DownloadFileAsync(Config.RepositoryFileName, credentials);
                StoryBoard.StoreToSession(PullPushStorySessionKey.BinaryCloudRepository.ToInt(), binaryCloudRepository);
                await StoryBoard.ContinueWith(PullPushStoryStepId.DecryptCloudRepository.ToInt());
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                ShowExceptionMessage(ex, _feedbackService, _languageService);
            }
        }
    }
}
