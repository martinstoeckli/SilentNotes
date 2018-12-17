using System.Security;
using System.Text;
using NUnit.Framework;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
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
        public void CorrectlyConvertsUnicodeBytesToSecureString()
        {
            byte[] candidate = StringToUnicodeBytes("lazy 🐢🖐🏿 doc.");
            SecureString result = SecureStringExtensions.UnicodeBytesToSecureString(candidate);
            Assert.IsNotNull(result);
            Assert.AreEqual("lazy 🐢🖐🏿 doc.", SecureStringExtensions.SecureStringToString(result));

            candidate = null;
            result = SecureStringExtensions.UnicodeBytesToSecureString(candidate);
            Assert.IsNull(result);
        }

        [Test]
        public void CorrectlyConvertsSecureStringToUnicodeBytes()
        {
            string candidate = "lazy 🐢🖐🏿 doc.";
            SecureString secureCandidate = SecureStringExtensions.StringToSecureString(candidate);
            byte[] bytes = SecureStringExtensions.SecureStringToUnicodeBytes(secureCandidate);

            Assert.AreEqual("lazy 🐢🖐🏿 doc.", UnicodeBytesToString(bytes));
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

        private static string UnicodeBytesToString(byte[] bytes)
        {
            return Encoding.Unicode.GetString(bytes, 0, bytes.Length);
        }

        private static byte[] StringToUnicodeBytes(string unicodeString)
        {
            return Encoding.Unicode.GetBytes(unicodeString);
        }
    }
}
