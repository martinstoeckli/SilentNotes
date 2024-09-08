using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Services;

namespace SilentNotesTest.Services
{
    [TestClass]
    public class ServiceFactoryTest
    {
        [TestMethod]
        public void CreatesNewInstancesEachTimeIfNotSingleton()
        {
            var factory = new ServiceFactory<string, Candidate>(false); // not singleton services
            factory.Add("id", () => new Candidate());

            Candidate candidate1 = factory.GetByKey("id");
            Candidate candidate2 = factory.GetByKey("id");

            Assert.AreNotSame(candidate1, candidate2);
        }

        [TestMethod]
        public void KeepsNewInstancesIfSingleton()
        {
            var factory = new ServiceFactory<string, Candidate>(true); // singleton services
            factory.Add("id", () => new Candidate());

            Candidate candidate1 = factory.GetByKey("id");
            Candidate candidate2 = factory.GetByKey("id");

            Assert.AreSame(candidate1, candidate2);
        }

        [TestMethod]
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

        [TestMethod]
        public void ThrowsForUnknownId()
        {
            var factory = new ServiceFactory<string, Candidate>(true);
            factory.Add("id1", () => new Candidate { Id = 1 });

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => factory.GetByKey("id2"));
        }

        private class Candidate
        {
            public int Id { get; set; }
        }
    }
}
