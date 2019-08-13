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
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public StoreLocalRepositoryToCloudAndQuitStep(
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
                SerializeableCloudStorageCredentials credentials = StoryBoard.LoadFromSession<SerializeableCloudStorageCredentials>(SynchronizationStorySessionKey.CloudStorageCredentials.ToInt());
                _repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);
                SettingsModel settings = _settingsService.LoadSettingsOrDefault();
                string transferCode = settings.TransferCode;

                bool needsNewTransferCode = !TransferCode.IsCodeSet(transferCode);
                if (needsNewTransferCode)
                    transferCode = TransferCode.GenerateCode(_cryptoRandomService);

                byte[] encryptedRepository = EncryptRepository(
                    localRepository, transferCode, _cryptoRandomService, settings.SelectedEncryptionAlgorithm);

                ICloudStorageClient cloudStorageClient = _cloudStorageClientFactory.GetOrCreate(credentials.CloudStorageId);
                await cloudStorageClient.UploadFileAsync(Config.RepositoryFileName, encryptedRepository, credentials);

                // All went well, time to save the transfer code, if a new one was created
                if (needsNewTransferCode)
                {
                    settings.TransferCode = transferCode;
                    _settingsService.TrySaveSettingsToLocalDevice(settings);

                    string formattedTransferCode = TransferCode.FormatTransferCodeForDisplay(transferCode).Replace(' ', '-');
                    string messageNewCreated = _languageService.LoadTextFmt("transfer_code_created", formattedTransferCode);
                    string messageWriteDown = _languageService.LoadText("transfer_code_writedown");
                    if (StoryBoard.Mode.ShouldUseGui())
                        await _feedbackService.ShowMessageAsync(messageNewCreated + Environment.NewLine + messageWriteDown, null);
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
    }
}
