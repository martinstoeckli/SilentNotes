using System.Security;
using System.Text;
using NUnit.Framework;
using VanillaCloudStorageClient;

namespace VanillaCloudStorageClientTest
{
    [TestFixture]
    public class SecureStringExtensionsTest
    {
        [Test]
        public void CorrectlyConvertsStringToSecureString()
        {
            string candidate = "The brown fox jumps over the lazy 🐢🖐🏿 doc.";
            SecureString result = SecureStringExtensions.StringToSecureString(candidate);
            Assert.IsNotNull(result);
            Assert.AreEqual("The brown fox jumps over the lazy 🐢🖐🏿 doc.", SecureStringExtensions.SecureStringToString(result));

            candidate = null;
            result = SecureStringExtensions.StringToSecureString(candidate);
            Assert.IsNull(result);

            candidate = string.Empty;
            result = SecureStringExtensions.StringToSecureString(candidate);
            Assert.AreEqual(0, result.Length);
        }

        [Test]
        public void CorrectlyConvertsSecureStringToString()
        {
            SecureString secureCandidate = new SecureString();
            secureCandidate.AppendChar('F');
            secureCandidate.AppendChar('o');
            secureCandidate.AppendChar('x');
            string retrievedCandidate = SecureStringExtensions.SecureStringToString(secureCandidate);
            Assert.AreEqual("Fox", retrievedCandidate);

            secureCandidate = SecureStringExtensions.StringToSecureString(null);
            Assert.IsNull(secureCandidate);
        }

        [Test]
        public void AreEqualsWorksCorrectly()
        {
            SecureString candidate1 = SecureStringExtensions.StringToSecureString("lazy 🐢🖐🏿 doc.");
            SecureString candidate2 = SecureStringExtensions.StringToSecureString("lazy 🐢🖐🏿 doc.");
            Assert.IsTrue(SecureStringExtensions.AreEqual(candidate1, candidate2));

            // Equal in length
            candidate1 = SecureStringExtensions.StringToSecureString("Hello world");
            candidate2 = SecureStringExtensions.StringToSecureString("Hello presi");
            Assert.IsFalse(SecureStringExtensions.AreEqual(candidate1, candidate2));

            // Different in length
            candidate1 = SecureStringExtensions.StringToSecureString("Hello world");
            candidate2 = SecureStringExtensions.StringToSecureString("Hello president");
            Assert.IsFalse(SecureStringExtensions.AreEqual(candidate1, candidate2));

            // Both null are equal
            candidate1 = null;
            candidate2 = null;
            Assert.IsTrue(SecureStringExtensions.AreEqual(candidate1, candidate2));
        }

        [Test]
        public void CorrectlyConvertsUnicodeBytesToSecureString()
        {
            byte[] candidate = Encoding.Unicode.GetBytes("lazy 🐢🖐🏿 doc.");
            SecureString result = SecureStringExtensions.BytesToSecureString(candidate, Encoding.Unicode);
            Assert.IsNotNull(result);
            Assert.AreEqual("lazy 🐢🖐🏿 doc.", SecureStringExtensions.SecureStringToString(result));

            candidate = null;
            result = SecureStringExtensions.BytesToSecureString(candidate, Encoding.Unicode);
            Assert.IsNull(result);
        }

        [Test]
        public void CorrectlyConvertsSecureStringToUnicodeBytes()
        {
            string candidate = "lazy 🐢🖐🏿 doc.";
            SecureString secureCandidate = SecureStringExtensions.StringToSecureString(candidate);
            byte[] bytes = SecureStringExtensions.SecureStringToBytes(secureCandidate, Encoding.Unicode);

            Assert.AreEqual("lazy 🐢🖐🏿 doc.", Encoding.Unicode.GetString(bytes));
        }

        [Test]
        public void CorrectlyConvertsUtf8BytesToSecureString()
        {
            byte[] candidate = Encoding.UTF8.GetBytes("lazy 🐢🖐🏿 doc.");
            SecureString result = SecureStringExtensions.BytesToSecureString(candidate, Encoding.UTF8);
            Assert.IsNotNull(result);
            Assert.AreEqual("lazy 🐢🖐🏿 doc.", SecureStringExtensions.SecureStringToString(result));

            candidate = null;
            result = SecureStringExtensions.BytesToSecureString(candidate, Encoding.UTF8);
            Assert.IsNull(result);
        }

        [Test]
        public void CorrectlyConvertsSecureStringToUtf8Bytes()
        {
            string candidate = "lazy 🐢🖐🏿 doc.";
            SecureString secureCandidate = SecureStringExtensions.StringToSecureString(candidate);
            byte[] bytes = SecureStringExtensions.SecureStringToBytes(secureCandidate, Encoding.UTF8);

            Assert.AreEqual("lazy 🐢🖐🏿 doc.", Encoding.UTF8.GetString(bytes));
        }
    }
}
