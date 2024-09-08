using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestClass]
    public class TransferCodeTest
    {
        [TestMethod]
        public void TransferCodeIsOfCorrectLength()
        {
            for (int length = 0; length < 999; length++)
            {
                string code = TransferCode.GenerateCode(length, CommonMocksAndStubs.CryptoRandomService());
                Assert.AreEqual(length, code.Length);
            }
        }

        [TestMethod]
        public void TransferCodeIsOfCorrectAlphabet()
        {
            for (int length = 0; length < 999; length++)
            {
                string code = TransferCode.GenerateCode(length, CommonMocksAndStubs.CryptoRandomService());
                Assert.IsTrue(UnmixableAlphabet.IsOfCorrectAlphabet(code));
            }
        }

        [TestMethod]
        public void TrySanitizeUserInputAcceptsValidCodes()
        {
            string sanitizedCode;
            Assert.IsTrue(TransferCode.TrySanitizeUserInput("8AerUwv22345hkpM", out sanitizedCode));
            Assert.AreEqual("8AerUwv22345hkpM", sanitizedCode);

            Assert.IsTrue(TransferCode.TrySanitizeUserInput("8Aer-Uwv2-2345-hkpM", out sanitizedCode));
            Assert.AreEqual("8AerUwv22345hkpM", sanitizedCode);

            Assert.IsTrue(TransferCode.TrySanitizeUserInput("8Aer Uwv2 2345   hkpM", out sanitizedCode));
            Assert.AreEqual("8AerUwv22345hkpM", sanitizedCode);
        }

        [TestMethod]
        public void TrySanitizeUserInputRejectsInvalidCodes()
        {
            // Invalid alphabet
            Assert.IsFalse(TransferCode.TrySanitizeUserInput("IAerUwv22345hkpM", out _));

            // Invalid length
            Assert.IsFalse(TransferCode.TrySanitizeUserInput("8AerUwv22345hkp", out _));
            Assert.IsFalse(TransferCode.TrySanitizeUserInput("8AerUwv22345hkpMS", out _));
            Assert.IsFalse(TransferCode.TrySanitizeUserInput("8Aer        hkpM", out _));

            // null and empty
            Assert.IsFalse(TransferCode.TrySanitizeUserInput(string.Empty, out _));
            Assert.IsFalse(TransferCode.TrySanitizeUserInput(null, out _));
            Assert.IsFalse(TransferCode.TrySanitizeUserInput("                ", out _));
        }
    }
}
