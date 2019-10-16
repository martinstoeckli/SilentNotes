using Moq;
using NUnit.Framework;
using SilentNotes.Crypto;
using SilentNotes.Crypto.SymmetricEncryption;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.StoryBoards;
using SilentNotes.StoryBoards.SynchronizationStory;
using SilentNotes.Workers;
using System.Xml.Linq;

namespace SilentNotesTest.StoryBoards.SynchronizationStory
{
    [TestFixture]
    public class DecryptCloudRepositoryStepTest
    {
        [Test]
        public void SuccessfulFlowEndsInNextStep()
        {
            const string transferCode = "abcdefgh";
            var settingsModel = CreateSettingsModel(transferCode);
            byte[] encryptedRepository = CreateEncryptedRepository(transferCode);

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.LoadFromSession<byte[]>(It.Is<SynchronizationStorySessionKey>(p => p == SynchronizationStorySessionKey.BinaryCloudRepository))).
                Returns(encryptedRepository);
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<INoteRepositoryUpdater> updater = new Mock<INoteRepositoryUpdater>();

            // Run step
            var step = new DecryptCloudRepositoryStep(
                SynchronizationStoryStepId.DecryptCloudRepository,
                storyBoard.Object,
                CommonMocksAndStubs.LanguageService(),
                CommonMocksAndStubs.FeedbackService(),
                settingsService.Object,
                updater.Object);
            Assert.DoesNotThrowAsync(step.Run);

            // Repository has not changed and was not stored in session
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.IsNotNull<SettingsModel>()), Times.Never);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<SynchronizationStoryStepId>(x => x == SynchronizationStoryStepId.IsSameRepository)), Times.Once);
        }

        [Test]
        public void TransfercodeOfHistoryWillBeReused()
        {
            const string transferCode = "abcdefgh";
            var settingsModel = CreateSettingsModel("qqqqqqqq");
            settingsModel.TransferCodeHistory.Add(transferCode); // Good transfercode is in history
            byte[] encryptedRepository = CreateEncryptedRepository(transferCode);

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.LoadFromSession<byte[]>(It.Is<SynchronizationStorySessionKey>(p => p == SynchronizationStorySessionKey.BinaryCloudRepository))).
                Returns(encryptedRepository);
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<INoteRepositoryUpdater> updater = new Mock<INoteRepositoryUpdater>();

            // Run step
            var step = new DecryptCloudRepositoryStep(
                SynchronizationStoryStepId.DecryptCloudRepository,
                storyBoard.Object,
                CommonMocksAndStubs.LanguageService(),
                CommonMocksAndStubs.FeedbackService(),
                settingsService.Object,
                updater.Object);
            Assert.DoesNotThrowAsync(step.Run);

            // transfercode was moved from history to current and was stored
            Assert.AreEqual(transferCode, settingsModel.TransferCode);
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.IsNotNull<SettingsModel>()), Times.Once);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<SynchronizationStoryStepId>(x => x == SynchronizationStoryStepId.IsSameRepository)), Times.Once);
        }

        [Test]
        public void MissingOrWrongTransfercodeLeadsToInputDialog()
        {
            const string transferCode = "abcdefgh";
            var settingsModel = CreateSettingsModel(null); // no transfer code at all
            byte[] encryptedRepository = CreateEncryptedRepository(transferCode);

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.LoadFromSession<byte[]>(It.Is<SynchronizationStorySessionKey>(p => p == SynchronizationStorySessionKey.BinaryCloudRepository))).
                Returns(encryptedRepository);
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<INoteRepositoryUpdater> updater = new Mock<INoteRepositoryUpdater>();

            // Run step
            var step = new DecryptCloudRepositoryStep(
                SynchronizationStoryStepId.DecryptCloudRepository,
                storyBoard.Object,
                CommonMocksAndStubs.LanguageService(),
                CommonMocksAndStubs.FeedbackService(),
                settingsService.Object,
                updater.Object);
            Assert.DoesNotThrowAsync(step.Run);

            // No changes should be done to the settings
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.IsNotNull<SettingsModel>()), Times.Never);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<SynchronizationStoryStepId>(x => x == SynchronizationStoryStepId.ShowTransferCode)), Times.Once);

            // Run step with wrong transfer code
            settingsModel.TransferCode = "qqqqqqqq";
            Assert.DoesNotThrowAsync(step.Run);
            storyBoard.Verify(m => m.ContinueWith(It.Is<SynchronizationStoryStepId>(x => x == SynchronizationStoryStepId.ShowTransferCode)), Times.Exactly(2));
        }

        [Test]
        public void InvalidRepositoryLeadsToErrorMessage()
        {
            const string transferCode = "abcdefgh";
            var settingsModel = CreateSettingsModel(transferCode);
            byte[] encryptedRepository = CreateEncryptedRepository(transferCode);
            encryptedRepository[8]++; // make it invalid

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.LoadFromSession<byte[]>(It.Is<SynchronizationStorySessionKey>(p => p == SynchronizationStorySessionKey.BinaryCloudRepository))).
                Returns(encryptedRepository);
            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<INoteRepositoryUpdater> updater = new Mock<INoteRepositoryUpdater>();

            // Run step
            var step = new DecryptCloudRepositoryStep(
                SynchronizationStoryStepId.DecryptCloudRepository,
                storyBoard.Object,
                CommonMocksAndStubs.LanguageService(),
                feedbackService.Object,
                settingsService.Object,
                updater.Object);
            Assert.DoesNotThrowAsync(step.Run);

            // Error message shown
            feedbackService.Verify(m => m.ShowToast(It.IsAny<string>()), Times.Once);

            // No changes should be done to the settings
            settingsService.Verify(m => m.TrySaveSettingsToLocalDevice(It.IsNotNull<SettingsModel>()), Times.Never);

            // no next step is called
            storyBoard.Verify(m => m.ContinueWith(It.IsAny<SynchronizationStoryStepId>()), Times.Never);
        }

        [Test]
        public void RepositoryTooNewForApplicationLeadsToErrorMessage()
        {
            const string transferCode = "abcdefgh";
            var settingsModel = CreateSettingsModel(transferCode);
            byte[] encryptedRepository = CreateEncryptedRepository(transferCode);

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.LoadFromSession<byte[]>(It.Is<SynchronizationStorySessionKey>(p => p == SynchronizationStorySessionKey.BinaryCloudRepository))).
                Returns(encryptedRepository);
            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);
            Mock<INoteRepositoryUpdater> updater = new Mock<INoteRepositoryUpdater>();
            updater.
                Setup(m => m.IsTooNewForThisApp(It.IsAny<XDocument>())).
                Returns(true);

            // Run step
            var step = new DecryptCloudRepositoryStep(
                SynchronizationStoryStepId.DecryptCloudRepository,
                storyBoard.Object,
                CommonMocksAndStubs.LanguageService(),
                feedbackService.Object,
                settingsService.Object,
                updater.Object);
            Assert.DoesNotThrowAsync(step.Run);

            // Error message shown
            feedbackService.Verify(m => m.ShowToast(It.IsAny<string>()), Times.Once);

            // no next step is called
            storyBoard.Verify(m => m.ContinueWith(It.IsAny<SynchronizationStoryStepId>()), Times.Never);
        }

        private static SettingsModel CreateSettingsModel(string transferCode)
        {
            return new SettingsModel
            {
                TransferCode = transferCode,
                AdoptCloudEncryptionAlgorithm = false,
            };
        }

        private static byte[] CreateEncryptedRepository(string password, NoteRepositoryModel repository = null)
        {
            if (repository == null)
                repository = new NoteRepositoryModel();
            byte[] serializedRepository = XmlUtils.SerializeToXmlBytes(repository);
            EncryptorDecryptor encryptor = new EncryptorDecryptor("SilentNotes");
            return encryptor.Encrypt(serializedRepository, password, SilentNotes.Crypto.KeyDerivation.KeyDerivationCostType.Low, CommonMocksAndStubs.CryptoRandomService(), BouncyCastleTwofishGcm.CryptoAlgorithmName);
        }
    }
}
