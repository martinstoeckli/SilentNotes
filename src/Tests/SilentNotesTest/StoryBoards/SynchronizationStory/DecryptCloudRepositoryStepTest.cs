using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SilentNotes.Crypto;
using SilentNotes.Crypto.SymmetricEncryption;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Stories;
using SilentNotes.Stories.SynchronizationStory;
using SilentNotes.Workers;
using System.Xml.Linq;
using VanillaCloudStorageClient;

namespace SilentNotesTest.Stories.SynchronizationStory
{
    [TestFixture]
    public class DecryptCloudRepositoryStepTest
    {
        [Test]
        public async Task SuccessfulFlowEndsInNextStep()
        {
            const string transferCode = "abcdefgh";
            var settingsModel = CreateSettingsModel(transferCode);
            byte[] encryptedRepository = CreateEncryptedRepository(transferCode);
            var model = new SynchronizationStoryModel
            {
                StoryMode = StoryMode.Silent,
                BinaryCloudRepository = encryptedRepository,
            };

            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(settingsService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService())
                .AddSingleton<INoteRepositoryUpdater>(new Mock<INoteRepositoryUpdater>().Object);

            // Run step
            var step = new DecryptCloudRepositoryStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // Repository has not changed and was not stored in session
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.IsNotNull<SettingsModel>()), Times.Never);

            // Next step is called
            Assert.IsInstanceOf<IsSameRepositoryStep>(result.NextStep);
        }

        [Test]
        public async Task TransfercodeOfHistoryWillBeReused()
        {
            const string transferCode = "abcdefgh";
            var settingsModel = CreateSettingsModel("qqqqqqqq");
            settingsModel.TransferCodeHistory.Add(transferCode); // Good transfercode is in history
            byte[] encryptedRepository = CreateEncryptedRepository(transferCode);
            var model = new SynchronizationStoryModel
            {
                StoryMode = StoryMode.Silent,
                BinaryCloudRepository = encryptedRepository,
            };

            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(settingsService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService())
                .AddSingleton<INoteRepositoryUpdater>(new Mock<INoteRepositoryUpdater>().Object);

            // Run step
            var step = new DecryptCloudRepositoryStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // transfercode was moved from history to current and was stored
            Assert.AreEqual(transferCode, settingsModel.TransferCode);
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.IsNotNull<SettingsModel>()), Times.Once);

            // Next step is called
            Assert.IsInstanceOf<IsSameRepositoryStep>(result.NextStep);
        }

        [Test]
        public async Task MissingOrWrongTransfercodeLeadsToInputDialog()
        {
            const string transferCode = "abcdefgh";
            var settingsModel = CreateSettingsModel(null); // no transfer code at all
            byte[] encryptedRepository = CreateEncryptedRepository(transferCode);
            var model = new SynchronizationStoryModel
            {
                StoryMode = StoryMode.Silent,
                BinaryCloudRepository = encryptedRepository,
            };

            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(settingsService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService())
                .AddSingleton<INoteRepositoryUpdater>(new Mock<INoteRepositoryUpdater>().Object);

            // Run step
            var step = new DecryptCloudRepositoryStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // No changes should be done to the settings
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.IsNotNull<SettingsModel>()), Times.Never);

            // Next step is called
            Assert.IsInstanceOf<ShowTransferCodeStep>(result.NextStep);

            // Run step with wrong transfer code
            settingsModel.TransferCode = "qqqqqqqq";
            result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);
            Assert.IsInstanceOf<ShowTransferCodeStep>(result.NextStep);
        }

        [Test]
        public async Task BusyIndicatorIsStopped()
        {
            const string transferCode = "abcdefgh";
            var settingsModel = CreateSettingsModel(null); // no transfer code at all
            byte[] encryptedRepository = CreateEncryptedRepository(transferCode);
            var model = new SynchronizationStoryModel
            {
                StoryMode = StoryMode.BusyIndicator,
                UserEnteredTransferCode = "wrong",
                BinaryCloudRepository = encryptedRepository,
            };

            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(settingsService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService())
                .AddSingleton<INoteRepositoryUpdater>(new Mock<INoteRepositoryUpdater>().Object)
                .AddSingleton<IFeedbackService>(feedbackService.Object);

            // Run step
            var step = new DecryptCloudRepositoryStep();

            // If user entered wrong code, page should be kept open and busy indicator stopped
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);
            Assert.IsNull(result.NextStep);
            feedbackService.Verify(m => m.SetBusyIndicatorVisible(It.Is<bool>(visible => visible == false), It.IsAny<bool>()), Times.Once);
        }


        [Test]
        public async Task InvalidRepositoryLeadsToErrorMessage()
        {
            const string transferCode = "abcdefgh";
            var settingsModel = CreateSettingsModel(transferCode);
            byte[] encryptedRepository = CreateEncryptedRepository(transferCode);
            encryptedRepository[8]++; // make it invalid
            var model = new SynchronizationStoryModel
            {
                StoryMode = StoryMode.Silent,
                BinaryCloudRepository = encryptedRepository,
            };

            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(settingsService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService())
                .AddSingleton<INoteRepositoryUpdater>(new Mock<INoteRepositoryUpdater>().Object);

            // Run step
            var step = new DecryptCloudRepositoryStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            Assert.IsNotNull(result.Error); // Error message shown
            Assert.IsNull(result.NextStep); // no next step is called

            // No changes should be done to the settings
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.IsNotNull<SettingsModel>()), Times.Never);
        }

        [Test]
        public async Task RepositoryTooNewForApplicationLeadsToErrorMessage()
        {
            const string transferCode = "abcdefgh";
            var settingsModel = CreateSettingsModel(transferCode);
            byte[] encryptedRepository = CreateEncryptedRepository(transferCode);
            var model = new SynchronizationStoryModel
            {
                StoryMode = StoryMode.Silent,
                BinaryCloudRepository = encryptedRepository,
            };

            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<INoteRepositoryUpdater> updater = new Mock<INoteRepositoryUpdater>();
            updater.
                Setup(m => m.IsTooNewForThisApp(It.IsAny<XDocument>())).
                Returns(true);

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(settingsService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService())
                .AddSingleton<INoteRepositoryUpdater>(updater.Object);

            // Run step
            var step = new DecryptCloudRepositoryStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            Assert.IsNotNull(result.Error); // Error message shown
            Assert.IsInstanceOf<SynchronizationStoryExceptions.UnsuportedRepositoryRevisionException>(result.Error);
            Assert.IsNull(result.NextStep); // no next step is called
        }

        private static SettingsModel CreateSettingsModel(string transferCode)
        {
            return new SettingsModel
            {
                TransferCode = transferCode,
            };
        }

        private static byte[] CreateEncryptedRepository(string password, NoteRepositoryModel repository = null)
        {
            if (repository == null)
                repository = new NoteRepositoryModel();
            byte[] serializedRepository = XmlUtils.SerializeToXmlBytes(repository);
            ICryptor encryptor = new Cryptor("SilentNotes", CommonMocksAndStubs.CryptoRandomService());
            return encryptor.Encrypt(serializedRepository, CryptoUtils.StringToSecureString(password), SilentNotes.Crypto.KeyDerivation.KeyDerivationCostType.Low, BouncyCastleTwofishGcm.CryptoAlgorithmName);
        }
    }
}
