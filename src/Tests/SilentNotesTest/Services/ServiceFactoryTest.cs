using System;
using NUnit.Framework;
using SilentNotes.Services;

namespace SilentNotesTest.Services
{
    [TestFixture]
    public class ServiceFactoryTest
    {
        [Test]
        public void CreatesNewInstancesEachTimeIfNotSingleton()
        {
            var factory = new ServiceFactory<string, Candidate>(false); // not singleton services
            factory.Add("id", () => new Candidate());

            Candidate candidate1 = factory.GetByKey("id");
            Candidate candidate2 = factory.GetByKey("id");

            Assert.AreNotSame(candidate1, candidate2);
        }

        [Test]
        public void KeepsNewInstancesIfSingleton()
        {
            var factory = new ServiceFactory<string, Candidate>(true); // singleton services
            factory.Add("id", () => new Candidate());

            Candidate candidate1 = factory.GetByKey("id");
            Candidate candidate2 = factory.GetByKey("id");

            Assert.AreSame(candidate1, candidate2);
        }

        [Test]
        public void FindsCorrectId()
        {
            var factory = new ServiceFactory<string, Candidate>(true);
            factory.Add("id1", () => new Candidate { Id = 1 });
            factory.Add("id2", () => new Candidate { Id = 2 });

            Candidate candidate1 = factory.GetByKey("id1");
            Candidate candidate2 = factory.GetByKey("ID2");

            Assert.AreNotSame(candidate1, candidate2);
            Assert.AreEqual(1, candidate1.Id);
            Assert.AreEqual(2, candidate2.Id);
        }

        [Test]
        public void ThrowsForUnknownId()
        {
            var factory = new ServiceFactory<string, Candidate>(true);
            factory.Add("id1", () => new Candidate { Id = 1 });

            Assert.Throws<ArgumentOutOfRangeException>(() => factory.GetByKey("id2"));
        }

        private class Candidate
        {
            public int Id { get; set; }
        }
    }
}
