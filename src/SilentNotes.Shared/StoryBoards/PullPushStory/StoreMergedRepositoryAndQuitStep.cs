// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using SilentNotes.Models;
using SilentNotes.Services;
using VanillaCloudStorageClient;

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
            Enum stepId,
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
                NoteRepositoryModel cloudRepository = StoryBoard.Session.Load<NoteRepositoryModel>(PullPushStorySessionKey.CloudRepository);
                _repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
                SettingsModel settings = _settingsService.LoadSettingsOrDefault();
                SerializeableCloudStorageCredentials credentials = settings.Credentials;

                NoteModel cloudNote = cloudRepository.Notes.FindById(_noteId);
                NoteModel localNote = localRepository.Notes.FindById(_noteId);
                if (localNote == null)
                {
                    throw new Exception("PullPushStory is triggered on the note dialog, so the note must exist.");
                }

                if (cloudNote == null)
                {
                    // Note does not yet exist in the cloud, or it was deleted permanently. Both
                    // cases should be rejected.
                    _feedbackService.ShowToast(_languageService["pushpull_error_no_cloud_note"]);
                    return;
                }

                if (cloudNote.ModifiedAt == localNote.ModifiedAt)
                {
                    // Notes are equal, nothing to sync
                    _feedbackService.ShowToast(_languageService["pushpull_success"]);
                    return;
                }

                // Merge repositories
                if (_direction == PullPushDirection.PullFromServer)
                {
                    cloudNote.CloneTo(localNote); // this can possibly move the note to the recycling bin or reverse
                    AddSafeToOtherRepositoryIfMissing(cloudRepository, localRepository, cloudNote.SafeId);
                    _repositoryStorageService.TrySaveRepository(localRepository);
                }
                else
                {
                    // Uploading explicitely can be seen as a confirmation that this version is the
                    // most current one. So we make sure that the uploaded version is not overwritten
                    // by other devices afterwards.
                    localNote.RefreshModifiedAt();
                    _repositoryStorageService.TrySaveRepository(localRepository);

                    localNote.CloneTo(cloudNote); // this can possibly move the note to the recycling bin or reverse
                    AddSafeToOtherRepositoryIfMissing(localRepository, cloudRepository, localNote.SafeId);
                    byte[] encryptedRepository = EncryptRepository(
                        cloudRepository, settings.TransferCode, _cryptoRandomService, settings.SelectedEncryptionAlgorithm);

                    ICloudStorageClient cloudStorageClient = _cloudStorageClientFactory.GetByKey(credentials.CloudStorageId);
                    await cloudStorageClient.UploadFileAsync(Config.RepositoryFileName, encryptedRepository, credentials);
                }
                _feedbackService.ShowToast(_languageService["pushpull_success"]);
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                ShowExceptionMessage(ex, _feedbackService, _languageService);
            }
        }

        private static void AddSafeToOtherRepositoryIfMissing(NoteRepositoryModel myRepository, NoteRepositoryModel otherRepository, Guid? safeId)
        {
            bool isMissingInOtherRepository = (safeId != null) && (otherRepository.Safes.FindById(safeId) == null);
            if (isMissingInOtherRepository)
            {
                SafeModel mySafe = myRepository.Safes.FindById(safeId);
                if (mySafe != null)
                    otherRepository.Safes.Add(mySafe.Clone());
            }
        }
    }
}
