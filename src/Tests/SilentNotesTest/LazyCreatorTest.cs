using System;
using System.Collections.Generic;
using NUnit.Framework;
using SilentNotes;

namespace SilentNotesTest
{
    [TestFixture]
    class LazyCreatorTest
    {
        [Test]
        public void LazyCreatorCreatesNewInstanceWhenMemberIsNull()
        {
            List<int> memberVariable = null;
            var result = LazyCreator.GetOrCreate(ref memberVariable);
            Assert.IsNotNull(result);
            Assert.AreSame(memberVariable, result);
        }

        [Test]
        public void LazyCreatorReturnsInstanceWhenMemberExists()
        {
            List<int> memberVariable = new List<int>();
            var result = LazyCreator.GetOrCreate(ref memberVariable);
            Assert.AreSame(memberVariable, result);
        }

        [Test]
        public void LazyCreatorCallsCreatorMethod()
        {
            bool wasCalled = false;
            Dictionary<string, string> returnedDictionary = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            Dictionary<string, string> memberVariable = null;

            var result = LazyCreator.GetOrCreate(
                ref memberVariable,
                () => {
                    wasCalled = true;
                    return returnedDictionary;
                });
            Assert.True(wasCalled);
            Assert.AreSame(memberVariable, returnedDictionary);
            Assert.AreSame(memberVariable, result);
        }
    }
}
