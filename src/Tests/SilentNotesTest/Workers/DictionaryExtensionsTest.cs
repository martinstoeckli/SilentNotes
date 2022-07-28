using System;
using System.Collections.Generic;
using NUnit.Framework;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestFixture]
    public class DictionaryExtensionsTest
    {
        [Test]
        public void TryGetKey_FindsKeyOfExistingValue()
        {
            var dictionary = new Dictionary<int, int> { { 1, 11 }, { 2, 22 } };
            bool res = DictionaryExtensions.TryGetKey(dictionary, 11, out int key);
            Assert.IsTrue(res);
            Assert.AreEqual(1, key);
            res = DictionaryExtensions.TryGetKey(dictionary, 22, out key);
            Assert.IsTrue(res);
            Assert.AreEqual(2, key);
        }

        [Test]
        public void TryGetKey_DoesNotFindKeyOfNonExistingValue()
        {
            var dictionary = new Dictionary<int, int> { { 1, 11 }, { 2, 22 } };
            bool res = DictionaryExtensions.TryGetKey(dictionary, 33, out int key);
            Assert.IsFalse(res);
            Assert.AreEqual(0, key); // default value
        }

        [Test]
        public void TryGetKey_WorksWithNullValues()
        {
            var dictionary = new Dictionary<string, string> { { "1", "11" }, { "2", null } };
            bool res = DictionaryExtensions.TryGetKey(dictionary, null, out string key);
            Assert.IsTrue(res);
            Assert.AreEqual("2", key);
        }

        [Test]
        public void TryGetKey_WorksWithComparer()
        {
            // Uses the case insensitive comparer
            var dictionary = new Dictionary<string, string> { { "1", "AA" }, { "2", "BB" } };
            bool res = DictionaryExtensions.TryGetKey(dictionary, "bb", out string key, StringComparer.InvariantCultureIgnoreCase);
            Assert.IsTrue(res);
            Assert.AreEqual("2", key);
        }
    }
}
