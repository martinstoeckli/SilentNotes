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
                Setup(m => m.LoadFromSession<NoteRepositoryModel>(It.Is<int>(p => p == SynchronizationStorySessionKey.CloudRepository.ToInt()))).
                Returns(repositoryModel);
            Mock<ILanguageService> languageService = new Mock<ILanguageService>();
            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();

            // Run step
            var step = new StoreCloudRepositoryToDeviceAndQuitStep(
                SynchronizationStoryStepId.StoreCloudRepositoryToDeviceAndQuit.ToInt(), storyBoard.Object, languageService.Object, feedbackService.Object, repositoryStorageService.Object);
            Assert.DoesNotThrowAsync(step.Run);

            // repository is stored to the local device
            repositoryStorageService.Verify(m => m.TrySaveRepository(It.Is<NoteRepositoryModel>(r => r == repositoryModel)), Times.Once);

            // Next step is called
            storyBoard.Verify(m => m.ContinueWith(It.Is<int>(x => x == SynchronizationStoryStepId.StopAndShowRepository.ToInt())), Times.Once);
        }
    }
}
