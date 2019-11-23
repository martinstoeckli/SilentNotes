using NUnit.Framework;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestFixture]
    public class CompressUtilsTest
    {
        [Test]
        public void CompressReturnsData()
        {
            byte[] data = CommonMocksAndStubs.FilledByteArray(64, 88);
            byte[] compressedData = CompressUtils.Compress(data);
            Assert.IsTrue(compressedData.Length > 3);
        }

        [Test]
        public void CompressHandlesNullData()
        {
            // Null data
            byte[] data = null;
            byte[] compressedData = CompressUtils.Compress(data);
            Assert.IsNull(compressedData);

            // Empty data
            data = new byte[0];
            compressedData = CompressUtils.Compress(data);
            byte[] decompressedData = CompressUtils.Decompress(compressedData);
            Assert.IsNotNull(compressedData);
            Assert.AreEqual(0, compressedData.Length);
            Assert.IsNotNull(decompressedData);
            Assert.AreEqual(data, decompressedData);
        }

        [Test]
        public void CompressAndDecompressReturnsOriginalData()
        {
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService();
            int byteCount = 1;
            while (byteCount < 1000)
            {
                byte[] data = randomGenerator.GetRandomBytes(byteCount);
                byte[] compressedData = CompressUtils.Compress(data);
                byte[] decompressedData = CompressUtils.Decompress(compressedData);

                Assert.AreEqual(data, decompressedData);
                byteCount = byteCount * 2;
            }
        }
    }
}
