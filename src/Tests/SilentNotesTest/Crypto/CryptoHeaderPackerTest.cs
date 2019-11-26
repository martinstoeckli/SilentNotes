using NUnit.Framework;
using SilentNotes.Crypto;

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
                Compression = "gzip",
            };
            byte[] cipher = CommonMocksAndStubs.FilledByteArray(16, 88);

            byte[] packedHeader = CryptoHeaderPacker.PackHeaderAndCypher(header, cipher);
            CryptoHeaderPacker.UnpackHeaderAndCipher(packedHeader, "MyAppName", out CryptoHeader unpackedHeader, out byte[] unpackedCipher);

            Assert.AreEqual(header.AppName, unpackedHeader.AppName);
            Assert.AreEqual(header.AlgorithmName, unpackedHeader.AlgorithmName);
            Assert.AreEqual(header.KdfName, unpackedHeader.KdfName);
            Assert.AreEqual(header.Cost, unpackedHeader.Cost);
            Assert.AreEqual(header.Nonce, unpackedHeader.Nonce);
            Assert.AreEqual(header.Salt, unpackedHeader.Salt);
            Assert.AreEqual(cipher, unpackedCipher);
            Assert.AreEqual(header.Compression, unpackedHeader.Compression);
        }

        [Test]
        public void HasMatchingHeaderRecognizesValidHeader()
        {
            byte[] array = CryptoUtils.StringToBytes("MyAppName v=8$");
            int revision;
            Assert.IsTrue(CryptoHeaderPacker.HasMatchingHeader(array, "MyAppName", out revision));
            Assert.AreEqual(8, revision);

            array = CryptoUtils.StringToBytes("MyAppName v=88$1234");
            Assert.IsTrue(CryptoHeaderPacker.HasMatchingHeader(array, "MyAppName", out revision));
            Assert.AreEqual(88, revision);

            array = CryptoUtils.StringToBytes("MyAppName v=8");
            Assert.IsFalse(CryptoHeaderPacker.HasMatchingHeader(array, "MyAppName", out _));

            array = CryptoUtils.StringToBytes("MyAppName v=8e$");
            Assert.IsFalse(CryptoHeaderPacker.HasMatchingHeader(array, "MyAppName", out _));

            array = CryptoUtils.StringToBytes("YourAppName v=8$");
            Assert.IsFalse(CryptoHeaderPacker.HasMatchingHeader(array, "MyAppName", out _));
        }

        [Test]
        public void HasMatchingHeaderRecognizesOldHeaderWithoutRevision()
        {
            byte[] array = CryptoUtils.StringToBytes("MyAppName$");
            int revision;
            Assert.IsTrue(CryptoHeaderPacker.HasMatchingHeader(array, "MyAppName", out revision));
            Assert.AreEqual(1, revision);

            array = CryptoUtils.StringToBytes("MyAppName$1234");
            Assert.IsTrue(CryptoHeaderPacker.HasMatchingHeader(array, "MyAppName", out revision));
            Assert.AreEqual(1, revision);

            array = CryptoUtils.StringToBytes("MyAppName");
            Assert.IsFalse(CryptoHeaderPacker.HasMatchingHeader(array, "MyAppName", out _));

            array = CryptoUtils.StringToBytes("YourAppName$");
            Assert.IsFalse(CryptoHeaderPacker.HasMatchingHeader(array, "MyAppName", out _));
        }

        [Test]
        public void HasMatchingHeaderHandlesNullAndEmpty()
        {
            byte[] array = null;
            Assert.IsFalse(CryptoHeaderPacker.HasMatchingHeader(array, "MyAppName", out _));

            array = new byte[0];
            Assert.IsFalse(CryptoHeaderPacker.HasMatchingHeader(array, "MyAppName", out _));
        }

        [Test]
        public void EnsureBackwardsCompatibility()
        {
            // Ensure that a once stored packed header can always be unpacked even after changes in the liberary
            string packedBase64Header = "TXlBcHBOYW1lJFR3b2Zpc2gkUWtKQ1FrSkNRa0k9JFBCS0RGMiRUVTFOVFUxTlRVMD0kMTAkWFhYWFhYWFhYWFhYWFhYWA==";
            byte[] packedHeader = CryptoUtils.Base64StringToBytes(packedBase64Header);
            CryptoHeaderPacker.UnpackHeaderAndCipher(packedHeader, "MyAppName", out CryptoHeader unpackedHeader, out byte[] unpackedCipher);

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
