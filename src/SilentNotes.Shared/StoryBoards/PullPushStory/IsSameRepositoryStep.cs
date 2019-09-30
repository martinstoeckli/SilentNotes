using System.Threading.Tasks;
using SilentNotes.Models;
using SilentNotes.Services;

namespace SilentNotes.StoryBoards.PullPushStory
{
    /// <summary>
    /// This step belongs to the <see cref="PullPushStoryBoard"/>. It checks whether the
    /// downloaded repository is the same repository as the one stored locally (has the same id).
    /// </summary>
    public class IsSameRepositoryStep : SynchronizationStory.IsSameRepositoryStep
    {
        protected readonly ILanguageService _languageService;
        protected readonly IFeedbackService _feedbackService;

        /// <inheritdoc/>
        public IsSameRepositoryStep(
            int stepId,
            IStoryBoard storyBoard,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            IRepositoryStorageService repositoryStorageService)
            : base(stepId, storyBoard, repositoryStorageService)
        {
            _languageService = languageService;
            _feedbackService = feedbackService;
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            _repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
            NoteRepositoryModel cloudRepository = StoryBoard.LoadFromSession<NoteRepositoryModel>(PullPushStorySessionKey.CloudRepository.ToInt());

            if (localRepository.Id == cloudRepository.Id)
            {
                await StoryBoard.ContinueWith(PullPushStoryStepId.StoreMergedRepositoryAndQuit.ToInt());
            }
            else
            {
                _feedbackService.ShowToast(_languageService["pushpull_error_need_sync_first"]);
            }
        }
    }
}
