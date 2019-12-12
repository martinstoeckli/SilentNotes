using NUnit.Framework;
using SilentNotes;
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
                Assert.IsTrue(UnmixableAlphabet.IsOfCorrectAlphabet(code));
            }
        }
    }
}
