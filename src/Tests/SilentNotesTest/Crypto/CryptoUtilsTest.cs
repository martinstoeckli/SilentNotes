using System;
using NUnit.Framework;
using SilentNotes.Crypto;
using SilentNotes.Services;

namespace SilentNotesTest.Crypto
{
    [TestFixture]
    public class CryptoUtilsTest
    {
        [Test]
        public void GenerateRandomBase62StringGeneratesValidStrings()
        {
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService();

            // check if length always matches the required length
            for (int length = 0; length < 300; length++)
            {
                string randomString = CryptoUtils.GenerateRandomBase62String(length, randomGenerator);
                Assert.AreEqual(length, randomString.Length);
                Assert.IsTrue(IsInBase62Alphabet(randomString));
            }
        }

        [Test]
        public void TruncateKeyReturnsOriginalIfNotLonger()
        {
            byte[] key = new byte[] { 3, 4, 5 };
            byte[] truncatedKey;

            truncatedKey = CryptoUtils.TruncateKey(key, 3);
            Assert.AreSame(key, truncatedKey);

            truncatedKey = CryptoUtils.TruncateKey(key, 4);
            Assert.AreSame(key, truncatedKey);

            truncatedKey = CryptoUtils.TruncateKey(null, 2);
            Assert.IsNull(truncatedKey);
        }

        [Test]
        public void TruncateKeyCutsIfLonger()
        {
            byte[] key = new byte[] { 3, 4, 5 };
            byte[] truncatedKey;

            truncatedKey = CryptoUtils.TruncateKey(key, 2);
            Assert.AreEqual(new byte[] { 3, 4 }, truncatedKey);
        }

        private static bool IsInBase62Alphabet(string randomString)
        {
            foreach (char c in randomString)
            {
                if (!"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".Contains(c.ToString()))
                    return false;
            }
            return true;
        }
    }
}
