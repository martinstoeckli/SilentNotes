using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// Abstraction for reading and writing of files, to make other services testable.
    /// </summary>
    public interface IFileService
    {
        /// <summary>
        /// Abstraction of <see cref="File.Exists(string?)"/>
        /// </summary>
        bool Exists(string filePath);

        /// <summary>
        /// Abstraction of <see cref="File.ReadAllBytesAsync(string, CancellationToken)"/>
        /// </summary>
        Task<byte[]> ReadAsync(string filePath, CancellationToken cancellationToken = default);

        /// <summary>
        /// Abstraction of <see cref="File.WriteAllBytesAsync(string, byte[], CancellationToken)"/>
        /// </summary>
        Task WriteAsync(string filePath, byte[] content, CancellationToken cancellationToken = default);
    }
}
