using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using SilentNotes.Services;

namespace SilentNotesTest.Services
{
    [TestFixture]
    public class ScopedServiceProviderTest
    {
        [Test]
        public void Get_FindsLatestRegisteredService()
        {
            IScopedServiceProvider<TestCandidate> provider = new ScopedServiceProvider<TestCandidate>();
            Guid owner = Guid.NewGuid();
            var firstCandidate = new TestCandidate();
            var lastCandidate = new TestCandidate();
            provider.Register(owner, firstCandidate);
            provider.Register(owner, lastCandidate);

            Assert.AreSame(lastCandidate, provider.Get());
        }

        [Test]
        public void Get_ReturnsNullIfNoServiceRegistered()
        {
            IScopedServiceProvider<TestCandidate> provider = new ScopedServiceProvider<TestCandidate>();
            Assert.IsNull(provider.Get());
        }

        [Test]
        public void Unregister_RemovesRegisteredServices()
        {
            IScopedServiceProvider<TestCandidate> provider = new ScopedServiceProvider<TestCandidate>();
            Guid owner = Guid.NewGuid();
            var firstCandidate = new TestCandidate();
            var lastCandidate = new TestCandidate();
            provider.Register(owner, firstCandidate);
            provider.Register(owner, lastCandidate);
            provider.Unregister(owner);
            Assert.IsNull(provider.Get());
        }

        [Test]
        public void Unregister_RemovesOnlyServicesOfOwner()
        {
            IScopedServiceProvider<TestCandidate> provider = new ScopedServiceProvider<TestCandidate>();
            Guid owner1 = Guid.NewGuid();
            var owner1Candidate = new TestCandidate();
            Guid owner2 = Guid.NewGuid();
            var owner2Candidate = new TestCandidate();
            provider.Register(owner1, owner1Candidate);
            provider.Register(owner2, owner2Candidate);
            provider.Unregister(owner1);
            Assert.AreSame(owner2Candidate, provider.Get());
        }

        private class TestCandidate
        {
        }
    }
}
