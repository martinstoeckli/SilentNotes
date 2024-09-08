using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Crypto;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestClass]
    public class ByteArrayExtensionsTest
    {
        [TestMethod]
        public void ByteArrayContainsAtWorksCorrectlyAtStart()
        {
            byte[] haystack = new byte[] { 1, 2, 3, 4 };
            byte[] needle = new byte[] { 1, 2 };
            Assert.IsTrue(haystack.ContainsAt(needle, 0));

            haystack = new byte[] { 1, 2 };
            needle = new byte[] { 1, 2 };
            Assert.IsTrue(haystack.ContainsAt(needle, 0));

            haystack = new byte[] { 1, 2, 3, 4 };
            needle = new byte[] { 2 };
            Assert.IsFalse(haystack.ContainsAt(needle, 0));

            haystack = new byte[] { 1, 2 };
            needle = new byte[] { 1, 2, 3, 4 };
            Assert.IsFalse(haystack.ContainsAt(needle, 0));
        }

        [TestMethod]
        public void ByteArrayContainsAtWorksCorrectly()
        {
            byte[] haystack = new byte[] { 1, 2, 3, 4 };
            byte[] needle = new byte[] { 2, 3 };
            Assert.IsTrue(haystack.ContainsAt(needle, 1));

            haystack = new byte[] { 1, 2, 3, 4 };
            needle = new byte[] { 3, 4 };
            Assert.IsTrue(haystack.ContainsAt(needle, 2));

            haystack = new byte[] { 1, 2, 3, 4 };
            needle = new byte[] { 2 };
            Assert.IsFalse(haystack.ContainsAt(needle, 3));

            haystack = new byte[] { 1, 2 };
            needle = new byte[] { 1, 2 };
            Assert.IsFalse(haystack.ContainsAt(needle, 1));
        }

        [TestMethod]
        public void ByteArrayStartsWithHandlesNullAndEmpty()
        {
            byte[] haystack = null;
            byte[] needle = null;
            Assert.ThrowsException<ArgumentNullException>(delegate { ByteArrayExtensions.ContainsAt(haystack, needle, 0); });

            // Works the same way as string.StartsWith
            bool expectedResult = string.Empty.StartsWith(string.Empty);
            haystack = new byte[0];
            needle = new byte[0];
            Assert.AreEqual(expectedResult, haystack.ContainsAt(needle, 0));
            Assert.IsTrue(expectedResult);

            expectedResult = "test".StartsWith(string.Empty);
            haystack = new byte[] { 1, 2, 3 };
            needle = new byte[0];
            Assert.AreEqual(expectedResult, haystack.ContainsAt(needle, 0));
            Assert.IsTrue(expectedResult);
        }

        [TestMethod]
        public void ContainsDigitAtWorksCorrectly()
        {
            byte[] array = CryptoUtils.StringToBytes("1-2z3");
            Assert.IsTrue(array.ContainsDigitCharAt(0));
            Assert.IsFalse(array.ContainsDigitCharAt(1));
            Assert.IsTrue(array.ContainsDigitCharAt(2));
            Assert.IsFalse(array.ContainsDigitCharAt(3));
            Assert.IsTrue(array.ContainsDigitCharAt(4));
            Assert.IsFalse(array.ContainsDigitCharAt(5));
        }
    }
}
