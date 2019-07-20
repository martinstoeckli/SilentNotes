using System;
using System.Diagnostics.CodeAnalysis;

namespace VanillaCloudStorageClient
{
    /// <summary>
    /// Base class of all cloud storage exceptions.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Just a list of exceptions.")]
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
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Just a list of exceptions.")]
    public class ConnectionFailedException : CloudStorageException
    {
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
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Just a list of exceptions.")]
    public class AccessDeniedException : CloudStorageException
    {
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
    /// Throw this exception if the required parameter is missing or invalid.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Just a list of exceptions.")]
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
