using System;
using System.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VanillaCloudStorageClient;

namespace VanillaCloudStorageClientTest
{
    [TestClass]
    public class CloudStorageCredentialsTest
    {
        [TestMethod]
        public void CorrectlyConvertsStringToSecureString()
        {
            string candidate = "The brown fox jumps over the lazy 🐢🖐🏿 doc.";
            SecureString result = StringToSecureString(candidate);
            Assert.IsNotNull(result);
            Assert.AreEqual("The brown fox jumps over the lazy 🐢🖐🏿 doc.", SecureStringToString(result));

            candidate = null;
            result = StringToSecureString(candidate);
            Assert.IsNull(result);

            candidate = string.Empty;
            result = StringToSecureString(candidate);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void CorrectlyConvertsSecureStringToString()
        {
            SecureString secureCandidate = new SecureString();
            secureCandidate.AppendChar('F');
            secureCandidate.AppendChar('o');
            secureCandidate.AppendChar('x');
            string retrievedCandidate = SecureStringToString(secureCandidate);
            Assert.AreEqual("Fox", retrievedCandidate);

            secureCandidate = StringToSecureString(null);
            Assert.IsNull(secureCandidate);
        }

        [TestMethod]
        public void ValidateAcceptsValidCredentials()
        {
            CloudStorageCredentials credentials = new CloudStorageCredentials
            {
                CloudStorageId = "Dropbox",
                Token = new CloudStorageToken(),
                Username = "user",
                UnprotectedPassword = "pwd",
                Url = "url",
                Secure = false
            };

            credentials.ThrowIfInvalid(CloudStorageCredentialsRequirements.Token);
            credentials.ThrowIfInvalid(CloudStorageCredentialsRequirements.Username | CloudStorageCredentialsRequirements.Password);
            credentials.ThrowIfInvalid(CloudStorageCredentialsRequirements.Username | CloudStorageCredentialsRequirements.Password | CloudStorageCredentialsRequirements.Url);
            credentials.ThrowIfInvalid(CloudStorageCredentialsRequirements.Username | CloudStorageCredentialsRequirements.Password | CloudStorageCredentialsRequirements.Url | CloudStorageCredentialsRequirements.Secure | CloudStorageCredentialsRequirements.AcceptUnsafeCertificate);
        }

        [TestMethod]
        public void ValidateRejectsInvalidCredentials()
        {
            CloudStorageCredentials credentials = new CloudStorageCredentials();
            Assert.ThrowsException<InvalidParameterException>(() => credentials.ThrowIfInvalid(CloudStorageCredentialsRequirements.Token));
            Assert.ThrowsException<InvalidParameterException>(() => credentials.ThrowIfInvalid(CloudStorageCredentialsRequirements.Username | CloudStorageCredentialsRequirements.Password));
            Assert.ThrowsException<InvalidParameterException>(() => credentials.ThrowIfInvalid(CloudStorageCredentialsRequirements.Username | CloudStorageCredentialsRequirements.Password | CloudStorageCredentialsRequirements.Url));
            Assert.ThrowsException<InvalidParameterException>(() => credentials.ThrowIfInvalid(CloudStorageCredentialsRequirements.Username | CloudStorageCredentialsRequirements.Password | CloudStorageCredentialsRequirements.Url | CloudStorageCredentialsRequirements.Secure | CloudStorageCredentialsRequirements.AcceptUnsafeCertificate));

            credentials = null;
            Assert.ThrowsException<InvalidParameterException>(() => credentials.ThrowIfInvalid(CloudStorageCredentialsRequirements.Token));
        }

        [TestMethod]
        public void AreEqualWorksWithSameContent()
        {
            CloudStorageCredentials credentials = new CloudStorageCredentials
            {
                Token = new CloudStorageToken { AccessToken = "a" }
            };
            CloudStorageCredentials credentials2 = new CloudStorageCredentials
            {
                Token = new CloudStorageToken { AccessToken = "a" }
            };

            Assert.IsTrue(credentials.AreEqualOrNull(credentials2));

            credentials = null;
            credentials2 = null;
            Assert.IsTrue(credentials.AreEqualOrNull(credentials2));
        }

        [TestMethod]
        public void AreEqualWorksWithDifferentPassword()
        {
            CloudStorageCredentials credentials = new CloudStorageCredentials
            {
                Password = SecureStringExtensions.StringToSecureString("abc")
            };
            CloudStorageCredentials credentials2 = new CloudStorageCredentials
            {
                Password = SecureStringExtensions.StringToSecureString("def")
            };

            Assert.IsFalse(credentials.AreEqualOrNull(credentials2));
        }


        /// <summary>
        /// Converts a string to a SecureString. Keep the usage of this method to a minimum, try to
        /// work with SeureString all the way instead.
        /// </summary>
        /// <param name="password">Password in string form.</param>
        /// <returns>Password in a SecureString.</returns>
        private static SecureString StringToSecureString(string password)
        {
            var credentials = new CloudStorageCredentials();
            credentials.UnprotectedPassword = password;
            return credentials.Password;
        }

        /// <summary>
        /// Converts a SecureString to a string. Keep the usage of this method to a minimum, try to
        /// work with SeureString all the way instead.
        /// </summary>
        /// <param name="password">Password as SecureString.</param>
        /// <returns>Password in a plain text managed string.</returns>
        private static string SecureStringToString(SecureString password)
        {
            var credentials = new CloudStorageCredentials();
            credentials.Password = password;
            return credentials.UnprotectedPassword;
        }
    }
}
