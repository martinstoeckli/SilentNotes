// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Services.CloudStorageServices;
using SilentNotes.Workers;

namespace SilentNotes.StoryBoards.SynchronizationStory
{
    /// <summary>
    /// This step is an end point of the <see cref="SynchronizationStoryBoard"/>. It merges the
    /// local repository with the downloaded repository and stores the merged repository.
    /// </summary>
    public class StoreMergedRepositoryAndQuitStep : StoryBoardStepBase
    {
        private readonly ILanguageService _languageService;
        private readonly IFeedbackService _feedbackService;
        private readonly ISettingsService _settingsService;
        private readonly ICryptoRandomService _cryptoRandomService;
        private readonly IRepositoryStorageService _repositoryStorageService;
        private readonly ICloudStorageServiceFactory _cloudStorageServiceFactory;

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
            ICloudStorageServiceFactory cloudStorageServiceFactory)
            : base(stepId, storyBoard)
        {
            _languageService = languageService;
            _feedbackService = feedbackService;
            _settingsService = settingsService;
            _cryptoRandomService = randomService;
            _repositoryStorageService = repositoryStorageService;
            _cloudStorageServiceFactory = cloudStorageServiceFactory;
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            try
            {
                NoteRepositoryModel cloudRepository = StoryBoard.LoadFromSession<NoteRepositoryModel>(SynchronizationStorySessionKey.CloudRepository.ToInt());
                CloudStorageAccount account = StoryBoard.LoadFromSession<CloudStorageAccount>(SynchronizationStorySessionKey.CloudStorageAccount.ToInt());
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
                    byte[] encryptedRepository = SynchronizationStoryBoard.EncryptRepository(
                        mergedRepository, settings.TransferCode, _cryptoRandomService, settings.SelectedEncryptionAlgorithm);

                    ICloudStorageService cloudStorageService = _cloudStorageServiceFactory.Create(account);
                    await cloudStorageService.UploadRepositoryAsync(encryptedRepository);
                }

                await StoryBoard.ContinueWith(SynchronizationStoryStepId.StopAndShowRepository.ToInt());
                _feedbackService.ShowToast(_languageService["sync_success"]);
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                SynchronizationStoryBoard.ShowExceptionMessage(ex, _feedbackService, _languageService);
            }
        }

        private bool RepositoriesAreEqual(NoteRepositoryModel repository1, NoteRepositoryModel repository2)
        {
            byte[] serialized1 = XmlUtils.SerializeToXmlBytes(repository1);
            byte[] serialized2 = XmlUtils.SerializeToXmlBytes(repository2);

            if (serialized1.Length != serialized2.Length)
                return false;

            int length = serialized1.Length;
            for (int i = 0; i < length; i++)
            {
                if (serialized1[i] != serialized2[i])
                    return false;
            }
            return true;
        }
    }
}
