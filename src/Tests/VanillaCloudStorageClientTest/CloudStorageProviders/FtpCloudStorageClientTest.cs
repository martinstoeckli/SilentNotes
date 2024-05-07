using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using VanillaCloudStorageClient;
using VanillaCloudStorageClient.CloudStorageProviders;

#pragma warning disable CS0162 // Unreachable code detected
namespace VanillaCloudStorageClientTest.CloudStorageProviders
{
    [TestFixture]
    public class FtpCloudStorageClientTest
    {
        /// <summary>
        /// Setting <see cref="DoRealWebRequests"/> to true, causes the unit tests to act as
        /// integration tests, doing real web requests to the server. The constants
        /// • <see cref="AuthorizationUrl"/>
        /// • <see cref="AuthorizationUsername"/>
        /// • <see cref="AuthorizationPassword"/>
        /// must then be set to valid credentials.
        /// Hint: The test also works with URLs including a directory.
        /// </summary>
        private const bool DoRealWebRequests = false;
        private const string AuthorizationUrl = "ftp://example.com";
        private const string AuthorizationUsername = "user@example.com";
        private const string AuthorizationPassword = "ValidPassword";
        private const bool UseSecureSsl = false;
        private const bool AcceptInvalidCertificate = false;
        [Test]
        public void FileLifecycleWorks()
        {
            string fileName = "unittest.dat";
            byte[] fileContent = new byte[16];
            new Random().NextBytes(fileContent);

            // 1) Test upload
            IFtpFakeResponse fakeResponse = null;
            if (!DoRealWebRequests)
            {
                Mock<IFtpFakeResponse> fakeResponseMock = new Mock<IFtpFakeResponse>();
                fakeResponse = fakeResponseMock.Object;
            }
            Assert.DoesNotThrowAsync(() => UploadFileWorksAsync(fileName, fileContent, fakeResponse));

            // 2) Test listing
            if (!DoRealWebRequests)
            {
                Mock<IFtpFakeResponse> fakeResponseMock = new Mock<IFtpFakeResponse>();
                fakeResponseMock.
                    Setup(m => m.GetFakeServerResponseString(It.Is<string>(r => r == AuthorizationUrl))).
                    Returns(".\r\n..\r\nXyz\r\nunittest.dat\r\nb.txt\r\nc.txt\r\n");
                fakeResponse = fakeResponseMock.Object;
            }
            List<string> res = Task.Run(async () => await ListFileNamesWorksAsync(GetCredentials(), fakeResponse)).Result;
            Assert.IsTrue(res.Count >= 1);
            Assert.IsTrue(res.Contains("unittest.dat"));

            // 3) Test exists
            if (!DoRealWebRequests)
            {
                Mock<IFtpFakeResponse> fakeResponseMock = new Mock<IFtpFakeResponse>();
                fakeResponseMock.
                    Setup(m => m.GetFakeServerExistsFile(It.Is<string>(r => r == AuthorizationUrl))).
                    Returns(true);
                fakeResponse = fakeResponseMock.Object;
            }
            bool exists = Task.Run(async () => await FileExistsWorksAsync(fileName, GetCredentials(), fakeResponse)).Result;
            Assert.IsTrue(exists);

            // 4) Test download
            if (!DoRealWebRequests)
            {
                Mock<IFtpFakeResponse> fakeResponseMock = new Mock<IFtpFakeResponse>();
                fakeResponseMock
                    .Setup(m => m.GetFakeServerResponseBytes(It.IsAny<string>()))
                    .Returns(fileContent);
                fakeResponse = fakeResponseMock.Object;
            }
            Byte[] downloadedContent = Task.Run(async () => await DownloadFileWorksAsync(fileName, GetCredentials(), fakeResponse)).Result;
            Assert.AreEqual(fileContent, downloadedContent);

            // 5) Test delete
            if (!DoRealWebRequests)
            {
                Mock<IFtpFakeResponse> fakeResponseMock = new Mock<IFtpFakeResponse>();
                fakeResponse = fakeResponseMock.Object;
            }
            Assert.DoesNotThrowAsync(() => DeleteFileWorksAsync(fileName, GetCredentials(), fakeResponse));

            // 6) Was really deleted?
            if (!DoRealWebRequests)
            {
                Mock<IFtpFakeResponse> fakeResponseMock = new Mock<IFtpFakeResponse>();
                fakeResponseMock.
                    Setup(m => m.GetFakeServerExistsFile(It.Is<string>(r => r == AuthorizationUrl))).
                    Returns(false);
                fakeResponse = fakeResponseMock.Object;
            }
            exists = Task.Run(async () => await FileExistsWorksAsync(fileName, GetCredentials(), fakeResponse)).Result;
            Assert.IsFalse(exists);
        }

        private async Task UploadFileWorksAsync(string fileName, byte[] fileContent, IFtpFakeResponse fakeResponse)
        {
            ICloudStorageClient client = new FtpCloudStorageClient(fakeResponse);
            CloudStorageCredentials credentials = GetCredentials();
            await client.UploadFileAsync(fileName, fileContent, credentials);
        }

        private async Task<List<string>> ListFileNamesWorksAsync(CloudStorageCredentials credentials, IFtpFakeResponse fakeResponse)
        {
            ICloudStorageClient client = new FtpCloudStorageClient(fakeResponse);
            List<string> result = await client.ListFileNamesAsync(credentials);
            return result;
        }

        private async Task<bool> FileExistsWorksAsync(string fileName, CloudStorageCredentials credentials, IFtpFakeResponse fakeResponse)
        {
            ICloudStorageClient client = new FtpCloudStorageClient(fakeResponse);
            bool result = await client.ExistsFileAsync(fileName, credentials);
            return result;
        }

        private async Task<byte[]> DownloadFileWorksAsync(string fileName, CloudStorageCredentials credentials, IFtpFakeResponse fakeResponse)
        {
            ICloudStorageClient client = new FtpCloudStorageClient(fakeResponse);
            byte[] result = await client.DownloadFileAsync(fileName, credentials);
            return result;
        }

        private async Task DeleteFileWorksAsync(string fileName, CloudStorageCredentials credentials, IFtpFakeResponse fakeResponse)
        {
            ICloudStorageClient client = new FtpCloudStorageClient(fakeResponse);
            await client.DeleteFileAsync(fileName, credentials);
        }

        [Test]
        [Ignore("Too many consecutive fails seems to block an FTP server.")]
        public void ThrowsWithInvalidUsername()
        {
            // Unfortunately there is no way to mock a the FtpWebResponse for a WebException, so we
            // can do only real web requests.
            if (DoRealWebRequests)
            {
                var credentials = GetCredentials();
                credentials.Username = "youdontknowme";
                Assert.ThrowsAsync<AccessDeniedException>(() => DownloadFileWorksAsync("a.txt", credentials, null));
            }
        }

        [Test]
        [Ignore("Too many consecutive fails seems to block an FTP server.")]
        public void ThrowsWithInvalidPassword()
        {
            // Unfortunately there is no way to mock a the FtpWebResponse for a WebException, so we
            // can do only real web requests.
            if (DoRealWebRequests)
            {
                var credentials = GetCredentials();
                credentials.UnprotectedPassword = "youdontknowme";
                Assert.ThrowsAsync<AccessDeniedException>(() => DownloadFileWorksAsync("a.txt", credentials, null));
            }
        }

        [Test]
        [Ignore("Too many consecutive fails seems to block an FTP server.")]
        public void ThrowsWithInvalidUrl()
        {
            // Unfortunately there is no way to mock a the FtpWebResponse for a WebException, so we
            // can do only real web requests.
            if (DoRealWebRequests)
            {
                var credentials = GetCredentials();
                string nonExistingFile = "nonexisting.non";
                Assert.ThrowsAsync<ConnectionFailedException>(() => DownloadFileWorksAsync(nonExistingFile, credentials, null));
            }
        }

        [Test]
        [Ignore("Too many consecutive fails seems to block an FTP server.")]
        public void ThrowsWithInvalidHost()
        {
            // Unfortunately there is no way to mock a the FtpWebResponse for a WebException, so we
            // can do only real web requests.
            if (DoRealWebRequests)
            {
                var credentials = GetCredentials();
                credentials.Url = "ftp://sl287.web.hostpoint.c";
                string nonExistingFile = "nonexisting.non";
                Assert.ThrowsAsync<ConnectionFailedException>(() => DownloadFileWorksAsync(nonExistingFile, credentials, null));
            }
        }

        private static CloudStorageCredentials GetCredentials()
        {
            return new CloudStorageCredentials
            {
                Url = AuthorizationUrl,
                Username = AuthorizationUsername,
                UnprotectedPassword = AuthorizationPassword,
                Secure = UseSecureSsl,
                AcceptInvalidCertificate = AcceptInvalidCertificate,
            };
        }
    }
}
