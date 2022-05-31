using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestFixture]
    public class AtomicFileWriterTest
    {
        private const string _testFileName = "test.txt";
        private string _directoryPath;

        [SetUp]
        public void SetupTestDirectory()
        {
            _directoryPath = Path.Combine(Path.GetTempPath(), "unittest_" + Guid.NewGuid().ToString());
            Directory.CreateDirectory(_directoryPath);
        }

        [TearDown]
        public void RemoveTestDirectory()
        {
            Directory.Delete(_directoryPath, true);
        }

        [Test]
        public void FileHasNewContentAfterSuccessfulStorage()
        {
            string filePath = GetTestFilePath();
            byte[] newContent = new byte[] { 88 };

            var writer = new AtomicFileWriter();
            writer.Write(filePath, stream => stream.Write(newContent));

            Assert.IsTrue(File.Exists(filePath));
            byte[] readContent = File.ReadAllBytes(filePath);
            Assert.AreEqual(newContent, readContent);
            Assert.IsFalse(TempFileExists());
            Assert.IsFalse(ReadyFileExists());
        }

        [Test]
        public void ExistingFileCanBeOverwritten()
        {
            string filePath = GetTestFilePath();
            var oldContent = new byte[] { 77, 66 };
            File.WriteAllBytes(filePath, oldContent);

            var newContent = new byte[] { 88 };

            var writer = new AtomicFileWriter();
            writer.Write(filePath, stream => stream.Write(newContent));

            byte[] readContent = File.ReadAllBytes(filePath);
            Assert.AreEqual(newContent, readContent);
        }

        [Test]
        public void OriginalFileIsLeftIntactWhenWritingFails()
        {
            string filePath = GetTestFilePath();
            var oldContent = new byte[] { 77, 66 };
            File.WriteAllBytes(filePath, oldContent);

            var newContent = new byte[] { 88 };
            var writer = new AtomicFileWriter(
                new AtomicFileWriter.TestSimulation { SimulateWriteError = true });
            try
            {
                writer.Write(filePath, stream => stream.Write(newContent));
            }
            catch (Exception)
            {
            }

            byte[] readContent = File.ReadAllBytes(filePath);
            Assert.AreEqual(oldContent, readContent);
            Assert.IsFalse(ReadyFileExists());
        }

        [Test]
        public void NextStorageWorksAfterWritingFailed()
        {
            string filePath = GetTestFilePath();
            var newContent = new byte[] { 88 };
            var writer = new AtomicFileWriter(
                new AtomicFileWriter.TestSimulation { SimulateWriteError = true });
            try
            {
                writer.Write(filePath, stream => stream.Write(newContent));
            }
            catch (Exception)
            {
            }

            writer = new AtomicFileWriter();
            var newerContent = new byte[] { 99 };
            writer.Write(filePath, stream => stream.Write(newerContent));

            byte[] readContent = File.ReadAllBytes(filePath);
            Assert.AreEqual(newerContent, readContent);
            Assert.IsFalse(ReadyFileExists());
        }

        [Test]
        public void ReadyStateIsSetAfterReplacingFailed()
        {
            string filePath = GetTestFilePath();
            var newContent = new byte[] { 88 };
            var writer = new AtomicFileWriter(
                new AtomicFileWriter.TestSimulation { SimulateReplaceError = true });
            try
            {
                writer.Write(filePath, stream => stream.Write(newContent));
            }
            catch (Exception)
            {
            }

            Assert.IsTrue(ReadyFileExists());
            byte[] readContent = File.ReadAllBytes(GetTempFilePath());
            Assert.AreEqual(newContent, readContent);
        }

        [Test]
        public void OperationCanBeCompletedAfterReplacingFailed()
        {
            string filePath = GetTestFilePath();
            var newContent = new byte[] { 88 };
            var writer = new AtomicFileWriter(
                new AtomicFileWriter.TestSimulation { SimulateReplaceError = true });
            try
            {
                writer.Write(filePath, stream => stream.Write(newContent));
            }
            catch (Exception)
            {
            }

            writer = new AtomicFileWriter();
            writer.CompletePendingWrite(filePath);

            byte[] readContent = File.ReadAllBytes(filePath);
            Assert.AreEqual(newContent, readContent);
            Assert.IsFalse(ReadyFileExists());
            Assert.IsFalse(TempFileExists());
        }

        [Test]
        public void SubsequentWritingIsBlockedAfterReplacingFailed()
        {
            string filePath = GetTestFilePath();
            var newContent = new byte[] { 88 };
            var writer = new AtomicFileWriter(
                new AtomicFileWriter.TestSimulation { SimulateReplaceError = true });
            try
            {
                writer.Write(filePath, stream => stream.Write(newContent));
            }
            catch (Exception)
            {
            }

            writer = new AtomicFileWriter();
            var newerContent = new byte[] { 99 };
            Assert.Throws<UnfinishedAtomicFileWritingException>(() => writer.Write(filePath, stream => stream.Write(newerContent)));

            // The ready state and the content of the first writing must still be intact, so it can
            // be completed later on.
            Assert.IsTrue(ReadyFileExists());
            Assert.IsTrue(TempFileExists());
            byte[] readContent = File.ReadAllBytes(GetTempFilePath());
            Assert.AreEqual(newContent, readContent);
        }

        [Test]
        public void InvalidPendingFileDoesntOverwriteOriginal()
        {
            string filePath = GetTestFilePath();
            var oldContent = new byte[] { 77 };
            File.WriteAllBytes(filePath, oldContent);

            var writer = new AtomicFileWriter { MinValidFileSize = 999 };
            var newContent = new byte[] { 88 }; // New content is too small
            writer.Write(filePath, stream => stream.Write(newContent));

            byte[] readContent = File.ReadAllBytes(filePath);
            Assert.AreEqual(oldContent, readContent);
            Assert.IsFalse(ReadyFileExists());
            Assert.IsFalse(TempFileExists());
        }

        [Test]
        public void InvalidPendingFileDoesntBlock()
        {
            string filePath = GetTestFilePath();
            var newContent = new byte[] { 88 };
            var writer = new AtomicFileWriter { MinValidFileSize = 999 };
            writer.Write(filePath, stream => stream.Write(newContent));

            var newerContent = new byte[] { 99 };
            writer = new AtomicFileWriter { MinValidFileSize = 1 };
            writer.Write(filePath, stream => stream.Write(newerContent));

            byte[] readContent = File.ReadAllBytes(filePath);
            Assert.AreEqual(newerContent, readContent);
            Assert.IsFalse(ReadyFileExists());
            Assert.IsFalse(TempFileExists());
        }

        private string GetTestFilePath()
        {
            return Path.Combine(_directoryPath, _testFileName);
        }

        private string GetTempFilePath()
        {
            return GetTestFilePath() + ".new";
        }

        private string GetReadyFilePath()
        {
            return GetTestFilePath() + ".ready";
        }

        private bool TempFileExists()
        {
            return File.Exists(GetTempFilePath());
        }

        private bool ReadyFileExists()
        {
            return File.Exists(GetReadyFilePath());
        }
    }
}
