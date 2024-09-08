using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes;
using SilentNotes.Services;
using SilentNotes.Stories;
using SilentNotes.Stories.SynchronizationStory;

namespace SilentNotesTest.Stories.SynchronizationStory
{
    [TestClass]
    public class StopAndShowRepositoryStepTest
    {
        [TestMethod]
        public async ValueTask RunStep_NavigatesHome_WhenInUiMode()
        {
            var synchronizationService = new Mock<ISynchronizationService>();
            var navigationService = new Mock<INavigationService>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ISynchronizationService>(synchronizationService.Object);
            serviceCollection.AddSingleton<INavigationService>(navigationService.Object);

            var model = new SynchronizationStoryModel { StoryMode = StoryMode.Dialogs };
            var res = await new StopAndShowRepositoryStep().RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // Story does not continue
            Assert.IsNull(res.NextStep);
            navigationService.Verify(m => m.NavigateTo(It.Is<string>(r => r == RouteNames.NoteRepository), It.IsAny<bool>()), Times.Once);
            synchronizationService.Verify(m => m.FinishedManualSynchronization(It.IsAny<IServiceProvider>()), Times.Once);
        }

        [TestMethod]
        public async ValueTask RunStep_DoesNotNavigate_WhenInSilentMode()
        {
            var synchronizationService = new Mock<ISynchronizationService>();
            var navigationService = new Mock<INavigationService>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ISynchronizationService>(synchronizationService.Object);
            serviceCollection.AddSingleton<INavigationService>(navigationService.Object);

            var model = new SynchronizationStoryModel { StoryMode = StoryMode.Silent };
            var res = await new StopAndShowRepositoryStep().RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // Story does not continue
            Assert.IsNull(res.NextStep);
            navigationService.Verify(m => m.NavigateTo(It.Is<string>(r => r == RouteNames.NoteRepository), It.IsAny<bool>()), Times.Never);
            synchronizationService.Verify(m => m.FinishedManualSynchronization(It.IsAny<IServiceProvider>()), Times.Never);
        }
    }
}
