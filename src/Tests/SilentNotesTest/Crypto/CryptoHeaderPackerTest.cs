using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Crypto;

namespace SilentNotesTest.Crypto
{
    [TestClass]
    public class CryptoHeaderPackerTest
    {
        [TestMethod]
        public void PackingAndUnpackingResultsInOriginal()
        {
            CryptoHeader header = new CryptoHeader
            {
                PackageName = "MyAppName",
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

            Assert.AreEqual(header.PackageName, unpackedHeader.PackageName);
            Assert.AreEqual(header.AlgorithmName, unpackedHeader.AlgorithmName);
            Assert.AreEqual(header.KdfName, unpackedHeader.KdfName);
            Assert.AreEqual(header.Cost, unpackedHeader.Cost);
            CollectionAssert.AreEqual(header.Nonce, unpackedHeader.Nonce);
            CollectionAssert.AreEqual(header.Salt, unpackedHeader.Salt);
            CollectionAssert.AreEqual(cipher, unpackedCipher);
            Assert.AreEqual(header.Compression, unpackedHeader.Compression);
        }

        [TestMethod]
        public void PackingAndUnpackingWorksWithKeyOnly()
        {
            // If the encryption is started with a key instead of a password, there are not all parametes set.
            CryptoHeader header = new CryptoHeader
            {
                PackageName = "MyAppName",
                AlgorithmName = "Twofish",
                KdfName = null,
                Cost = null,
                Nonce = CommonMocksAndStubs.FilledByteArray(8, 66),
                Salt = null,
                Compression = "gzip",
            };
            byte[] cipher = CommonMocksAndStubs.FilledByteArray(16, 88);

            byte[] packedHeader = CryptoHeaderPacker.PackHeaderAndCypher(header, cipher);
            CryptoHeaderPacker.UnpackHeaderAndCipher(packedHeader, "MyAppName", out CryptoHeader unpackedHeader, out byte[] unpackedCipher);

            Assert.AreEqual(header.PackageName, unpackedHeader.PackageName);
            Assert.AreEqual(header.AlgorithmName, unpackedHeader.AlgorithmName);
            Assert.AreEqual(header.KdfName, unpackedHeader.KdfName);
            Assert.AreEqual(header.Cost, unpackedHeader.Cost);
            CollectionAssert.AreEqual(header.Nonce, unpackedHeader.Nonce);
            CollectionAssert.AreEqual(header.Salt, unpackedHeader.Salt);
            CollectionAssert.AreEqual(cipher, unpackedCipher);
            Assert.AreEqual(header.Compression, unpackedHeader.Compression);
        }

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        public void HasMatchingHeaderHandlesNullAndEmpty()
        {
            byte[] array = null;
            Assert.IsFalse(CryptoHeaderPacker.HasMatchingHeader(array, "MyAppName", out _));

            array = new byte[0];
            Assert.IsFalse(CryptoHeaderPacker.HasMatchingHeader(array, "MyAppName", out _));
        }

        [TestMethod]
        public void EnsureBackwardsCompatibility()
        {
            // Ensure that a once stored packed header can always be unpacked even after changes in the liberary
            string packedBase64Header = "TXlBcHBOYW1lJFR3b2Zpc2gkUWtKQ1FrSkNRa0k9JFBCS0RGMiRUVTFOVFUxTlRVMD0kMTAkWFhYWFhYWFhYWFhYWFhYWA==";
            byte[] packedHeader = CryptoUtils.Base64StringToBytes(packedBase64Header);
            CryptoHeaderPacker.UnpackHeaderAndCipher(packedHeader, "MyAppName", out CryptoHeader unpackedHeader, out byte[] unpackedCipher);

            Assert.AreEqual("MyAppName", unpackedHeader.PackageName);
            Assert.AreEqual("Twofish", unpackedHeader.AlgorithmName);
            Assert.AreEqual("PBKDF2", unpackedHeader.KdfName);
            Assert.AreEqual("10", unpackedHeader.Cost);
            CollectionAssert.AreEqual(CommonMocksAndStubs.FilledByteArray(8, 66), unpackedHeader.Nonce);
            CollectionAssert.AreEqual(CommonMocksAndStubs.FilledByteArray(8, 77), unpackedHeader.Salt);
            CollectionAssert.AreEqual(CommonMocksAndStubs.FilledByteArray(16, 88), unpackedCipher);
        }
    }
}
