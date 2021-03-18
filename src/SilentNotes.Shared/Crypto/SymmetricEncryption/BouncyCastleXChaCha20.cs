// Copyright © 2021 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace SilentNotes.Crypto.SymmetricEncryption
{
    /// <summary>
    /// Implementation of the <see cref="ISymmetricEncryptionAlgorithm"/> interface, implementing
    /// the XChaCha20-Poly1305 algorithm. Is is tested to be compatible with the libsodium library.
    /// This class depends on the BouncyCastle library.
    /// </summary>
    public class BouncyCastleXChaCha20 : ISymmetricEncryptionAlgorithm
    {
        /// <summary>The name of the XChaCha20-Poly1305 encryption algorithm.</summary>
        public const string CryptoAlgorithmName = "xchacha20_poly1305";

        private const int NonceSizeBytes = 24; // 192 bits
        private const int KeySizeBytes = 32; // 256 bits
        private const int MacSizeBytes = 16; // 128 bits

        /// <inheritdoc />
        public string Name
        {
            get { return CryptoAlgorithmName; }
        }

        /// <inheritdoc />
        public byte[] Encrypt(byte[] message, byte[] key, byte[] nonce)
        {
            return EncryptOrDecrypt(true, message, key, nonce);
        }

        /// <inheritdoc />
        public byte[] Decrypt(byte[] cipher, byte[] key, byte[] nonce)
        {
            return EncryptOrDecrypt(false, cipher, key, nonce);
        }

        private byte[] EncryptOrDecrypt(bool forEncryption, byte[] data, byte[] key, byte[] nonce)
        {
            if (ExpectedKeySize != key.Length)
                throw new CryptoException("Invalid key size");
            if (ExpectedNonceSize != nonce.Length)
                throw new CryptoException("Invalid nonce size");

            // We do the X-part of XChaCha20 and then use the ChaCha20 from bouncy castle
            byte[] subkey = new byte[KeySizeBytes];
            ChaCha20Base.HChaCha20(subkey, key, nonce);
            byte[] chaChaNonce = CreateChaChaNonce(nonce);

            ICipherParameters aeadParams = new AeadParameters(
                new KeyParameter(subkey), MacSizeBytes * 8, chaChaNonce, null);
            IAeadCipher chaCha20Poly1305 = new ChaCha20Poly1305();
            chaCha20Poly1305.Init(forEncryption, aeadParams);

            byte[] result = new byte[chaCha20Poly1305.GetOutputSize(data.Length)];
            int len = chaCha20Poly1305.ProcessBytes(data, 0, data.Length, result, 0);
            chaCha20Poly1305.DoFinal(result, len);
            return result;
        }

        /// <inheritdoc />
        public int ExpectedKeySize
        {
            get { return KeySizeBytes; }
        }

        /// <inheritdoc />
        public int ExpectedNonceSize
        {
            get { return NonceSizeBytes; }
        }

        /// <summary>
        /// Creates a ChaCha20 compatible nonce with 4 leading zero bytes + 8 bytes [16-23] from
        /// the XChaCha20 nonce.
        /// </summary>
        /// <param name="nonce">The original nonce of XChaCha20.</param>
        /// <returns>The nonce for the ChaCha20.</returns>
        private static byte[] CreateChaChaNonce(byte[] nonce)
        {
            byte[] result = new byte[12];
            Array.Clear(result, 0, 4);
            Array.Copy(nonce, 16, result, 4, 8);
            return result;
        }

        /// <summary>
        /// Copied from https://github.com/daviddesmet/NaCl.Core and made a static helper class.
        /// </summary>
        private static class ChaCha20Base
        {
            public const int BLOCK_SIZE_IN_INTS = 16;
            public static uint[] SIGMA = new uint[] { 0x61707865, 0x3320646E, 0x79622D32, 0x6B206574 }; //Encoding.ASCII.GetBytes("expand 32-byte k");

            /// <summary>
            /// Process a pseudorandom keystream block, converting the key and part of the <paramref name="nonce"/> into a <paramref name="subkey"/>, and the remainder of the <paramref name="nonce"/>.
            /// </summary>
            /// <param name="subKey">The subKey.</param>
            /// <param name="nonce">The nonce.</param>
            public static void HChaCha20(Span<byte> subKey, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce)
            {
                // See https://tools.ietf.org/html/draft-arciszewski-xchacha-01#section-2.2.

                Span<uint> state = stackalloc uint[BLOCK_SIZE_IN_INTS];

                // Setting HChaCha20 initial state
                HChaCha20InitialState(state, key, nonce);

                // Block function
                ShuffleState(state);

                state[4] = state[12];
                state[5] = state[13];
                state[6] = state[14];
                state[7] = state[15];

                ArrayUtils.StoreArray8UInt32LittleEndian(subKey, 0, state);
            }

            private static void HChaCha20InitialState(Span<uint> state, ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce)
            {
                // See https://tools.ietf.org/html/draft-arciszewski-xchacha-01#section-2.2.

                // Set ChaCha20 constant
                SetSigma(state);

                // Set 256-bit Key
                SetKey(state, key);

                // Set 128-bit Nonce
                state[12] = ArrayUtils.LoadUInt32LittleEndian(nonce, 0);
                state[13] = ArrayUtils.LoadUInt32LittleEndian(nonce, 4);
                state[14] = ArrayUtils.LoadUInt32LittleEndian(nonce, 8);
                state[15] = ArrayUtils.LoadUInt32LittleEndian(nonce, 12);
            }

            private static void ShuffleState(Span<uint> state)
            {
                for (var i = 0; i < 10; i++)
                {
                    QuarterRound(ref state[0], ref state[4], ref state[8], ref state[12]);
                    QuarterRound(ref state[1], ref state[5], ref state[9], ref state[13]);
                    QuarterRound(ref state[2], ref state[6], ref state[10], ref state[14]);
                    QuarterRound(ref state[3], ref state[7], ref state[11], ref state[15]);
                    QuarterRound(ref state[0], ref state[5], ref state[10], ref state[15]);
                    QuarterRound(ref state[1], ref state[6], ref state[11], ref state[12]);
                    QuarterRound(ref state[2], ref state[7], ref state[8], ref state[13]);
                    QuarterRound(ref state[3], ref state[4], ref state[9], ref state[14]);
                }
            }

            private static void QuarterRound(ref uint a, ref uint b, ref uint c, ref uint d)
            {
                a += b;
                d = BitUtils.RotateLeft(d ^ a, 16);
                c += d;
                b = BitUtils.RotateLeft(b ^ c, 12);
                a += b;
                d = BitUtils.RotateLeft(d ^ a, 8);
                c += d;
                b = BitUtils.RotateLeft(b ^ c, 7);
            }

            /// <summary>
            /// Sets the ChaCha20 constant.
            /// </summary>
            /// <param name="state">The state.</param>
            private static void SetSigma(Span<uint> state)
            {
                state[0] = SIGMA[0];
                state[1] = SIGMA[1];
                state[2] = SIGMA[2];
                state[3] = SIGMA[3];
            }

            /// <summary>
            /// Sets the 256-bit Key.
            /// </summary>
            /// <param name="state">The state.</param>
            /// <param name="key">The key.</param>
            private static void SetKey(Span<uint> state, ReadOnlySpan<byte> key)
            {
                state[4] = ArrayUtils.LoadUInt32LittleEndian(key, 0);
                state[5] = ArrayUtils.LoadUInt32LittleEndian(key, 4);
                state[6] = ArrayUtils.LoadUInt32LittleEndian(key, 8);
                state[7] = ArrayUtils.LoadUInt32LittleEndian(key, 12);
                state[8] = ArrayUtils.LoadUInt32LittleEndian(key, 16);
                state[9] = ArrayUtils.LoadUInt32LittleEndian(key, 20);
                state[10] = ArrayUtils.LoadUInt32LittleEndian(key, 24);
                state[11] = ArrayUtils.LoadUInt32LittleEndian(key, 28);
            }
        }

        /// <summary>
        /// Copied from https://github.com/daviddesmet/NaCl.Core
        /// </summary>
        private static class ArrayUtils
        {
            /// <summary>
            /// Loads 4 bytes of the input buffer into an unsigned 32-bit integer, beginning at the input offset.
            /// </summary>
            /// <param name="buf">The input buffer.</param>
            /// <param name="offset">The input offset.</param>
            /// <returns>System.UInt32.</returns>
            public static uint LoadUInt32LittleEndian(ReadOnlySpan<byte> buf, int offset)
                => BinaryPrimitives.ReadUInt32LittleEndian(buf.Slice(offset + 0, sizeof(int)));

            /// <summary>
            /// Stores the byte array in 8 parts split into 4 bytes and places it in the output buffer.
            /// </summary>
            /// <param name="output">The output buffer.</param>
            /// <param name="offset">The starting offset.</param>
            /// <param name="input">The input buffer.</param>
            public static void StoreArray8UInt32LittleEndian(Span<byte> output, int offset, ReadOnlySpan<uint> input)
            => StoreArrayUInt32LittleEndian(output, offset, input, 8);

            /// <summary>
            /// Stores the byte array split in n size parts into 4 bytes and places it in the output buffer.
            /// </summary>
            /// <param name="output">The output buffer.</param>
            /// <param name="offset">The starting offset.</param>
            /// <param name="input">The input buffer.</param>
            /// <param name="size">The parts to split the input buffer.</param>
            public static void StoreArrayUInt32LittleEndian(Span<byte> output, int offset, ReadOnlySpan<uint> input, int size)
            {
                var len = sizeof(int);

                var start = offset + 0;
                for (var i = 0; i < size; i++)
                {
                    BinaryPrimitives.WriteUInt32LittleEndian(output.Slice(start, len), input[i]);
                    start += len;
                }
            }
        }

        /// <summary>
        /// Copied from https://github.com/daviddesmet/NaCl.Core
        /// </summary>
        private static class BitUtils
        {
            /// <summary>
            /// Rotates the specified value left by the specified number of bits.
            /// </summary>
            /// <param name="value">The value to rotate.</param>
            /// <param name="offset">The number of bits to rotate by.</param>
            /// <returns>The rotated value.</returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static uint RotateLeft(uint value, int offset)
            {
                return (value << offset) | (value >> (32 - offset));
            }
        }
    }
}
