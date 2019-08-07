using NUnit.Framework;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestFixture]
    public class TransferCodeTest
    {
        [Test]
        public void TransferCodeIsOfCorrectLength()
        {
            for (int length = 0; length < 999; length++)
            {
                string code = TransferCode.GenerateCode(length, CommonMocksAndStubs.CryptoRandomService());
                Assert.AreEqual(length, code.Length);
            }
        }

        [Test]
        public void TransferCodeIsOfCorrectAlphabet()
        {
            for (int length = 0; length < 999; length++)
            {
                string code = TransferCode.GenerateCode(length, CommonMocksAndStubs.CryptoRandomService());
                Assert.IsTrue(IsOfCorrectAlphabet(code));
            }
        }

        private bool IsOfCorrectAlphabet(string code)
        {
            foreach (var letter in code)
            {
                if ((letter < '1' || letter > '9') && (letter < 'a' || letter > 'z') || letter == 'l')
                    return false;
            }
            return true;
        }
    }
}
