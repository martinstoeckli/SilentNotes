using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SilentNotes.Workers
{
    /// <summary>
    /// A file writer which guarantees that only completely written files are replacing an already
    /// existing file. This is especially important on Android systems, where the OS can kill the
    /// process at any moment. When the application writes a file at this very moment, it results
    /// in a file with 0 bytes and therefore destroys the content of this file.
    /// </summary>
    /// <remarks>
    /// This file writer does not confer any file locking semantics. Do not use this class when
    /// the file may be accessed or modified concurrently by multiple threads or processes.
    /// </remarks>
    public class AtomicFileWriter
    {
        private readonly TestSimulation _testSimulation;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicFileWriter"/> class.
        /// </summary>
        public AtomicFileWriter()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicFileWriter"/> class.
        /// Use this overload only for testing purposes.
        /// </summary>
        /// <param name="testParameters">Parameters only for unit testing.</param>
        public AtomicFileWriter(TestSimulation testParameters)
        {
            _testSimulation = testParameters;
        }

        /// <summary>
        /// Writes to a file, so that the integrity of the exsting file is guaranteed until the
        /// new file is written completely.
        /// </summary>
        /// <remarks>If the directory does not yet exist, the function tries to create it, before
        /// writing the file. The file names filename + ".new" and + ".ready" are required by the
        /// function and should not be used.</remarks>
        /// <param name="filePath">Full path to the file to write/overwrite.</param>
        /// <param name="writingDelegate">Delegate receiving an open file stream, which can be used
        /// to write to the file.</param>
        public void Write(string filePath, Action<FileStream> writingDelegate)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (writingDelegate == null)
                throw new ArgumentNullException(nameof(writingDelegate));

            // Create directory if not yet existing
            string directoryPath = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(directoryPath);

            // Check if an unfinished writing operation exists, and if so, block the new write
            // operation. This way we can prevent that the previously written new content can get
            // lost/overwritten.
            string readyFilePath = GetReadyFilePath(filePath);
            if (File.Exists(readyFilePath))
                throw new UnfinishedAtomicFileWritingException(filePath);

            // Write to temporary file
            string tempFilePath = GetTempFilePath(filePath);
            using (FileStream tempFileStream = new FileStream(tempFilePath, FileMode.Create))
            {
                // Critical place, we can simulate an error while writing, or the killing of the
                // writing process by the OS (Android).
                if ((_testSimulation != null) && _testSimulation.SimulateWriteError)
                    throw new Exception(nameof(_testSimulation.SimulateWriteError));
                writingDelegate(tempFileStream);
            }

            // Now that the new file is written completely (process was not killed while writing),
            // we set the ready file state. The ready-file indicates that a completely written file
            // is waiting to replace the original file and should block future writings until removed.
            File.WriteAllBytes(readyFilePath, new byte[] { 32 });

            CompletePendingWrite(filePath);
        }

        /// <summary>
        /// Checks whether there is an unfinished writing operation and tries to complete it.
        /// </summary>
        /// <param name="filePath"></param>
        /// <exception cref="UnfinishedAtomicFileWritingException"></exception>
        public void CompletePendingWrite(string filePath)
        {
            string readyFilePath = GetReadyFilePath(filePath);
            if (!File.Exists(readyFilePath))
                return;

            // Critical place, we can simulate an error while replacing the original file with
            // the new content.
            if ((_testSimulation != null) && _testSimulation.SimulateReplaceError)
                throw new Exception(nameof(TestSimulation.SimulateReplaceError));

            // An unfinished writing operation exists (ready file still exists)
            string tempFilePath = GetTempFilePath(filePath);
            File.Copy(tempFilePath, filePath, true);
            if (!SameFileSize(tempFilePath, filePath))
            {
                // The ready file will still exist, so no data loss
                throw new UnfinishedAtomicFileWritingException(filePath);
            }

            // Clean up ready-file after the target file has been copied error free
            File.Delete(readyFilePath);
            File.Delete(tempFilePath);
        }

        private static string GetTempFilePath(string filePath)
        {
            return filePath + ".new";
        }

        private static string GetReadyFilePath(string filePath)
        {
            return filePath + ".ready";
        }

        private static bool SameFileSize(string filePath1, string filePath2)
        {
            long fileSize1 = new FileInfo(filePath1).Length;
            long fileSize2 = new FileInfo(filePath2).Length;
            return fileSize1 == fileSize2;
        }

        public class TestSimulation
        {
            /// <summary>
            /// Gets a value indicating that a failed stream writing should be simulated.
            /// </summary>
            public bool SimulateWriteError { get; set; }

            /// <summary>
            /// Gets a value indicating that the original file could not be replaces with the new content.
            /// </summary>
            public bool SimulateReplaceError { get; set; }
        }
    }

    /// <summary>
    /// This exception is thrown when an unfinished writing operation blocks the writing of a file.
    /// </summary>
    public class UnfinishedAtomicFileWritingException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnfinishedAtomicFileWritingException"/> class.
        /// </summary>
        /// <param name="filePath">The file which should be written to, but has an unfinished
        /// writing operation pending.</param>
        public UnfinishedAtomicFileWritingException(string filePath)
            : base(CreateErrorMessage(filePath))
        {
            FilePath = filePath;
        }

        /// <summary>
        /// Gets the path of the file which could not be written to.
        /// </summary>
        public string FilePath { get; }

        private static string CreateErrorMessage(string filePath)
        {
            return string.Format("The file '{0}' cannot be written, because there is an unfinished file writing operation pending.", filePath);
        }
    }
}
