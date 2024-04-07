using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using NUnit.Framework;
using SilentNotes.Services;
using SilentNotes.Stories;

namespace SilentNotesTest.Stories
{
    [TestFixture]
    public class StoryStepBaseTest
    {
        [Test]
        public async Task RunStory_CallsRunStep()
        {
            var nextStep = new TestStoryStep(null, null, null);
            var storyStep = new TestStoryStep(nextStep, null, null);

            await storyStep.RunStoryAndShowLastFeedback(null, null, StoryMode.Toasts);
            Assert.AreEqual(1, storyStep.RunStepCount);
            Assert.AreEqual(1, nextStep.RunStepCount);
        }

        [Test]
        public async Task RunStory_ShowsToast()
        {
            var feedbackService = new Mock<IFeedbackService>();
            var services = new ServiceCollection()
                .AddSingleton<IFeedbackService>(feedbackService.Object);
            var storyStep = new TestStoryStep(null, "abc", null);

            await storyStep.RunStoryAndShowLastFeedback(null, services.BuildServiceProvider(), StoryMode.Silent);
            feedbackService.Verify(m => m.ShowToast(It.IsAny<string>(), It.IsAny<Severity>()), Times.Never);

            await storyStep.RunStoryAndShowLastFeedback(null, services.BuildServiceProvider(), StoryMode.Toasts);
            feedbackService.Verify(m => m.ShowToast(It.IsAny<string>(), It.IsAny<Severity>()), Times.Once);
        }

        [Test]
        public async Task RunStory_ShowsMessages()
        {
            var feedbackService = new Mock<IFeedbackService>();
            var services = new ServiceCollection()
                .AddSingleton<IFeedbackService>(feedbackService.Object);
            var storyStep = new TestStoryStep(null, null, "abc");

            await storyStep.RunStoryAndShowLastFeedback(null, services.BuildServiceProvider(), StoryMode.Silent);
            feedbackService.Verify(m => m.ShowMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButtons>(), It.IsAny<bool>()), Times.Never);

            await storyStep.RunStoryAndShowLastFeedback(null, services.BuildServiceProvider(), StoryMode.Toasts | StoryMode.Messages);
            feedbackService.Verify(m => m.ShowMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButtons>(), It.IsAny<bool>()), Times.Once);
        }
    }

    internal class TestStoryStep : StoryStepBase<TestModel>
    {
        private readonly IStoryStep<TestModel> _nextStep;
        private readonly string _toast;
        private readonly string _message;

        public TestStoryStep(IStoryStep<TestModel> nextStep, string toast, string message)
        {
            _nextStep = nextStep;
            _toast = toast;
            _message = message;
        }

        public override Task<StoryStepResult<TestModel>> RunStep(TestModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            RunStepCount += 1;
            return Task.FromResult(new StoryStepResult<TestModel>(_nextStep, _toast, _message));
        }

        protected override string TranslateException(Exception ex, IServiceProvider serviceProvider)
        {
            return ex.Message;
        }

        public int RunStepCount { get; private set; }
    }

    internal class TestModel
    {
    }
}
