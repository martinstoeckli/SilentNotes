using System;
using System.Threading.Tasks;
using SilentNotes.Models;
using SilentNotes.Services;

namespace SilentNotes.StoryBoards.SynchronizationStory
{
    /// <summary>
    /// This step is an end point of the <see cref="SynchronizationStoryBoard"/>. It keeps the
    /// cloud repository and stores it to the local device.
    /// </summary>
    public class StoreCloudRepositoryToDeviceAndQuitStep : SynchronizationStoryBoardStepBase
    {
        private readonly ILanguageService _languageService;
        private readonly IFeedbackService _feedbackService;
        private readonly IRepositoryStorageService _repositoryStorageService;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreCloudRepositoryToDeviceAndQuitStep"/> class.
        /// </summary>
        public StoreCloudRepositoryToDeviceAndQuitStep(
            Enum stepId,
            IStoryBoard storyBoard,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            IRepositoryStorageService repositoryStorageService)
            : base(stepId, storyBoard)
        {
            _languageService = languageService;
            _feedbackService = feedbackService;
            _repositoryStorageService = repositoryStorageService;
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            try
            {
                NoteRepositoryModel cloudRepository = StoryBoard.LoadFromSession<NoteRepositoryModel>(SynchronizationStorySessionKey.CloudRepository);
                _repositoryStorageService.TrySaveRepository(cloudRepository);
                await StoryBoard.ContinueWith(SynchronizationStoryStepId.StopAndShowRepository);
                _feedbackService.ShowToast(_languageService["sync_success"]);
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                ShowExceptionMessage(ex, _feedbackService, _languageService);
            }
        }
    }
}
