using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Crypto.SymmetricEncryption;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Stories;
using SilentNotes.Stories.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotesTest.Stories.SynchronizationStory
{
    [TestClass]
    public class StoreLocalRepositoryToCloudAndQuitStepTest
    {
        [TestMethod]
        public async Task GenerateAndStoreNewTransfercode()
        {
            SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials();
            var settingsModel = CreateSettingsModel(null); // Transfercode does not yet exist
            NoteRepositoryModel repositoryModel = new NoteRepositoryModel();
            var model = new SynchronizationStoryModel
            {
                StoryMode = StoryMode.Silent,
                CloudRepository = repositoryModel,
                Credentials = credentialsFromSession,
            };

            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repositoryModel));
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(settingsService.Object)
                .AddSingleton<IRepositoryStorageService>(repositoryStorageService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService())
                .AddSingleton<ICryptoRandomService>(CommonMocksAndStubs.CryptoRandomService())
                .AddSingleton<ICloudStorageClientFactory>(CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));

            // Run step
            var step = new StoreLocalRepositoryToCloudAndQuitStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // Settings are stored with new transfer code
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.Is<SettingsModel>(s => !string.IsNullOrEmpty(s.TransferCode))), Times.Once);

            // Repository was uploaded
            cloudStorageClient.Verify(m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.Is<CloudStorageCredentials>(c => c == credentialsFromSession)), Times.Once);

            // Notification trigger was added to settings
            Assert.AreEqual(1, settingsModel.NotificationTriggers.Count);

            // Next step is called
            Assert.IsInstanceOfType<StopAndShowRepositoryStep>(result.NextStep);
        }

        [TestMethod]
        public async Task KeepExistingTransfercode()
        {
            SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials();
            var settingsModel = CreateSettingsModel("abcdefgh"); // Transfercode exists
            NoteRepositoryModel repositoryModel = new NoteRepositoryModel();
            var model = new SynchronizationStoryModel
            {
                StoryMode = StoryMode.Silent,
                CloudRepository = repositoryModel,
                Credentials = credentialsFromSession,
            };

            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repositoryModel));

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(settingsService.Object)
                .AddSingleton<IRepositoryStorageService>(repositoryStorageService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService())
                .AddSingleton<ICryptoRandomService>(CommonMocksAndStubs.CryptoRandomService())
                .AddSingleton<ICloudStorageClientFactory>(CommonMocksAndStubs.CloudStorageClientFactory());

            // Run step
            var step = new StoreLocalRepositoryToCloudAndQuitStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // No settings are stored
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.IsAny<SettingsModel>()), Times.Never);

            // Next step is called
            Assert.IsInstanceOfType<StopAndShowRepositoryStep>(result.NextStep);
        }

        private static SettingsModel CreateSettingsModel(string transferCode)
        {
            return new SettingsModel
            {
                TransferCode = transferCode,
                SelectedEncryptionAlgorithm = BouncyCastleTwofishGcm.CryptoAlgorithmName,
            };
        }
    }
}
