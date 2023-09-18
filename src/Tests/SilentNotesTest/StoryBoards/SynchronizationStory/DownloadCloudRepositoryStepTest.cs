using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SilentNotes.Services;
using SilentNotes.Stories;
using SilentNotes.Stories.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotesTest.Stories.SynchronizationStory
{
    [TestFixture]
    public class DownloadCloudRepositoryStepTest
    {
        [Test]
        public async Task SuccessfulFlowEndsInNextStep()
        {
            SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials();
            byte[] repositoryFromSession = null;
            byte[] repository = new byte[8];
            var model = new SynchronizationStoryModel
            {
                Credentials = credentialsFromSession,
                BinaryCloudRepository = repositoryFromSession,
            };

            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();
            cloudStorageClient.
                Setup(m => m.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CloudStorageCredentials>())).
                ReturnsAsync(repository);

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService())
                .AddSingleton<ICloudStorageClientFactory>(CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));

            // Run step
            var step = new DownloadCloudRepositoryStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), StoryMode.Silent);

            // Repository was stored in session
            Assert.AreEqual(repository, model.BinaryCloudRepository);

            // Next step is called
            Assert.IsInstanceOf<ExistsTransferCodeStep>(result.NextStep);
        }

        [Test]
        public async Task ErrorMessageIsShownInCaseOfException()
        {
            SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials();
            byte[] repositoryFromSession = null;
            var model = new SynchronizationStoryModel
            {
                Credentials = credentialsFromSession,
                BinaryCloudRepository = repositoryFromSession,
            };

            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();
            cloudStorageClient.
                Setup(m => m.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CloudStorageCredentials>())).
                Throws<ConnectionFailedException>();

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService())
                .AddSingleton<ICloudStorageClientFactory>(CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));

            // Run step
            var step = new DownloadCloudRepositoryStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), StoryMode.Silent);

            //StoryBoardStepResult result = await DownloadCloudRepositoryStep.RunSilent(session, CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));
            Assert.IsNotNull(result.Error); // Error message is shown
            Assert.IsNull(result.NextStep); // Next step is not called
        }
    }
}
