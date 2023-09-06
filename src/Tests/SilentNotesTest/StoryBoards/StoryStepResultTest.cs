using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SilentNotes.Stories;

namespace SilentNotesTest.Stories
{
    [TestFixture]
    public class StoryStepResultTest
    {
        [Test]
        public void IsNextStep_ComparesCorrectly()
        {
            StoryStepResult<TestModel> res = new StoryStepResult<TestModel>(new TestStep());
            Assert.IsTrue(res.NextStepIs(typeof(TestStep)));
            Assert.IsFalse(res.NextStepIs(typeof(int)));
        }

        [Test]
        public void IsNextStep_HandlesNullCorrectly()
        {
            StoryStepResult<TestModel> res = new StoryStepResult<TestModel>(new TestStep());
            Assert.IsFalse(res.NextStepIs(null)); // only parameter is null

            res = new StoryStepResult<TestModel>(null);
            Assert.IsTrue(res.NextStepIs(null)); // both are null
            Assert.IsFalse(res.NextStepIs(typeof(TestStep))); // only NextStepId is null
        }

        public class TestModel
        {
        }

        private class TestStep : StoryStepBase<TestModel>
        {
            public override ValueTask<StoryStepResult<TestModel>> RunStep(TestModel model, IServiceProvider serviceProvider, StoryMode uiMode)
            {
                return ValueTask.FromResult<StoryStepResult<TestModel>>(null);
            }

            protected override string TranslateException(Exception ex, IServiceProvider serviceProvider)
            {
                return ex.Message;
            }
        }
    }
}
