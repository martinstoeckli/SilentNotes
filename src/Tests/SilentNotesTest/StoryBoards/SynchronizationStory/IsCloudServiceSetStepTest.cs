using Moq;
using NUnit.Framework;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.StoryBoards;
using SilentNotes.StoryBoards.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotesTest.StoryBoards.SynchronizationStory
{
    [TestFixture]
    public class IsCloudServiceSetStepTest
    {
        [Test]
        public void RunSilent_GoesToShowFirstTimeDialog_WithoutCloudStorageClientSet()
        {
            IStoryBoardSession session = new StoryBoardSession();

            var settingsModel = new SettingsModel();
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);

            var res = IsCloudServiceSetStep.RunSilent(session, settingsService.Object);

            // Continue with correct next step to ask for missing credentials
            Assert.IsTrue(res.NextStepIs(SynchronizationStoryStepId.ShowFirstTimeDialog));
            Assert.IsFalse(session.TryLoad(SynchronizationStorySessionKey.CloudStorageCredentials, out SerializeableCloudStorageCredentials _));
        }

        [Test]
        public void RunSilent_GoesToExistsCloudRepository_WithCloudStorageClientSet()
        {
            IStoryBoardSession session = new StoryBoardSession();

            var credentials = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdWebdav };
            var settingsModel = new SettingsModel { Credentials = credentials };
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settingsModel);

            StoryBoardStepResult res = IsCloudServiceSetStep.RunSilent(session, settingsService.Object);

            // Continue with correct next step with credentials set
            Assert.IsTrue(res.NextStepIs(SynchronizationStoryStepId.ExistsCloudRepository));
            Assert.AreEqual(credentials, session.Load<SerializeableCloudStorageCredentials>(SynchronizationStorySessionKey.CloudStorageCredentials));
        }
    }
}
