﻿using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Services;
using SilentNotes.Stories;
using SilentNotes.Stories.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotesTest.Stories.SynchronizationStory
{
    [TestClass]
    public class DownloadCloudRepositoryStepTest
    {
        [TestMethod]
        public async Task SuccessfulFlowEndsInNextStep()
        {
            SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials();
            byte[] repositoryFromSession = null;
            byte[] repository = new byte[8];
            var model = new SynchronizationStoryModel
            {
                StoryMode = StoryMode.Silent,
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
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // Repository was stored in session
            Assert.AreEqual(repository, model.BinaryCloudRepository);

            // Next step is called
            Assert.IsInstanceOfType<ExistsTransferCodeStep>(result.NextStep);
        }

        [TestMethod]
        public async Task ErrorMessageIsShownInCaseOfException()
        {
            SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials();
            byte[] repositoryFromSession = null;
            var model = new SynchronizationStoryModel
            {
                StoryMode = StoryMode.Silent,
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
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            //StoryBoardStepResult result = await DownloadCloudRepositoryStep.RunSilent(session, CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));
            Assert.IsNotNull(result.Error); // Error message is shown
            Assert.IsNull(result.NextStep); // Next step is not called
        }
    }
}
