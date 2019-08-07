// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
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
        private readonly ILanguageService _languageService;
        private readonly IFeedbackService _feedbackService;
        private readonly ISettingsService _settingsService;
        private readonly ICryptoRandomService _cryptoRandomService;
        private readonly IRepositoryStorageService _repositoryStorageService;
        private readonly ICloudStorageClientFactory _cloudStorageClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreMergedRepositoryAndQuitStep"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public StoreMergedRepositoryAndQuitStep(
            int stepId,
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
            try
            {
                NoteRepositoryModel cloudRepository = StoryBoard.LoadFromSession<NoteRepositoryModel>(SynchronizationStorySessionKey.CloudRepository.ToInt());
                SerializeableCloudStorageCredentials credentials = StoryBoard.LoadFromSession<SerializeableCloudStorageCredentials>(SynchronizationStorySessionKey.CloudStorageCredentials.ToInt());
                _repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
                SettingsModel settings = _settingsService.LoadSettingsOrDefault();

                // Merge repositories
                NoteRepositoryMerger merger = new NoteRepositoryMerger();
                NoteRepositoryModel mergedRepository = merger.Merge(localRepository, cloudRepository);

                // Store merged repository locally when different
                if (!RepositoriesAreEqual(mergedRepository, localRepository))
                {
                    _repositoryStorageService.TrySaveRepository(mergedRepository);
                }

                // Store merged repository to the cloud when different, otherwise spare the slow upload
                if (!RepositoriesAreEqual(mergedRepository, cloudRepository))
                {
                    byte[] encryptedRepository = EncryptRepository(
                        mergedRepository, settings.TransferCode, _cryptoRandomService, settings.SelectedEncryptionAlgorithm);

                    ICloudStorageClient cloudStorageClient = _cloudStorageClientFactory.GetOrCreate(credentials.CloudStorageId);
                    await cloudStorageClient.UploadFileAsync(Config.RepositoryFileName, encryptedRepository, credentials);
                }

                await StoryBoard.ContinueWith(SynchronizationStoryStepId.StopAndShowRepository.ToInt());
                _feedbackService.ShowToast(_languageService["sync_success"]);
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                ShowExceptionMessage(ex, _feedbackService, _languageService);
            }
        }

        private bool RepositoriesAreEqual(NoteRepositoryModel repository1, NoteRepositoryModel repository2)
        {
            return repository1.GetModificationFingerprint() == repository2.GetModificationFingerprint();
        }
    }
}
