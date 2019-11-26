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
