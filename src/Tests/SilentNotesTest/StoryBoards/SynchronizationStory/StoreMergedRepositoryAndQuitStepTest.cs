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
    public class StoreMergedRepositoryAndQuitStepTest
    {
        [TestMethod]
        public async Task DoNotStoreAnythingWhenRepositoriesAreSame()
        {
            NoteRepositoryModel repositoryModel = new NoteRepositoryModel();
            repositoryModel.Revision = NoteRepositoryModel.CurrentSavingRevision;
            var model = new SynchronizationStoryModel
            {
                StoryMode = StoryMode.Silent,
                CloudRepository = repositoryModel
            };

            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repositoryModel)); // same as from storyBoard
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(CommonMocksAndStubs.SettingsService(null))
                .AddSingleton<IRepositoryStorageService>(repositoryStorageService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService())
                .AddSingleton<ICryptoRandomService>(CommonMocksAndStubs.CryptoRandomService())
                .AddSingleton<ICloudStorageClient>(cloudStorageClient.Object);

            // Run step
            var step = new StoreMergedRepositoryAndQuitStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // repository is not stored to the local device
            repositoryStorageService.Verify(m => m.TrySaveRepository(It.IsAny<NoteRepositoryModel>()), Times.Never);

            // repository is not stored to the cloud
            cloudStorageClient.Verify(m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CloudStorageCredentials>()), Times.Never);

            // Next step is called
            Assert.IsInstanceOfType<StopAndShowRepositoryStep>(result.NextStep);
        }

        [TestMethod]
        public async Task StoreMergedRepositoryWhenDifferent()
        {
            const string transferCode = "abcdefgh";
            SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials();
            var settingsModel = CreateSettingsModel(transferCode);
            NoteRepositoryModel repositoryModelLocal = new NoteRepositoryModel();
            repositoryModelLocal.Notes.Add(new NoteModel());
            NoteRepositoryModel repositoryModelCloud = new NoteRepositoryModel();
            repositoryModelCloud.Notes.Add(new NoteModel());
            var model = new SynchronizationStoryModel
            {
                StoryMode = StoryMode.Silent,
                CloudRepository = repositoryModelCloud,
                Credentials = credentialsFromSession,
            };

            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repositoryModelLocal));
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(CommonMocksAndStubs.SettingsService(settingsModel))
                .AddSingleton<IRepositoryStorageService>(repositoryStorageService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService())
                .AddSingleton<ICryptoRandomService>(CommonMocksAndStubs.CryptoRandomService())
                .AddSingleton<ICloudStorageClientFactory>(CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));

            // Run step
            var step = new StoreMergedRepositoryAndQuitStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // repository is stored to the local device
            repositoryStorageService.Verify(m => m.TrySaveRepository(It.IsAny<NoteRepositoryModel>()), Times.Once);

            // repository is stored to the cloud
            cloudStorageClient.Verify(m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CloudStorageCredentials>()), Times.Once);

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
