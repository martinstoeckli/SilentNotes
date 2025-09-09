using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestClass]
    public class CompressUtilsTest
    {
        [TestMethod]
        public void CompressReturnsData()
        {
            byte[] data = CommonMocksAndStubs.FilledByteArray(64, 88);
            byte[] compressedData = CompressUtils.Compress(data);
            Assert.IsTrue(compressedData.Length > 3);
        }

        [TestMethod]
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
            CollectionAssert.AreEqual(data, decompressedData);
        }

        [TestMethod]
        public void CompressAndDecompressReturnsOriginalData()
        {
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService();
            int byteCount = 1;
            while (byteCount < 1000)
            {
                byte[] data = randomGenerator.GetRandomBytes(byteCount);
                byte[] compressedData = CompressUtils.Compress(data);
                byte[] decompressedData = CompressUtils.Decompress(compressedData);

                CollectionAssert.AreEqual(data, decompressedData);
                byteCount = byteCount * 2;
            }
        }

        [TestMethod]
        public void CreateZipArchive_CanBeReadBack()
        {
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService();
            var file0 = new CompressUtils.CompressEntry { Name = "FileName1", Data = randomGenerator.GetRandomBytes(8) };
            var file1 = new CompressUtils.CompressEntry { Name = "FileName2 with blanks and 🐧", Data = randomGenerator.GetRandomBytes(64) };

            byte[] zipArchiveContent = CompressUtils.CreateZipArchive(new[] { file0, file1 });
            var zipArchiveEntries = CompressUtils.OpenZipArchive(zipArchiveContent);

            Assert.AreEqual(2, zipArchiveEntries.Count);
            Assert.AreEqual(file0.Name, zipArchiveEntries[0].Name);
            Assert.IsTrue(Enumerable.SequenceEqual(file0.Data, zipArchiveEntries[0].Data));
            Assert.AreEqual(file1.Name, zipArchiveEntries[1].Name);
            Assert.IsTrue(Enumerable.SequenceEqual(file1.Data, zipArchiveEntries[1].Data));
        }

        [TestMethod]
        public void CreateZipArchive_WorksWithNullBytes()
        {
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService();
            var file0 = new CompressUtils.CompressEntry { Name = "a", Data = new byte[0] }; // content with zero length, empty
            var file1 = new CompressUtils.CompressEntry { Name = "b", Data = new byte[] { 0, 55, 0 } }; // content with 0-bytes

            byte[] zipArchiveContent = CompressUtils.CreateZipArchive(new[] { file0, file1 });
            var zipArchiveEntries = CompressUtils.OpenZipArchive(zipArchiveContent);

            Assert.AreEqual(2, zipArchiveEntries.Count);
            Assert.AreEqual(file0.Name, zipArchiveEntries[0].Name);
            Assert.IsTrue(Enumerable.SequenceEqual(file0.Data, zipArchiveEntries[0].Data));
            Assert.AreEqual(file1.Name, zipArchiveEntries[1].Name);
            Assert.IsTrue(Enumerable.SequenceEqual(file1.Data, zipArchiveEntries[1].Data));
        }
    }
}
