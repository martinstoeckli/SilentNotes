using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="IFileService"/> interface.
    /// </summary>
    public class FileService : IFileService
    {
        /// <inheritdoc/>
        public bool Exists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <inheritdoc/>
        public async Task<byte[]> ReadAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return await File.ReadAllBytesAsync(filePath, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task WriteAsync(string filePath, byte[] content, CancellationToken cancellationToken = default)
        {
            await File.WriteAllBytesAsync(filePath, content, cancellationToken);
        }
    }
}
