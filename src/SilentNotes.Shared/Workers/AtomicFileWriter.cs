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
    /// There is a minimal chance, that the process is interrupted when renaming the files, in this
    /// case the old file can be found with filename + ".old", but this case should be negligible.
    /// </remarks>
    public static class AtomicFileWriter
    {
        /// <summary>
        /// Writes to a file, so that the integrity of the exsting file is guaranteed until the new
        /// file is written completely.
        /// </summary>
        /// <remarks>If the directory does not yet exist, the function tries to create it, before
        /// writing the file. The file names filename + ".new" and + ".old" are required by the
        /// function and should not be used.</remarks>
        /// <param name="filePath">Full path to the file to write/overwrite.</param>
        /// <param name="writingDelegate">Delegate receiving an open file stream, which can be used
        /// to write to the file.</param>
        public static void Write(string filePath, Action<FileStream> writingDelegate)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (writingDelegate == null)
                throw new ArgumentNullException(nameof(writingDelegate));

            // Create directory if not yet existing
            string directoryPath = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(directoryPath);

            // Write to temporary file
            string newFilePath = filePath + ".new";
            using (FileStream tempFileStream = new FileStream(newFilePath, FileMode.Create))
            {
                writingDelegate(tempFileStream);
            }

            // Now that the new file is written completely (process was not killed in meantime),
            // it will be renamed to the original file name.
            File.Copy(newFilePath, filePath, true);

            // Clean up if copy is verified
            if (File.Exists(filePath))
                File.Delete(newFilePath);
        }

        public static void CompleteUnfinishedWrite(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            string newFilePath = filePath + ".new";
            if (File.Exists(newFilePath))
            {
                // Since the temporary file was not cleaned up, something went wrong when writing the last time.
                File.Copy(newFilePath, filePath, true);

                // Clean up if copy is verified
                if (File.Exists(filePath))
                    File.Delete(newFilePath);
            }
            else
            {
                // Todo: Remove this handling of legacy code situation in next version
                string oldFilePath = filePath + ".old";
                if (File.Exists(oldFilePath))
                {
                    File.Copy(oldFilePath, filePath, true);
                    File.Delete(oldFilePath);
                }
            }
        }
    }
}
