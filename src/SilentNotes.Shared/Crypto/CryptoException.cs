// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;

namespace SilentNotes.Crypto
{
    /// <summary>
    /// Base class, and generic exception class, for the StoCrypto module.
    /// </summary>
    public class CryptoException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoException"/> class.
        /// </summary>
        public CryptoException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public CryptoException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">An inner exception which lead to this exception.</param>
        public CryptoException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// This exception will be thrown, whenever the cipher has an invalid format.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Just exceptions.")]
    public class CryptoExceptionInvalidCipherFormat : CryptoException
    {
    }

    /// <summary>
    /// This exception will be thrown, whenever the cipher was packed with a future incompatible
    /// version and cannot be decrypted with the current version.
    /// </summary>
    public class CryptoUnsupportedRevisionException : CryptoException
    {
    }

    /// <summary>
    /// This exception will be thrown, whenever the decryption fails, mostly because of a wrong key.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Just exceptions.")]
    public class CryptoDecryptionException : CryptoException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CryptoDecryptionException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">An inner exception which lead to this exception.</param>
        public CryptoDecryptionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
