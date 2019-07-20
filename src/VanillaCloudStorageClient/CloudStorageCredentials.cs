using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Security;
using System.Xml.Serialization;

namespace VanillaCloudStorageClient
{
    /// <summary>
    /// Holds the credentials to connect to a cloud storage service. Usually this is either an
    /// OAuth2 token, a username/password combination, or a username/password/url combination.
    /// </summary>
    public class CloudStorageCredentials
    {
        /// <summary>
        /// Gets or sets the oauth2 access token if necessary, otherwise this is null.
        /// </summary>
        public CloudStorageToken Token { get; set; }

        /// <summary>
        /// Gets or sets the username for login if necessary, otherwise this is null.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets a password for login if necessary, otherwise this is null.
        /// </summary>
        public SecureString Password { get; set; }

        /// <summary>
        /// Gets or sets the url for login if necessary, otherwise this is null.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an encrypted SSL connection should be used.
        /// This value is currently used onyl by the FTP provider, others will ignore this value.
        /// </summary>
        public bool Secure { get; set; }

        /// <summary>
        /// Gets or sets the plain text password. Keep the usage of this property to a minimum,
        /// try to work with SeureString all the way instead.
        /// </summary>
        /// <remarks>
        /// The password itself will still be stored in the <see cref="Password"/> property.
        /// </remarks>
        [XmlIgnore]
        public string UnprotectedPassword
        {
            get
            {
                if (Password == null)
                    return null;
                else
                    return new NetworkCredential(null, Password).Password;
            }

            set
            {
                if (value == null)
                    Password = null;
                else
                    Password = new NetworkCredential(null, value).SecurePassword;
            }
        }
    }

    /// <summary>
    /// Extension methods for the <see cref="CloudStorageCredentials"/> class.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Just extension methods for the same class.")]
    public static class CloudStorageCredentialsExtensions
    {
        /// <summary>
        /// Validates the credentials according to the given <paramref name="requirements"/>.
        /// </summary>
        /// <param name="credentials">The credentials to validate.</param>
        /// <param name="requirements">The requirements the credentials must comply with.</param>
        /// <exception cref="InvalidParameterException">Is thrown when the credentials are invalid.</exception>
        public static void ThrowIfInvalid(this CloudStorageCredentials credentials, CloudStorageCredentialsRequirements requirements)
        {
            if (credentials == null)
                throw new InvalidParameterException(nameof(CloudStorageCredentials));

            switch (requirements)
            {
                case CloudStorageCredentialsRequirements.Token:
                    if (credentials.Token == null)
                        throw new InvalidParameterException(string.Format("{0}.{1}", nameof(CloudStorageCredentials), nameof(CloudStorageCredentials.Token)));
                    break;
                case CloudStorageCredentialsRequirements.UsernamePassword:
                    if (string.IsNullOrWhiteSpace(credentials.Username))
                        throw new InvalidParameterException(string.Format("{0}.{1}", nameof(CloudStorageCredentials), nameof(CloudStorageCredentials.Username)));
                    if (credentials.Password == null)
                        throw new InvalidParameterException(string.Format("{0}.{1}", nameof(CloudStorageCredentials), nameof(CloudStorageCredentials.Password)));
                    break;
                case CloudStorageCredentialsRequirements.UsernamePasswordUrl:
                case CloudStorageCredentialsRequirements.UsernamePasswordUrlSecure:
                    if (string.IsNullOrWhiteSpace(credentials.Username))
                        throw new InvalidParameterException(string.Format("{0}.{1}", nameof(CloudStorageCredentials), nameof(CloudStorageCredentials.Username)));
                    if (credentials.Password == null)
                        throw new InvalidParameterException(string.Format("{0}.{1}", nameof(CloudStorageCredentials), nameof(CloudStorageCredentials.Password)));
                    if (string.IsNullOrWhiteSpace(credentials.Url))
                        throw new InvalidParameterException(string.Format("{0}.{1}", nameof(CloudStorageCredentials), nameof(CloudStorageCredentials.Url)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(requirements));
            }
        }
    }
}
