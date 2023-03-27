// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;
using VanillaCloudStorageClient;

namespace SilentNotes.StoryBoards.SynchronizationStory
{
    /// <summary>
    /// This step is an end point of the <see cref="SynchronizationStoryBoard"/>. It merges the
    /// local repository with the downloaded repository and stores the merged repository.
    /// </summary>
    public class StoreMergedRepositoryAndQuitStep : SynchronizationStoryBoardStepBase
    {
        protected readonly ILanguageService _languageService;
        protected readonly IFeedbackService _feedbackService;
        protected readonly ISettingsService _settingsService;
        protected readonly ICryptoRandomService _cryptoRandomService;
        protected readonly IRepositoryStorageService _repositoryStorageService;
        protected readonly ICloudStorageClientFactory _cloudStorageClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreMergedRepositoryAndQuitStep"/> class.
        /// </summary>
        public StoreMergedRepositoryAndQuitStep(
            Enum stepId,
            IStoryBoard storyBoard,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            ISettingsService settingsService,
            ICryptoRandomService randomService,
            IRepositoryStorageService repositoryStorageService,
            ICloudStorageClientFactory cloudStorageClientFactory)
            : base(stepId, storyBoard)
        {
            _languageService = languageService;
            _feedbackService = feedbackService;
            _settingsService = settingsService;
            _cryptoRandomService = randomService;
            _repositoryStorageService = repositoryStorageService;
            _cloudStorageClientFactory = cloudStorageClientFactory;
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            StoryBoardStepResult result = await RunSilent(
                StoryBoard.Session,
                _settingsService,
                _languageService,
                _cryptoRandomService,
                _repositoryStorageService,
                _cloudStorageClientFactory);
            await StoryBoard.ShowFeedback(result, _feedbackService, _languageService);
            if (result.HasNextStep)
                await StoryBoard.ContinueWith(result.NextStepId);
        }

        /// <summary>
        /// Executes the parts of the step which can be run silently without UI in a background service.
        /// </summary>
        public static async Task<StoryBoardStepResult> RunSilent(
            IStoryBoardSession session,
            ISettingsService settingsService,
            ILanguageService languageService,
            ICryptoRandomService cryptoRandomService,
            IRepositoryStorageService repositoryStorageService,
            ICloudStorageClientFactory cloudStorageClientFactory)
        {
            try
            {
                NoteRepositoryModel cloudRepository = session.Load<NoteRepositoryModel>(SynchronizationStorySessionKey.CloudRepository);
                SerializeableCloudStorageCredentials credentials = session.Load<SerializeableCloudStorageCredentials>(SynchronizationStorySessionKey.CloudStorageCredentials);
                repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
                SettingsModel settings = settingsService.LoadSettingsOrDefault();

                // Merge repositories
                NoteRepositoryMerger merger = new NoteRepositoryMerger();
                NoteRepositoryModel mergedRepository = merger.Merge(localRepository, cloudRepository);

                // Store merged repository locally when different
                if (!RepositoriesAreEqual(mergedRepository, localRepository))
                {
                    repositoryStorageService.TrySaveRepository(mergedRepository);
                }

                // Store merged repository to the cloud when different, otherwise spare the slow upload
                if (!RepositoriesAreEqual(mergedRepository, cloudRepository))
                {
                    byte[] encryptedRepository = EncryptRepository(
                        mergedRepository, settings.TransferCode, cryptoRandomService, settings.SelectedEncryptionAlgorithm);

                    ICloudStorageClient cloudStorageClient = cloudStorageClientFactory.GetByKey(credentials.CloudStorageId);
                    await cloudStorageClient.UploadFileAsync(Config.RepositoryFileName, encryptedRepository, credentials);
                }

                return new StoryBoardStepResult(SynchronizationStoryStepId.StopAndShowRepository, languageService["sync_success"]);
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                return new StoryBoardStepResult(ex);
            }
        }

        private static bool RepositoriesAreEqual(NoteRepositoryModel repository1, NoteRepositoryModel repository2)
        {
            return repository1.GetModificationFingerprint() == repository2.GetModificationFingerprint();
        }
    }
}
