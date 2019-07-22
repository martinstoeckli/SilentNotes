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
