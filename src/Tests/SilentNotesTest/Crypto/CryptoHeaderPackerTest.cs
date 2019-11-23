using NUnit.Framework;
using SilentNotes.Crypto;
using SilentNotes.Services;

namespace SilentNotesTest.Crypto
{
    [TestFixture]
    public class CryptoHeaderPackerTest
    {
        [Test]
        public void PackingAndUnpackingResultsInOriginal()
        {
            CryptoHeader header = new CryptoHeader
            {
                AppName = "MyAppName",
                AlgorithmName = "Twofish",
                KdfName = "PBKDF2",
                Cost = "10",
                Nonce = CommonMocksAndStubs.FilledByteArray(8, 66),
                Salt = CommonMocksAndStubs.FilledByteArray(8, 77),
            };
            byte[] cipher = CommonMocksAndStubs.FilledByteArray(16, 88);

            byte[] packedHeader = CryptoHeaderPacker.PackHeaderAndCypher(header, cipher);
            CryptoHeaderPacker.UnpackHeaderAndCipher(packedHeader, out CryptoHeader unpackedHeader, out byte[] unpackedCipher);

            Assert.AreEqual(header.AppName, unpackedHeader.AppName);
            Assert.AreEqual(header.AlgorithmName, unpackedHeader.AlgorithmName);
            Assert.AreEqual(header.KdfName, unpackedHeader.KdfName);
            Assert.AreEqual(header.Cost, unpackedHeader.Cost);
            Assert.AreEqual(header.Nonce, unpackedHeader.Nonce);
            Assert.AreEqual(header.Salt, unpackedHeader.Salt);
            Assert.AreEqual(cipher, unpackedCipher);
        }

        [Test]
        public void EnsureLongTimeUnpacking()
        {
            // Ensure that a once stored packed header can always be unpacked even after changes in the liberary
            string packedBase64Header = "TXlBcHBOYW1lJFR3b2Zpc2gkUWtKQ1FrSkNRa0k9JFBCS0RGMiRUVTFOVFUxTlRVMD0kMTAkWFhYWFhYWFhYWFhYWFhYWA==";
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService(88);

            byte[] packedHeader = CryptoUtils.Base64StringToBytes(packedBase64Header);
            CryptoHeaderPacker.UnpackHeaderAndCipher(packedHeader, out CryptoHeader unpackedHeader, out byte[] unpackedCipher);

            Assert.AreEqual("MyAppName", unpackedHeader.AppName);
            Assert.AreEqual("Twofish", unpackedHeader.AlgorithmName);
            Assert.AreEqual("PBKDF2", unpackedHeader.KdfName);
            Assert.AreEqual("10", unpackedHeader.Cost);
            Assert.AreEqual(CommonMocksAndStubs.FilledByteArray(8, 66), unpackedHeader.Nonce);
            Assert.AreEqual(CommonMocksAndStubs.FilledByteArray(8, 77), unpackedHeader.Salt);
            Assert.AreEqual(CommonMocksAndStubs.FilledByteArray(16, 88), unpackedCipher);
        }
    }
}
