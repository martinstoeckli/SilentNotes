using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Flurl.Http.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VanillaCloudStorageClient;
using VanillaCloudStorageClient.CloudStorageProviders;

namespace VanillaCloudStorageClientTest.CloudStorageProviders
{
    [TestClass]
    public class MurenaCloudStorageClientTest
    {
        /// <summary>
        /// Setting <see cref="DoRealWebRequests"/> to true, causes the unit tests to act as
        /// integration tests, doing real web requests to the API. The constants
        /// • <see cref="AuthorizationUsername"/>
        /// • <see cref="AuthorizationPassword"/>
        /// must then be set to valid credentials.
        /// Possible test server for anonymous requests: http://webdavserver.com
        /// </summary>
        private const bool DoRealWebRequests = false;
        private const string AuthorizationUsername = "userX@e.email";
        private const string AuthorizationPassword = "passwordX";
        private HttpTest _httpTest;

        [TestInitialize]
        public void CreateHttpTest()
        {
            if (!DoRealWebRequests)
                _httpTest = new HttpTest(); // Put flurl into test mode
        }

        [TestCleanup]
        public void DisposeHttpTest()
        {
            if (!DoRealWebRequests)
                _httpTest.Dispose();
        }

        [TestMethod]
        public async Task FileLifecycleWorks()
        {
            string fileName = "unittest.dat";
            byte[] fileContent = new byte[16];
            new Random().NextBytes(fileContent);

            // 1) Test upload
            await UploadFileWorksAsync(fileName, fileContent);

            // 2) Test listing
            if (!DoRealWebRequests)
            {
                _httpTest.RespondWith(GetWebdavFileListResponse());
            }
            List<string> res = await ListFileNamesWorksAsync();
            Assert.IsTrue(res.Count >= 1);
            Assert.IsTrue(res.Contains("unittest.dat"));

            // 3) Test exists
            if (!DoRealWebRequests)
            {
                _httpTest.RespondWith(GetWebdavFileListResponse());
            }
            bool exists = await FileExistsWorksAsync(fileName);
            Assert.IsTrue(exists);

            // 4) Test download
            if (!DoRealWebRequests)
            {
                HttpContent httpContent = new ByteArrayContent(fileContent);
                _httpTest.RespondWith(() => httpContent);
            }
            Byte[] downloadedContent = await DownloadFileWorksAsync(fileName);
            CollectionAssert.AreEqual(fileContent, downloadedContent);

            // 5) Test delete
            if (!DoRealWebRequests)
            {
                _httpTest.RespondWith(() => new ByteArrayContent(new byte[0]));
            }
            await DeleteFileWorksAsync(fileName);

            // 6) Was really deleted?
            if (!DoRealWebRequests)
            {
                _httpTest.RespondWith(GetWebdavEmptyFileListResponse());
            }
            exists = await FileExistsWorksAsync(fileName);
            Assert.IsFalse(exists);
        }

        private async Task UploadFileWorksAsync(string fileName, byte[] fileContent)
        {
            ICloudStorageClient client = new MurenaCloudStorageClient(false);
            await client.UploadFileAsync(fileName, fileContent, GetCredentials());
        }

        private async Task<byte[]> DownloadFileWorksAsync(string fileName, CloudStorageCredentials credentials = null)
        {
            ICloudStorageClient client = new MurenaCloudStorageClient(false);
            if (credentials == null)
                credentials = GetCredentials();
            return await client.DownloadFileAsync(fileName, credentials);
        }

        private async Task DeleteFileWorksAsync(string fileName)
        {
            ICloudStorageClient client = new MurenaCloudStorageClient(false);
            await client.DeleteFileAsync(fileName, GetCredentials());
        }

        private async Task<List<string>> ListFileNamesWorksAsync()
        {
            ICloudStorageClient client = new MurenaCloudStorageClient(false);
            List<string> result = await client.ListFileNamesAsync(GetCredentials());
            return result;
        }

        private async Task<bool> FileExistsWorksAsync(string filename)
        {
            ICloudStorageClient client = new MurenaCloudStorageClient(false);
            return await client.ExistsFileAsync(filename, GetCredentials());
        }


        private string GetWebdavFileListResponse()
        {
            return
@"<d:multistatus xmlns:d='DAV:' xmlns:s='http://sabredav.org/ns' xmlns:oc='http://owncloud.org/ns' xmlns:nc='http://nextcloud.org/ns'>
  <d:response>
    <d:href>/remote.php/dav/files/test@e.email/</d:href>
    <d:propstat>
      <d:prop>
        <d:resourcetype>
          <d:collection />
        </d:resourcetype>
      </d:prop>
      <d:status>HTTP/1.1 200 OK</d:status>
    </d:propstat>
  </d:response>
  <d:response>
    <d:href>/remote.php/dav/files/test@e.email/Documents/</d:href>
    <d:propstat>
      <d:prop>
        <d:resourcetype>
          <d:collection />
        </d:resourcetype>
      </d:prop>
      <d:status>HTTP/1.1 200 OK</d:status>
    </d:propstat>
  </d:response>
  <d:response>
    <d:href>/remote.php/dav/files/test@e.email/unittest.dat</d:href>
    <d:propstat>
      <d:prop>
        <d:resourcetype />
      </d:prop>
      <d:status>HTTP/1.1 200 OK</d:status>
    </d:propstat>
  </d:response>
</d:multistatus>";
        }

        private string GetWebdavEmptyFileListResponse()
        {
            return
@"<d:multistatus xmlns:d='DAV:' xmlns:s='http://sabredav.org/ns' xmlns:oc='http://owncloud.org/ns' xmlns:nc='http://nextcloud.org/ns'>
  <d:response>
    <d:href>/remote.php/dav/files/test@e.email/</d:href>
    <d:propstat>
      <d:prop>
        <d:resourcetype>
          <d:collection />
        </d:resourcetype>
      </d:prop>
      <d:status>HTTP/1.1 200 OK</d:status>
    </d:propstat>
  </d:response>
  <d:response>
    <d:href>/remote.php/dav/files/test@e.email/Documents/</d:href>
    <d:propstat>
      <d:prop>
        <d:resourcetype>
          <d:collection />
        </d:resourcetype>
      </d:prop>
      <d:status>HTTP/1.1 200 OK</d:status>
    </d:propstat>
  </d:response>
</d:multistatus>";
        }

        private static CloudStorageCredentials GetCredentials()
        {
            return new CloudStorageCredentials
            {
                Username = AuthorizationUsername,
                UnprotectedPassword = AuthorizationPassword
            };
        }
    }
}
