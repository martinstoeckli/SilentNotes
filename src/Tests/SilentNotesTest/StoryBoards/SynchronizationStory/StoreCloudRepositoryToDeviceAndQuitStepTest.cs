using Moq;
using NUnit.Framework;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.StoryBoards;
using SilentNotes.StoryBoards.SynchronizationStory;

namespace SilentNotesTest.StoryBoards.SynchronizationStory
{
    [TestFixture]
    public class StoreCloudRepositoryToDeviceAndQuitStepTest
    {
        [Test]
        public void StoresRepositoryToDevice()
        {
            NoteRepositoryModel repositoryModel = new NoteRepositoryModel();

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.Session.Load<NoteRepositoryModel>(It.Is<SynchronizationStorySessionKey>(p => p == SynchronizationStorySessionKey.CloudRepository))).
                Returns(repositoryModel);
            Mock<ILanguageService> languageService = new Mock<ILanguageService>();
            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();

            // Run step
            var step = new StoreCloudRepositoryToDeviceAndQuitStep(
                SynchronizationStoryStepId.StoreCloudRepositoryToDeviceAndQuit, storyBoard.Object, languageService.Object, feedbackService.Object, repositoryStorageService.Object);
            Assert.DoesNotThrowAsync(step.Run);

            // repository is stored to the local device
            repositoryStorageService.Verify(m => m.TrySaveRepository(It.Is<NoteRepositoryModel>(r => r == repositoryModel)), Times.Once);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<SynchronizationStoryStepId>(x => x == SynchronizationStoryStepId.StopAndShowRepository)), Times.Once);
        }
    }
}
