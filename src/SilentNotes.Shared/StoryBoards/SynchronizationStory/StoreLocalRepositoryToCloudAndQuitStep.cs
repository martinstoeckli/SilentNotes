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
    /// This step is an end point of the <see cref="SynchronizationStoryBoard"/>. It keeps the
    /// local repository and stores it to the cloud.
    /// </summary>
    public class StoreLocalRepositoryToCloudAndQuitStep : SynchronizationStoryBoardStepBase
    {
        private readonly ILanguageService _languageService;
        private readonly IFeedbackService _feedbackService;
        private readonly ISettingsService _settingsService;
        private readonly ICryptoRandomService _cryptoRandomService;
        private readonly IRepositoryStorageService _repositoryStorageService;
        private readonly ICloudStorageClientFactory _cloudStorageClientFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreLocalRepositoryToCloudAndQuitStep"/> class.
        /// </summary>
        public StoreLocalRepositoryToCloudAndQuitStep(
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
                SerializeableCloudStorageCredentials credentials = session.Load<SerializeableCloudStorageCredentials>(SynchronizationStorySessionKey.CloudStorageCredentials);
                repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
                SettingsModel settings = settingsService.LoadSettingsOrDefault();
                string transferCode = settings.TransferCode;

                bool needsNewTransferCode = !TransferCode.IsCodeSet(transferCode);
                if (needsNewTransferCode)
                    transferCode = TransferCode.GenerateCode(cryptoRandomService);

                byte[] encryptedRepository = EncryptRepository(
                    localRepository, transferCode, cryptoRandomService, settings.SelectedEncryptionAlgorithm);

                ICloudStorageClient cloudStorageClient = cloudStorageClientFactory.GetByKey(credentials.CloudStorageId);
                await cloudStorageClient.UploadFileAsync(Config.RepositoryFileName, encryptedRepository, credentials);

                // All went well, time to save the transfer code, if a new one was created
                string message = null;
                if (needsNewTransferCode)
                {
                    settings.TransferCode = transferCode;
                    settingsService.TrySaveSettingsToLocalDevice(settings);

                    string formattedTransferCode = TransferCode.FormatTransferCodeForDisplay(transferCode);
                    string messageNewCreated = languageService.LoadTextFmt("transfer_code_created", formattedTransferCode);
                    string messageWriteDown = languageService.LoadText("transfer_code_writedown");
                    message = messageNewCreated + Environment.NewLine + messageWriteDown;
                }

                return new StoryBoardStepResult(SynchronizationStoryStepId.StopAndShowRepository, languageService["sync_success"], message);
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                return new StoryBoardStepResult(ex);
            }
        }
    }
}
