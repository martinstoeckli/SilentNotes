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
    /// This step is an end point of the <see cref="SynchronizationStoryBoard"/>. It keeps the
    /// local repository and stores it to the cloud.
    /// </summary>
    public class StoreLocalRepositoryToCloudAndQuitStep : StoryBoardStepBase
    {
        private readonly ILanguageService _languageService;
        private readonly IFeedbackService _feedbackService;
        private readonly ISettingsService _settingsService;
        private readonly ICryptoRandomService _cryptoRandomService;
        private readonly IRepositoryStorageService _repositoryStorageService;
        private readonly ICloudStorageServiceFactory _cloudStorageServiceFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreLocalRepositoryToCloudAndQuitStep"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public StoreLocalRepositoryToCloudAndQuitStep(
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
                CloudStorageAccount account = StoryBoard.LoadFromSession<CloudStorageAccount>(SynchronizationStorySessionKey.CloudStorageAccount.ToInt());
                _repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
                SettingsModel settings = _settingsService.LoadSettingsOrDefault();
                string transferCode = settings.TransferCode;

                bool needsNewTransferCode = !TransferCode.IsCodeSet(transferCode);
                if (needsNewTransferCode)
                    transferCode = TransferCode.GenerateCode(_cryptoRandomService);

                byte[] encryptedRepository = SynchronizationStoryBoard.EncryptRepository(
                    localRepository, transferCode, _cryptoRandomService, settings.SelectedEncryptionAlgorithm);

                ICloudStorageService cloudStorageService = _cloudStorageServiceFactory.Create(account);
                await cloudStorageService.UploadRepositoryAsync(encryptedRepository);

                // All went well, time to save the transfer code, if a new one was created
                if (needsNewTransferCode)
                {
                    settings.TransferCode = transferCode;
                    _settingsService.TrySaveSettingsToLocalDevice(settings);

                    string formattedTransferCode = TransferCode.FormatTransferCodeForDisplay(transferCode).Replace(' ', '-');
                    string messageNewCreated = _languageService.LoadTextFmt("transfer_code_created", formattedTransferCode);
                    string messageWriteDown = _languageService.LoadText("transfer_code_writedown");
                    await _feedbackService.ShowMessageAsync(messageNewCreated + Environment.NewLine + messageWriteDown, null);
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
    }
}
