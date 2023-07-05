using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
//using SilentNotes.StoryBoards;
//using SilentNotes.StoryBoards.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotesTest.StoryBoards.SynchronizationStory
{
    [TestFixture]
    public class DownloadCloudRepositoryStepTest
    {
        // todo: reactivate tests
        //[Test]
        //public async Task SuccessfulFlowEndsInNextStep()
        //{
        //    SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials();
        //    byte[] repositoryFromSession = null;
        //    byte[] repository = new byte[8];

        //    StoryBoardSession session = new StoryBoardSession();
        //    session.Store(SynchronizationStorySessionKey.CloudStorageCredentials, credentialsFromSession);
        //    session.Store(SynchronizationStorySessionKey.BinaryCloudRepository, repositoryFromSession);

        //    Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();
        //    cloudStorageClient.
        //        Setup(m => m.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CloudStorageCredentials>())).
        //        ReturnsAsync(repository);

        //    // Run step
        //    StoryBoardStepResult result = await DownloadCloudRepositoryStep.RunSilent(session, CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));

        //    // Repository was stored in session
        //    Assert.AreEqual(repository, session.Load<byte[]>(SynchronizationStorySessionKey.BinaryCloudRepository));

        //    // Next step is called
        //    Assert.AreEqual(SynchronizationStoryStepId.ExistsTransferCode, result.NextStepId);
        //}

        //[Test]
        //public async Task ErrorMessageIsShownInCaseOfException()
        //{
        //    SerializeableCloudStorageCredentials credentialsFromSession = new SerializeableCloudStorageCredentials();
        //    byte[] repositoryFromSession = null;

        //    StoryBoardSession session = new StoryBoardSession();
        //    session.Store(SynchronizationStorySessionKey.CloudStorageCredentials, credentialsFromSession);
        //    session.Store(SynchronizationStorySessionKey.BinaryCloudRepository, repositoryFromSession);

        //    Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();
        //    cloudStorageClient.
        //        Setup(m => m.DownloadFileAsync(It.IsAny<string>(), It.IsAny<CloudStorageCredentials>())).
        //        Throws<ConnectionFailedException>();

        //    // Run step
        //    StoryBoardStepResult result = await DownloadCloudRepositoryStep.RunSilent(session, CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));
        //    Assert.IsNotNull(result.Error); // Error message is shown
        //    Assert.IsNull(result.NextStepId); // Next step is not called
        //}
    }
}
