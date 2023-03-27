using System;
using Moq;
using NUnit.Framework;
using SilentNotes.Services;
using SilentNotes.StoryBoards;

namespace SilentNotesTest.StoryBoards
{
    [TestFixture]
    public class StoryBoardTest
    {
        [Test]
        public void StartRunsTheFirstStep()
        {
            Mock<IStoryBoardStep> step1 = new Mock<IStoryBoardStep>();
            Mock<IStoryBoardStep> step2 = new Mock<IStoryBoardStep>();

            IStoryBoard board = new StoryBoardBase();
            board.RegisterStep(step1.Object);
            board.RegisterStep(step2.Object);

            board.Start();

            step1.Verify(x => x.Run(), Times.Once);
            step2.Verify(x => x.Run(), Times.Never);
        }

        [Test]
        public void ContinuesWithRunsCorrectStep()
        {
            Mock<IStoryBoardStep> step1 = new Mock<IStoryBoardStep>();
            step1.SetupGet(x => x.Id).Returns(StepId.Step1);
            Mock<IStoryBoardStep> step2 = new Mock<IStoryBoardStep>();
            step2.SetupGet(x => x.Id).Returns(StepId.Step2);

            IStoryBoard board = new StoryBoardBase();
            board.RegisterStep(step1.Object);
            board.RegisterStep(step2.Object);

            board.ContinueWith(StepId.Step2);

            step1.Verify(x => x.Run(), Times.Never);
            step2.Verify(x => x.Run(), Times.Once);
        }

        [Test]
        public void ContinuesWithNullDoesntThrow()
        {
            Mock<IStoryBoardStep> step1 = new Mock<IStoryBoardStep>();
            step1.SetupGet(x => x.Id).Returns(StepId.Step1);

            IStoryBoard board = new StoryBoardBase();
            board.RegisterStep(step1.Object);

            Assert.DoesNotThrowAsync(() => board.ContinueWith(null));
        }

        [Test]
        public void LoadingFromSessionRequiresCorrectType()
        {
            IStoryBoard board = new StoryBoardBase();
            board.Session.Store(SessionId.Key1, "Caramel");

            // Correctly read a string from the session
            bool res = board.Session.TryLoad(SessionId.Key1, out string stringValue);
            Assert.IsTrue(res);
            Assert.AreEqual("Caramel", stringValue);

            // Wrongly read an int from the session
            res = board.Session.TryLoad(SessionId.Key1, out int intValue);
            Assert.IsFalse(res);
        }

        [Test]
        public void RemoveFromSessionRemovesExistingKey()
        {
            IStoryBoard board = new StoryBoardBase();
            board.Session.Store(SessionId.Key1, "Caramel");
            bool result = board.Session.TryLoad(SessionId.Key1, out string value);
            Assert.IsTrue(result);
            Assert.AreEqual("Caramel", value);

            board.Session.Remove(SessionId.Key1);
            result = board.Session.TryLoad(SessionId.Key1, out value);
            Assert.IsFalse(result);
        }

        [Test]
        public void WrongKeyDoesNotFindAnything()
        {
            IStoryBoard board = new StoryBoardBase();
            board.Session.Store(SessionId.Key1, "Caramel");

            // Correctly read a string from the session
            bool res = board.Session.TryLoad(SessionId.Key2, out string _);
            Assert.IsFalse(res);

            res = board.Session.TryLoad(StepId.Step1, out string _);
            Assert.IsFalse(res);
        }

        [Test]
        public void RemoveFromSessionDoesNotThrowForUnexistingKeys()
        {
            IStoryBoard board = new StoryBoardBase();
            board.Session.Store(SessionId.Key1, "Caramel");

            Assert.DoesNotThrow(() => board.Session.Remove(SessionId.Key2));
        }

        [Test]
        public void ShowFeedbackShowsError()
        {
            StoryBoardStepResult result = new StoryBoardStepResult(new Exception("test"));
            IStoryBoard board = new StoryBoardBase();

            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();

            board.ShowFeedback(result, feedbackService.Object, CommonMocksAndStubs.LanguageService());
            feedbackService.Verify(m => m.ShowToast(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public void ShowFeedbackShowsMessage()
        {
            StoryBoardStepResult result = new StoryBoardStepResult(null, null, "test");
            IStoryBoard board = new StoryBoardBase();

            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();

            board.ShowFeedback(result, feedbackService.Object, CommonMocksAndStubs.LanguageService());
            feedbackService.Verify(m => m.ShowMessageAsync(It.Is<string>(v => v == "test"), It.IsAny<string>(), It.IsAny<MessageBoxButtons>(), It.IsAny<bool>()), Times.Once);
        }

        [Test]
        public void ShowFeedbackShowsToast()
        {
            StoryBoardStepResult result = new StoryBoardStepResult(null, "test", null);
            IStoryBoard board = new StoryBoardBase();

            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();

            board.ShowFeedback(result, feedbackService.Object, CommonMocksAndStubs.LanguageService());
            feedbackService.Verify(m => m.ShowToast(It.Is<string>(v => v == "test")), Times.Once);
        }

        private enum StepId
        {
            Step1,
            Step2,
        }

        private enum SessionId
        {
            Key1,
            Key2,
        }
    }
}
