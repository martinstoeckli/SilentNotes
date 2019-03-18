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
    public class FtpCloudStorageServiceTest
    {
        private const string URL = "ftp://sl287.web.hostpoint.ch/";
        private const string USERNAME = "silentnotes@martinstoeckli.ch";
        private const string PASSWORD = "unrealpassword";
        private ICryptoRandomService _randomSource;

        [OneTimeSetUp]
        public void TestSetup()
        {
            _randomSource = new RandomSource4UnitTest();
        }

        [Test]
        public async Task CloudStorageFtpCanUploadAndDownload()
        {
            byte[] originalContent = _randomSource.GetRandomBytes(16);

            ICloudStorageService service = new FtpCloudStorageService();
            service.Account.Url = URL;
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
        public async Task CloudStorageFtpCheckMissingRepository()
        {
            ICloudStorageService service = new FtpCloudStorageService();
            service.Account.Url = "ftp://sl287.web.hostpoint.ch/empty/";
            service.Account.Username = USERNAME;
            service.Account.Password = SecureStringExtensions.StringToSecureString(PASSWORD);

            Assert.IsFalse(await service.ExistsRepositoryAsync());
        }

        [Test]
        [Ignore("A successful login immediately before, will prevent the excepted exception.")]
        public void CloudStorageFtpThrowsAuthenticationExceptionWithInvalidUsername()
        {
            ICloudStorageService service = new FtpCloudStorageService();
            service.Account.Url = URL;
            service.Account.Username = USERNAME;
            service.Account.Password = SecureStringExtensions.StringToSecureString(PASSWORD + "invalid");

            Assert.ThrowsAsync<CloudStorageForbiddenException>(() => service.DownloadRepositoryAsync());
        }

        //[Test]
        //public void ThrowsConnectionException()
        //{
        //    ICloudStorageService service = new FtpCloudStorageService();
        //    service.SetLoginData("ftp://local_g_host/", USERNAME, PASSWORD);
        //    Assert.ThrowsAsync<CloudStorageConnectionException>(() => service.DownloadRepositoryAsync());

        //    service.SetLoginData("a", USERNAME, PASSWORD);
        //    Assert.ThrowsAsync<CloudStorageConnectionException>(() => service.ExistsRepositoryAsync());
        //}

        //[Test]
        //public async Task CheckMissingRepository()
        //{
        //    ICloudStorageService service = new FtpCloudStorageService();
        //    service.SetLoginData("ftp://localhost/empty/", USERNAME, PASSWORD);
        //    Assert.IsFalse(await service.ExistsRepositoryAsync());
        //}

        //[Test]
        //public void ThrowsWhenNoListing()
        //{
        //    ICloudStorageService service = new FtpCloudStorageService();
        //    service.SetLoginData("ftp://localhost/nolisting/", USERNAME, PASSWORD);
        //    Assert.ThrowsAsync<CloudStorageForbiddenException>(() => service.ExistsRepositoryAsync());
        //}

        //[Test]
        //public void ThrowsAuthenticationExceptionWithInvalidUsername()
        //{
        //    ICloudStorageService service = new FtpCloudStorageService();
        //    service.SetLoginData("ftp://localhost/", "sugus", PASSWORD);
        //    Assert.ThrowsAsync<CloudStorageForbiddenException>(() => service.DownloadRepositoryAsync());
        //}

        ////[Test]
        ////public void ThrowsAuthenticationExceptionWithInvalidPassword()
        ////{
        ////    ICloudStorageService service = new FtpCloudStorageService();
        ////    service.SetLoginData("ftp://localhost/", USERNAME, "");
        ////    Assert.ThrowsAsync<CloudStorageAuthenticationException>(() => service.DownloadRepositoryAsync());
        ////}

        //[Test]
        //public void ThrowsMissingPrivilegesExceptionWhenNoRead()
        //{
        //    ICloudStorageService service = new FtpCloudStorageService();
        //    service.SetLoginData("ftp://localhost/noread/", USERNAME, PASSWORD);
        //    Assert.ThrowsAsync<CloudStorageForbiddenException>(() => service.DownloadRepositoryAsync());
        //}

        //[Test]
        //public void ThrowsMissingPrivilegesExceptionWhenNoWrite()
        //{
        //    ICloudStorageService service = new FtpCloudStorageService();
        //    service.SetLoginData("ftp://localhost/nowrite/", USERNAME, PASSWORD);
        //    byte[] repository = new byte[] { 44, 55, 66 };
        //    Assert.ThrowsAsync<CloudStorageForbiddenException>(() => service.UploadRepositoryAsync(repository));
        //}
    }
}
