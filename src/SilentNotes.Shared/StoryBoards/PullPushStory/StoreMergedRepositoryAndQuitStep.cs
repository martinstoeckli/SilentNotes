using System;
using System.Threading.Tasks;
using SilentNotes.Models;
using SilentNotes.Services;

namespace SilentNotes.StoryBoards.PullPushStory
{
    /// <summary>
    /// This step is an end point of the <see cref="PullPushStoryBoard"/>. It merges the
    /// local note with the downloaded note and stores the merged repository.
    /// </summary>
    public class StoreMergedRepositoryAndQuitStep : SynchronizationStory.StoreMergedRepositoryAndQuitStep
    {
        /// <inheritdoc/>
        public StoreMergedRepositoryAndQuitStep(
            int stepId, IStoryBoard storyBoard,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            ISettingsService settingsService,
            ICryptoRandomService randomService,
            IRepositoryStorageService repositoryStorageService,
            ICloudStorageClientFactory cloudStorageClientFactory)
            : base(stepId, storyBoard, languageService, feedbackService, settingsService, randomService, repositoryStorageService, cloudStorageClientFactory)
        {
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            try
            {
                NoteRepositoryModel cloudRepository = StoryBoard.LoadFromSession<NoteRepositoryModel>(PullPushStorySessionKey.CloudRepository.ToInt());
                _repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
                SettingsModel settings = _settingsService.LoadSettingsOrDefault();

                // Merge repositories
                // todo:

                _feedbackService.ShowToast(_languageService["sync_success"]);
                StoryBoard.ClearSession();
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                ShowExceptionMessage(ex, _feedbackService, _languageService);
            }
        }
    }
}
