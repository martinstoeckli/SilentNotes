using NUnit.Framework;
using SilentNotes.Services.CloudStorageServices;
using SilentNotes.Workers;
using System.Xml.Linq;

namespace SilentNotesTest.Services.CloudStorageServices
{
    [TestFixture]
    public class CloudStorageAccountTest
    {
        [Test]
        public void IsSerializable()
        {
            CloudStorageAccount account = new CloudStorageAccount
            {
                CloudType = CloudStorageType.GMX,
                OauthAccessToken = "abc",
                OauthRefreshToken = "def",
                ProtectedPassword = "uhu",
                Url = "url",
                Username = "usr",
            };
            byte[] serializedAccount = XmlUtils.SerializeToXmlBytes(account);
            XDocument deserializedAccountXml = XmlUtils.LoadFromXmlBytes(serializedAccount);
            CloudStorageAccount deserializedAccount = XmlUtils.DeserializeFromXmlDocument<CloudStorageAccount>(deserializedAccountXml);
            Assert.AreEqual(CloudStorageType.GMX, deserializedAccount.CloudType);
            Assert.AreEqual("abc", deserializedAccount.OauthAccessToken);
            Assert.AreEqual("def", deserializedAccount.OauthRefreshToken);
            Assert.AreEqual("uhu", deserializedAccount.ProtectedPassword);
            Assert.AreEqual("url", deserializedAccount.Url);
            Assert.AreEqual("usr", deserializedAccount.Username);
        }
    }
}
