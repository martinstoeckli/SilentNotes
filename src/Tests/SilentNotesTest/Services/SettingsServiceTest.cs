using System.Xml.Linq;
using Moq;
using NUnit.Framework;
using SilentNotes.Crypto;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Services.CloudStorageServices;
using SilentNotes.Workers;

namespace SilentNotesTest.Services
{
    [TestFixture]
    public class SettingsServiceTest
    {
        [Test]
        public void LoadsSettingsReturnsStoredSettings()
        {
            SettingsModel storedSettings = new SettingsModel { TransferCode = "abcdefgh" };
            XDocument xml = XmlUtils.SerializeToXmlDocument(storedSettings);
            Mock<IXmlFileService> fileService = new Mock<IXmlFileService>();
            fileService.
                Setup(m => m.TryLoad(It.IsAny<string>(), out xml)).
                Returns(true);
            Mock<IDataProtectionService> dataProtectionService = new Mock<IDataProtectionService>();

            SettingsServiceBase service = new TestableService(fileService.Object, dataProtectionService.Object);
            SettingsModel settings = service.LoadSettingsOrDefault();

            // Loaded existing settings and did not store it
            Assert.IsNotNull(settings);
            Assert.AreEqual("abcdefgh", settings.TransferCode);
            fileService.Verify(m => m.TrySerializeAndSave(It.IsAny<string>(), It.IsAny<SettingsModel>()), Times.Never);
        }

        [Test]
        public void LoadSettingsCreatesDefaultIfNoFileFound()
        {
            XDocument xml = null;
            Mock<IXmlFileService> fileService = new Mock<IXmlFileService>();
            fileService.
                Setup(m => m.TryLoad(It.IsAny<string>(), out xml)).
                Returns(false);
            Mock<IDataProtectionService> dataProtectionService = new Mock<IDataProtectionService>();

            SettingsServiceBase service = new TestableService(fileService.Object, dataProtectionService.Object);
            SettingsModel settings = service.LoadSettingsOrDefault();

            // Created new settings and stored it
            Assert.IsNotNull(settings);
            fileService.Verify(m => m.TrySerializeAndSave(It.IsAny<string>(), It.IsAny<SettingsModel>()), Times.Once);
        }

        [Test]
        public void Version1ConfigWillBeUpdated()
        {
            XDocument xml = XDocument.Parse(Version1Settings);
            Mock<IXmlFileService> fileService = new Mock<IXmlFileService>();
            fileService.
                Setup(m => m.TryLoad(It.IsAny<string>(), out xml)).
                Returns(true);
            Mock<IDataProtectionService> dataProtectionService = new Mock<IDataProtectionService>();
            dataProtectionService.
                Setup(m => m.Protect(It.IsAny<byte[]>())).
                Returns("abcdefgh");
            dataProtectionService.
                Setup(m => m.Unprotect(It.IsAny<string>())).
                Returns(SecureStringExtensions.SecureStringToUnicodeBytes(SecureStringExtensions.StringToSecureString("brownie")));

            SettingsServiceBase service = new TestableService(fileService.Object, dataProtectionService.Object);
            SettingsModel settings = service.LoadSettingsOrDefault();
            Assert.AreEqual("twofish_gcm", settings.SelectedEncryptionAlgorithm);
            Assert.AreEqual(true, settings.AdoptCloudEncryptionAlgorithm);
            Assert.AreEqual("scuj2wfpdcodmgzm", settings.TransferCode);
            Assert.AreEqual(2, settings.TransferCodeHistory.Count);
            Assert.AreEqual("scuj2wfpdcodmgzm", settings.TransferCodeHistory[0]);
            Assert.AreEqual("b2bgiqeghfvn2ufx", settings.TransferCodeHistory[1]);

            Assert.AreEqual(SettingsModel.NewestSupportedRevision, settings.Revision);
            Assert.IsNotNull(settings.CloudStorageAccount);
            Assert.AreEqual(CloudStorageType.WebDAV, settings.CloudStorageAccount.CloudType);
            Assert.AreEqual("https://webdav.hidrive.strato.com/users/martinstoeckli/", settings.CloudStorageAccount.Url);
            Assert.AreEqual("martinstoeckli", settings.CloudStorageAccount.Username);
            Assert.AreEqual("brownie", SecureStringExtensions.SecureStringToString(settings.CloudStorageAccount.Password));

            // Updated settings where saved
            fileService.Verify(m => m.TrySerializeAndSave(It.IsAny<string>(), It.IsAny<SettingsModel>()), Times.Once);
        }

        /// <summary>
        /// Make abstract class testable
        /// </summary>
        private class TestableService : SettingsServiceBase
        {
            public TestableService(IXmlFileService xmlFileService, IDataProtectionService dataProtectionService)
                : base(xmlFileService, dataProtectionService)
            {
            }

            protected override string GetDirectoryPath()
            {
                return string.Empty;
            }

            /// <summary>
            /// Test the password update from Version1 in Android to Version2
            /// </summary>
            /// <param name="root"></param>
            protected override void UpdateSettingsFrom1To2(XElement root)
            {
                const string snpsk = "53EC49B1-6600+406b;B84F-0B9CFA1D2BE1";
                base.UpdateSettingsFrom1To2(root);

                // Handle protected password
                XElement oldPasswortElement = root.Element("cloud_storage")?.Element("cloud_password");
                XElement cloudStorageAccount = root.Element("cloud_storage_account");
                if ((oldPasswortElement != null) && (cloudStorageAccount != null))
                {
                    // Deobfuscate old password
                    EncryptorDecryptor decryptor = new EncryptorDecryptor("snps");
                    byte[] binaryCipher = CryptoUtils.Base64StringToBytes(oldPasswortElement.Value);
                    byte[] unprotectedBinaryPassword = decryptor.Decrypt(binaryCipher, snpsk);

                    // Protect with new data protection service and add to XML
                    string protectedPassword = _dataProtectionService.Protect(unprotectedBinaryPassword);
                    cloudStorageAccount.Add(new XElement("protected_password", protectedPassword));
                    CryptoUtils.CleanArray(unprotectedBinaryPassword);
                }
            }
        }

        private const string Version1Settings =
@"<?xml version='1.0' encoding='utf-8'?>
<silentnotes_settings xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
  <cloud_storage>
    <cloud_type>WebDAV</cloud_type>
    <cloud_url>https://webdav.hidrive.strato.com/users/martinstoeckli/</cloud_url>
    <cloud_username>martinstoeckli</cloud_username>
    <cloud_password>c25wcyRhZXNfZ2NtJGRaVlVWeWtYZUhMaGp2TzcrcHpIc1E9PSRwYmtkZjIkYXRDZDM3b1ZGSHFrdWcyZnpYZ1J6UT09JDEwMDAkkO8wfiy/8u7QsGzLI8y33sU24elOuQhh</cloud_password>
  </cloud_storage>
  <selected_encryption_algorithm>twofish_gcm</selected_encryption_algorithm>
  <adopt_cloud_encryption_algorithm>true</adopt_cloud_encryption_algorithm>
  <transfer_code>scuj2wfpdcodmgzm</transfer_code>
  <transfer_code_history>
    <transfer_code>scuj2wfpdcodmgzm</transfer_code>
    <transfer_code>b2bgiqeghfvn2ufx</transfer_code>
  </transfer_code_history>
</silentnotes_settings>";
    }
}
