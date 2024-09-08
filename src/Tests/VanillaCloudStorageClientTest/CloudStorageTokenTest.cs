using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VanillaCloudStorageClient;

namespace VanillaCloudStorageClientTest
{
    [TestClass]
    public class CloudStorageTokenTest
    {
        [TestMethod]
        public void SetExpiryDateBySecondsWorks()
        {
            CloudStorageToken token = new CloudStorageToken();
            token.SetExpiryDateBySecondsFromNow(375);

            Assert.IsNotNull(token.ExpiryDate);
            Assert.IsTrue(token.ExpiryDate > DateTime.UtcNow);
            Assert.IsTrue(token.ExpiryDate < DateTime.UtcNow.AddSeconds(375));
        }

        [TestMethod]
        public void SetExpiryDateBySecondsWorksWithVeryShortPeriod()
        {
            CloudStorageToken token = new CloudStorageToken();
            token.SetExpiryDateBySecondsFromNow(8);

            Assert.IsNotNull(token.ExpiryDate);
            Assert.IsTrue(token.ExpiryDate > DateTime.UtcNow);
            Assert.IsTrue(token.ExpiryDate < DateTime.UtcNow.AddSeconds(8));
        }

        [TestMethod]
        public void SetExpiryDateBySecondsWorksWithNull()
        {
            CloudStorageToken token = new CloudStorageToken();
            token.SetExpiryDateBySecondsFromNow(null);

            Assert.IsNull(token.ExpiryDate);
        }

        [TestMethod]
        public void AreEqualWorksWithSameContent()
        {
            CloudStorageToken token = new CloudStorageToken
            {
                AccessToken = "a", RefreshToken = null, ExpiryDate = new DateTime(1984, 11, 11)
            };

            CloudStorageToken token2 = new CloudStorageToken
            {
                AccessToken = "a", RefreshToken = null, ExpiryDate = new DateTime(1984, 11, 11)
            };

            Assert.IsTrue(token.AreEqualOrNull(token2));
            Assert.IsFalse(token.AreEqualOrNull(null));

            token = null;
            Assert.IsTrue(token.AreEqualOrNull(null));
        }

        [TestMethod]
        public void AreEqualWorksWithNullDate()
        {
            CloudStorageToken token = new CloudStorageToken
            {
                AccessToken = "a",
                RefreshToken = null,
                ExpiryDate = null
            };

            CloudStorageToken token2 = new CloudStorageToken
            {
                AccessToken = "a",
                RefreshToken = null,
                ExpiryDate = null
            };

            Assert.IsTrue(token.AreEqualOrNull(token2));

            token2.ExpiryDate = new DateTime(1984, 11, 11);
            Assert.IsFalse(token.AreEqualOrNull(token2));
        }

        [TestMethod]
        public void NeedsRefreshReturnsTrueIfExpired()
        {
            CloudStorageToken token = new CloudStorageToken { RefreshToken = "notempty" };
            token.ExpiryDate = DateTime.Now.AddSeconds(-8);
            Assert.IsTrue(token.NeedsRefresh());
        }

        [TestMethod]
        public void NeedsRefreshReturnsFalseIfNotExpired()
        {
            CloudStorageToken token = new CloudStorageToken { RefreshToken = "notempty" };
            token.ExpiryDate = DateTime.Now.AddSeconds(8);
            Assert.IsFalse(token.NeedsRefresh());
        }

        [TestMethod]
        public void NeedsRefreshReturnsTrueIfNoExpirationDate()
        {
            CloudStorageToken token = new CloudStorageToken { RefreshToken = "notempty" };
            Assert.IsTrue(token.NeedsRefresh());
        }

        [TestMethod]
        public void NeedsRefreshReturnsFalseForTokenFlow()
        {
            // The token flow has no refresh token
            CloudStorageToken token = new CloudStorageToken { RefreshToken = null };
            token.ExpiryDate = DateTime.UtcNow.AddSeconds(-8); // Would have been expired
            Assert.IsFalse(token.NeedsRefresh());
        }
    }
}
