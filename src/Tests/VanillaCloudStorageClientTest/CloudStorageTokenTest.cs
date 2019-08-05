using System;
using NUnit.Framework;
using VanillaCloudStorageClient;

namespace VanillaCloudStorageClientTest
{
    [TestFixture]
    public class CloudStorageTokenTest
    {
        [Test]
        public void SetExpiryDateBySecondsWorks()
        {
            CloudStorageToken token = new CloudStorageToken();
            token.SetExpiryDateBySecondsFromNow(375);

            Assert.IsNotNull(token.ExpiryDate);
            Assert.IsTrue(token.ExpiryDate > DateTime.Now);
            Assert.IsTrue(token.ExpiryDate < DateTime.Now.AddSeconds(375));
        }

        [Test]
        public void SetExpiryDateBySecondsWorksWithVeryShortPeriod()
        {
            CloudStorageToken token = new CloudStorageToken();
            token.SetExpiryDateBySecondsFromNow(8);

            Assert.IsNotNull(token.ExpiryDate);
            Assert.IsTrue(token.ExpiryDate > DateTime.Now);
            Assert.IsTrue(token.ExpiryDate < DateTime.Now.AddSeconds(8));
        }

        [Test]
        public void SetExpiryDateBySecondsWorksWithNull()
        {
            CloudStorageToken token = new CloudStorageToken();
            token.SetExpiryDateBySecondsFromNow(null);

            Assert.IsNull(token.ExpiryDate);
        }

        [Test]
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

        [Test]
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

        [Test]
        public void NeedsRefreshReturnsTrueIfExpired()
        {
            CloudStorageToken token = new CloudStorageToken { RefreshToken = "notempty" };
            token.ExpiryDate = DateTime.Now.AddSeconds(-8);
            Assert.IsTrue(token.NeedsRefresh());
        }

        [Test]
        public void NeedsRefreshReturnsFalseIfNotExpired()
        {
            CloudStorageToken token = new CloudStorageToken { RefreshToken = "notempty" };
            token.ExpiryDate = DateTime.Now.AddSeconds(8);
            Assert.IsFalse(token.NeedsRefresh());
        }

        [Test]
        public void NeedsRefreshReturnsTrueIfNoExpirationDate()
        {
            CloudStorageToken token = new CloudStorageToken { RefreshToken = "notempty" };
            Assert.IsTrue(token.NeedsRefresh());
        }

        [Test]
        public void NeedsRefreshReturnsFalseForTokenFlow()
        {
            // The token flow has no refresh token
            CloudStorageToken token = new CloudStorageToken { RefreshToken = null };
            token.ExpiryDate = DateTime.Now.AddSeconds(-8); // Would have been expired
            Assert.IsFalse(token.NeedsRefresh());
        }
    }
}
