using System.Threading.Tasks;
using NUnit.Framework;
using SilentNotes;
using SilentNotes.Services;
using SilentNotes.Services.CloudStorageServices;
using SilentNotes.Workers;

namespace SilentNotesTest.Services.CloudStorageServices
{
    [TestFixture]
    [Ignore("Slow integration test, start only on demand, insert real password.")]
    public class GmxCloudStorageServiceTest
    {
        private const string USERNAME = "silentnotesunittest@gmx.net";
        private const string PASSWORD = "unrealpassword";
        private ICryptoRandomService _randomSource;

        [OneTimeSetUp]
        public void TestSetup()
        {
            _randomSource = new RandomSource4UnitTest();
        }

        [Test]
        public async Task CloudStorageGmxCanUploadAndDownload()
        {
            byte[] originalContent = _randomSource.GetRandomBytes(16);

            ICloudStorageService service = new GmxCloudStorageService();
            service.Account.Username = USERNAME;
            service.Account.Password = SecureStringExtensions.StringToSecureString(PASSWORD);

            // Upload
            await service.UploadRepositoryAsync(originalContent);

            // Exists
            bool fileExists = await service.ExistsRepositoryAsync();
            Assert.IsTrue(fileExists);

            // Download
            byte[] downloadedContent = await service.DownloadRepositoryAsync();
            Assert.AreEqual(originalContent, downloadedContent);
        }

        [Test]
        public async Task CloudStorageGmxCheckMissingRepository()
        {
            ICloudStorageService service = new GmxCloudStorageService();
            service.Account.Username = USERNAME;
            service.Account.Password = SecureStringExtensions.StringToSecureString(PASSWORD);
            service.Account.Url = "https://webdav.mc.gmx.net/emptydirectory/";

            Assert.IsFalse(await service.ExistsRepositoryAsync());
        }

        [Test]
        public void CloudStorageGmxThrowsAuthenticationExceptionWithInvalidUsername()
        {
            ICloudStorageService service = new GmxCloudStorageService();
            service.Account.Username = USERNAME;
            service.Account.Password = SecureStringExtensions.StringToSecureString(PASSWORD + "invalid");

            Assert.ThrowsAsync<CloudStorageForbiddenException>(() => service.DownloadRepositoryAsync());
        }
    }
}
