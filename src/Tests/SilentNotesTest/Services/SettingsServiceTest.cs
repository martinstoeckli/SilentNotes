using System.Linq;
using System.Security;
using System.Text;
using System.Xml.Linq;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Crypto;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;
using VanillaCloudStorageClient;

namespace SilentNotesTest.Services
{
    [TestClass]
    public class SettingsServiceTest
    {
        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        public void Version1ConfigWillBeUpdated()
        {
            XDocument xml = XDocument.Parse(Version1Settings);
            Mock<IXmlFileService> fileService = new Mock<IXmlFileService>();
            fileService.
                Setup(m => m.TryLoad(It.IsAny<string>(), out xml)).
                Returns(true);
            Mock<IDataProtectionService> dataProtectionService = new Mock<IDataProtectionService>();

            dataProtectionService.
                Setup(m => m.Protect(It.Is<byte[]>(p => p.SequenceEqual(Encoding.ASCII.GetBytes("abcdefgh"))))).
                Returns("protected_abcdefgh_v2");
            dataProtectionService.
                Setup(m => m.Unprotect(It.Is<string>(p => p == "protected_abcdefgh_v2"))).
                Returns(SecureStringExtensions.SecureStringToBytes(SecureStringExtensions.StringToSecureString("abcdefgh_v2"), Encoding.Unicode));
            dataProtectionService.
                Setup(m => m.Protect(It.Is<byte[]>(p => p.SequenceEqual(Encoding.UTF8.GetBytes("abcdefgh_v2"))))).
                Returns("protected_abcdefgh_v3");
            dataProtectionService.
                Setup(m => m.Unprotect(It.Is<string>(p => p == "protected_abcdefgh_v3"))).
                Returns(Encoding.UTF8.GetBytes("abcdefgh_v3"));

            dataProtectionService.
                Setup(m => m.Protect(It.Is<byte[]>(p => p.SequenceEqual(Encoding.UTF8.GetBytes("martinstoeckli"))))).
                Returns("protected_martinstoeckli_v3");
            dataProtectionService.
                Setup(m => m.Unprotect(It.Is<string>(p => p == "protected_martinstoeckli_v3"))).
                Returns(Encoding.UTF8.GetBytes("martinstoeckli_v3"));

            SettingsServiceBase service = new TestableService(fileService.Object, dataProtectionService.Object);
            SettingsModel settings = service.LoadSettingsOrDefault();
            Assert.AreEqual("twofish_gcm", settings.SelectedEncryptionAlgorithm);
            Assert.AreEqual("scuj2wfpdcodmgzm", settings.TransferCode);
            Assert.AreEqual(2, settings.TransferCodeHistory.Count);
            Assert.AreEqual("scuj2wfpdcodmgzm", settings.TransferCodeHistory[0]);
            Assert.AreEqual("b2bgiqeghfvn2ufx", settings.TransferCodeHistory[1]);

            Assert.AreEqual(SettingsModel.NewestSupportedRevision, settings.Revision);
            Assert.IsNotNull(settings.Credentials);
            Assert.AreEqual(CloudStorageClientFactory.CloudStorageIdWebdav, settings.Credentials.CloudStorageId);
            Assert.AreEqual("https://webdav.hidrive.strato.com/users/martinstoeckli/", settings.Credentials.Url);
            Assert.AreEqual("martinstoeckli_v3", settings.Credentials.Username);
            Assert.AreEqual("abcdefgh_v3", settings.Credentials.UnprotectedPassword);

            // Updated settings where saved
            fileService.Verify(m => m.TrySerializeAndSave(It.IsAny<string>(), It.IsAny<SettingsModel>()), Times.Once);
        }

        [TestMethod]
        public void Version2ConfigWillBeUpdated()
        {
            XDocument xml = XDocument.Parse(Version2Settings);
            Mock<IXmlFileService> fileService = new Mock<IXmlFileService>();
            fileService.
                Setup(m => m.TryLoad(It.IsAny<string>(), out xml)).
                Returns(true);
            Mock<IDataProtectionService> dataProtectionService = new Mock<IDataProtectionService>();

            dataProtectionService.
                Setup(m => m.Unprotect(It.Is<string>(p => p == "this_is_a_protected_password"))).
                Returns(SecureStringExtensions.SecureStringToBytes(SecureStringExtensions.StringToSecureString("abcdefgh_v2"), Encoding.Unicode));
            dataProtectionService.
                Setup(m => m.Protect(It.Is<byte[]>(p => p.SequenceEqual(Encoding.UTF8.GetBytes("abcdefgh_v2"))))).
                Returns("protected_abcdefgh_v3");
            dataProtectionService.
                Setup(m => m.Unprotect(It.Is<string>(p => p == "protected_abcdefgh_v3"))).
                Returns(Encoding.UTF8.GetBytes("abcdefgh_v3"));

            dataProtectionService.
                Setup(m => m.Protect(It.Is<byte[]>(p => p.SequenceEqual(Encoding.UTF8.GetBytes("martinstoeckli"))))).
                Returns("protected_martinstoeckli_v3");
            dataProtectionService.
                Setup(m => m.Unprotect(It.Is<string>(p => p == "protected_martinstoeckli_v3"))).
                Returns(Encoding.UTF8.GetBytes("martinstoeckli_v3"));

            SettingsServiceBase service = new TestableService(fileService.Object, dataProtectionService.Object);
            SettingsModel settings = service.LoadSettingsOrDefault();
            Assert.AreEqual("twofish_gcm", settings.SelectedEncryptionAlgorithm);
            Assert.AreEqual("scuj2wfpdcodmgzm", settings.TransferCode);
            Assert.AreEqual(0, settings.TransferCodeHistory.Count);

            Assert.AreEqual(SettingsModel.NewestSupportedRevision, settings.Revision);
            Assert.IsNotNull(settings.Credentials);
            Assert.AreEqual(CloudStorageClientFactory.CloudStorageIdWebdav, settings.Credentials.CloudStorageId);
            Assert.AreEqual("https://webdav.hidrive.strato.com/users/martinstoeckli/", settings.Credentials.Url);
            Assert.AreEqual("martinstoeckli_v3", settings.Credentials.Username);
            Assert.AreEqual("abcdefgh_v3", settings.Credentials.UnprotectedPassword);

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
                base.UpdateSettingsFrom1To2(root);
                SecureString snpsk = CryptoUtils.StringToSecureString("53EC49B1-6600+406b;B84F-0B9CFA1D2BE1");

                // Handle protected password
                XElement oldPasswortElement = root.Element("cloud_storage")?.Element("cloud_password");
                XElement cloudStorageAccount = root.Element("cloud_storage_account");
                if ((oldPasswortElement != null) && (cloudStorageAccount != null))
                {
                    // Deobfuscate old password
                    ICryptor decryptor = new Cryptor("snps", null);
                    byte[] binaryCipher = CryptoUtils.Base64StringToBytes(oldPasswortElement.Value);
                    byte[] unprotectedBinaryPassword = decryptor.Decrypt(binaryCipher, snpsk, out _);

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

        private const string Version2Settings =
@"<?xml version='1.0' encoding='utf-8'?>
<silentnotes_settings xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' revision='2'>
  <cloud_storage_account>
    <username>martinstoeckli</username>
    <protected_password>this_is_a_protected_password</protected_password>
    <url>https://webdav.hidrive.strato.com/users/martinstoeckli/</url>
    <cloud_type>WebDAV</cloud_type>
  </cloud_storage_account>
  <selected_theme>forest</selected_theme>
  <selected_encryption_algorithm>twofish_gcm</selected_encryption_algorithm>
  <adopt_cloud_encryption_algorithm>true</adopt_cloud_encryption_algorithm>
  <auto_sync_mode>CostFreeInternetOnly</auto_sync_mode>
  <transfer_code>scuj2wfpdcodmgzm</transfer_code>
  <transfer_code_history />
  <show_cursor_keys>true</show_cursor_keys>
  <font-scale>1</font-scale>
  <default_note_color>#fbf4c1</default_note_color>
</silentnotes_settings>";
    }
}
