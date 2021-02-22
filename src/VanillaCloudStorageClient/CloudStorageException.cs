// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace VanillaCloudStorageClient
{
    /// <summary>
    /// Base class of all cloud storage exceptions.
    /// </summary>
    public class CloudStorageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CloudStorageException"/> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="innerException">The inner exception if any, or null.</param>
        public CloudStorageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Throw this exception if the server could not be connected.
    /// Usually this happens when the user provides a wrong url, or if the internet access is
    /// turned off.
    /// </summary>
    public class ConnectionFailedException : CloudStorageException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionFailedException"/> class.
        /// Prefer the other constructors for production and keep this one for unittesting.
        /// </summary>
        public ConnectionFailedException()
            : base("An error occured while connecting to the server", null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionFailedException"/> class.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="innerException">The inner exception if any, or null.</param>
        public ConnectionFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionFailedException"/> class.
        /// </summary>
        /// <param name="innerException">The inner exception if any, or null.</param>
        public ConnectionFailedException(Exception innerException)
            : base("An error occured while connecting to the server", innerException)
        {
        }
    }

    /// <summary>
    /// Throw this exception if the authentication failed or if there are missing privileges.
    /// Usually this happens when the user provides a wrong username password combination.
    /// </summary>
    public class AccessDeniedException : CloudStorageException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccessDeniedException"/> class.
        /// Prefer the other constructors for production and keep this one for unittesting.
        /// </summary>
        public AccessDeniedException()
            : base("Access to the requested resource was denied.", null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccessDeniedException"/> class.
        /// </summary>
        /// <param name="innerException">The inner exception if any, or null.</param>
        public AccessDeniedException(Exception innerException)
            : base("Access to the requested resource was denied.", innerException)
        {
        }
    }

    /// <summary>
    /// Throw this exception if the token refresh was not granted by the OAuth2 server.
    /// This happens when the refresh-token has expired and the user has to do a new login.
    /// </summary>
    public class RefreshTokenExpiredException : CloudStorageException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshTokenExpiredException"/> class.
        /// Prefer the other constructors for production and keep this one for unittesting.
        /// </summary>
        public RefreshTokenExpiredException()
            : base("The OAuth2 server answered with a 'invalid_grant error'.", null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefreshTokenExpiredException"/> class.
        /// Prefer the other constructors for production and keep this one for unittesting.
        /// </summary>
        /// <param name="innerException">The inner exception if any, or null.</param>
        public RefreshTokenExpiredException(Exception innerException)
            : base("The OAuth2 server answered with a 'invalid_grant error'.", innerException)
        {
        }
    }

    /// <summary>
    /// Throw this exception if the required parameter is missing or invalid.
    /// </summary>
    public class InvalidParameterException : CloudStorageException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidParameterException"/> class.
        /// </summary>
        /// <param name="parameterName">The name of the parameter which is missing or invalid.</param>
        public InvalidParameterException(string parameterName)
            : base(string.Format("The parameter '{0}' is missing or invalid.", parameterName), null)
        {
            ParameterName = parameterName;
        }

        /// <summary>
        /// Gets the name of the invalid parameter.
        /// </summary>
        public string ParameterName { get; private set; }
    }
}
