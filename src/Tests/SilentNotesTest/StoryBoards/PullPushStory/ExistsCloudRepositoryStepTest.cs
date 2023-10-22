using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Stories;
using SilentNotes.Stories.PullPushStory;
using SilentNotes.Stories.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotesTest.Stories.PullPushStory
{
    [TestFixture]
    public class ExistsCloudRepositoryStepTest
    {
        [Test]
        public async Task CorrectNextStepWhenCloudRepositoryExists()
        {
            SerializeableCloudStorageCredentials credentials = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdDropbox };
            SettingsModel settingsModel = new SettingsModel { Credentials = credentials, TransferCode = "abc" };
            var model = new PullPushStoryModel(new Guid(), PullPushDirection.PullFromServer)
            {
                StoryMode = StoryMode.Toasts,
                Credentials = credentials,
            };

            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();
            cloudStorageClient.
                Setup(m => m.ExistsFileAsync(It.IsAny<string>(), It.IsAny<CloudStorageCredentials>())).
                ReturnsAsync(true);

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(settingsService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService())
                .AddSingleton<ICloudStorageClientFactory>(CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));

            // Run step
            var step = new SilentNotes.Stories.PullPushStory.ExistsCloudRepositoryStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // Settings are not stored because no token needs to be refreshed
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.Is<SettingsModel>(s => s.Credentials == credentials)), Times.Never);

            // Next step is called
            Assert.IsInstanceOf<SilentNotes.Stories.PullPushStory.DownloadCloudRepositoryStep>(result.NextStep);
        }

        [Test]
        public async Task QuitWhenMissingClientOrTransfercode()
        {
            SerializeableCloudStorageCredentials credentials = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdDropbox };
            SettingsModel settingsModel = new SettingsModel { Credentials = credentials };
            var model = new PullPushStoryModel(new Guid(), PullPushDirection.PullFromServer)
            {
                StoryMode = StoryMode.Toasts,
                Credentials = credentials,
            };

            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();
            cloudStorageClient.
                Setup(m => m.ExistsFileAsync(It.IsAny<string>(), It.IsAny<CloudStorageCredentials>())).
                ReturnsAsync(true);

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(settingsService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService("need_sync_first"))
                .AddSingleton<ICloudStorageClientFactory>(CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));

            // Run step with missing transfercode
            var step = new SilentNotes.Stories.PullPushStory.ExistsCloudRepositoryStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // Next step is not called
            Assert.IsNull(result.NextStep);
            Assert.AreEqual("need_sync_first", result.Toast);

            // Run step with missing storage client
            settingsModel.TransferCode = "abc";
            settingsModel.Credentials.CloudStorageId = null;
            step = new SilentNotes.Stories.PullPushStory.ExistsCloudRepositoryStep();
            result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // Next step is not called
            Assert.IsNull(result.NextStep);
            Assert.AreEqual("need_sync_first", result.Toast);
        }
    }
}
