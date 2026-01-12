// Copyright © 2026 Martin Stoeckli.
// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the license was not distributed with this
// file, You can obtain one at https://opensource.org/licenses/MIT.

using System;
using Java.Nio;
using SilentNotes.Platforms.Services;

namespace SilentNotes.Platforms.Android
{
    /// <summary>
    /// Wrapper around a Java.Nio.ByteBuffer which exposes it as a .Net stream.
    /// </summary>
    internal class ReadOnlyByteBufferStream : Stream
    {
        private readonly ByteBuffer _buffer;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyByteBufferStream"/> class.
        /// </summary>
        /// <param name="buffer">The byte buffer to wrap.</param>
        public ReadOnlyByteBufferStream(ByteBuffer buffer)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            _buffer = buffer;
        }

        /// <inheritdoc/>
        public override bool CanRead => !_disposed;

        /// <inheritdoc/>
        public override bool CanSeek => !_disposed;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length
        {
            get
            {
                EnsureNotClosed();
                return _buffer.Limit();
            }
        }

        /// <inheritdoc/>
        public override long Position
        {
            get
            {
                EnsureNotClosed();
                return _buffer.Position();
            }

            set
            {
                if ((value < 0) || (value > int.MaxValue))
                    throw new ArgumentOutOfRangeException(nameof(value));
                EnsureNotClosed();

                _buffer.Position((int)value);
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            // Stream is readonly
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            Stream.ValidateBufferArguments(buffer, offset, count);
            EnsureNotClosed();

            int remaining = _buffer.Remaining();
            int toRead = Math.Min(remaining, count);
            if (toRead == 0)
                return 0;

            _buffer.Get(buffer, offset, toRead);
            return toRead;
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            EnsureNotClosed();

            int remaining = _buffer.Remaining();
            int toRead = Math.Min(remaining, buffer.Length);
            if (toRead == 0)
                return 0;

            byte[] array = System.Buffers.ArrayPool<byte>.Shared.Rent(toRead);
            try
            {
                _buffer.Get(array, 0, toRead);
                new ReadOnlySpan<byte>(array, 0, toRead).CopyTo(buffer);
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(array);
            }
            return toRead;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            EnsureNotClosed();

            int newPosition;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    newPosition = (int)offset;
                    break;
                case SeekOrigin.Current:
                    newPosition = _buffer.Position() + (int)offset;
                    break;
                case SeekOrigin.End:
                    newPosition = _buffer.Limit() + (int)offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin));
            };

            if ((newPosition < 0) || (newPosition > _buffer.Limit()))
                throw new IOException("Attempted to seek outside the buffer bounds.");

            _buffer.Position(newPosition);
            return newPosition;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException(); // Read only stream
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException(); // Read only stream
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _disposed = true;
        }

        protected void EnsureNotClosed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ReadOnlyByteBufferStream));
        }
    }
}
