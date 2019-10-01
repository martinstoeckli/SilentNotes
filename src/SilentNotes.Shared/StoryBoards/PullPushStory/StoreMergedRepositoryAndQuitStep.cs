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
        private readonly Guid _noteId;
        private readonly PullPushDirection _direction;

        /// <inheritdoc/>
        public StoreMergedRepositoryAndQuitStep(
            int stepId,
            IStoryBoard storyBoard,
            Guid noteId,
            PullPushDirection direction,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            ISettingsService settingsService,
            ICryptoRandomService randomService,
            IRepositoryStorageService repositoryStorageService,
            ICloudStorageClientFactory cloudStorageClientFactory)
            : base(stepId, storyBoard, languageService, feedbackService, settingsService, randomService, repositoryStorageService, cloudStorageClientFactory)
        {
            _noteId = noteId;
            _direction = direction;
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            try
            {
                NoteRepositoryModel cloudRepository = StoryBoard.LoadFromSession<NoteRepositoryModel>(PullPushStorySessionKey.CloudRepository.ToInt());
                _repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
                SettingsModel settings = _settingsService.LoadSettingsOrDefault();

                NoteInfo cloudInfo = CreateNoteInfo(cloudRepository, _noteId);
                NoteInfo localInfo = CreateNoteInfo(localRepository, _noteId);
                if (!localInfo.NoteExists)
                {
                    throw new Exception("PullPushStory is triggered on the note dialog, so the note must exist.");
                }

                if (!cloudInfo.NoteExists)
                {
                    _feedbackService.ShowToast(_languageService["pushpull_error_no_cloud_note"]);
                    return;
                }

                if (cloudInfo.Note.ModifiedAt == localInfo.Note.ModifiedAt)
                {
                    // Notes are equal, nothing to sync
                    _feedbackService.ShowToast(_languageService["sync_success"]);
                    return;
                }

                // Merge repositories
                if (_direction == PullPushDirection.PullFromServer)
                {
                    cloudInfo.Note.CopyTo(localInfo.Note);
                    _repositoryStorageService.TrySaveRepository(localRepository);
                    _feedbackService.ShowToast(_languageService["sync_success"]);
                }
                else
                {
                    // todo:
                }
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                ShowExceptionMessage(ex, _feedbackService, _languageService);
            }
        }

        private static NoteInfo CreateNoteInfo(NoteRepositoryModel repository, Guid noteId)
        {
            NoteInfo result = new NoteInfo();
            result.Note = repository.Notes.FindById(noteId);
            result.NoteExists = result.Note != null;
            result.NoteIsRecycled = result.NoteExists && result.Note.InRecyclingBin;
            result.NoteIsDeleted = repository.DeletedNotes.Contains(noteId);
            return result;
        }

        private class NoteInfo
        {
            public NoteModel Note { get; set; }

            public bool NoteExists { get; set; }

            public bool NoteIsRecycled { get;set; }

            public bool NoteIsDeleted { get; set; }
        }
    }
}
