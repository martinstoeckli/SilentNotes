using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Flurl.Http.Testing;
using NUnit.Framework;
using VanillaCloudStorageClient;
using VanillaCloudStorageClient.CloudStorageProviders;

namespace VanillaCloudStorageClientTest.CloudStorageProviders
{
    [TestFixture]
    public class WebdavCloudStorageClientTest
    {
        /// <summary>
        /// Setting <see cref="DoRealWebRequests"/> to true, causes the unit tests to act as
        /// integration tests, doing real web requests to the API. The constants
        /// • <see cref="AuthorizationUrl"/>,
        /// • <see cref="AuthorizationUsername"/>
        /// • <see cref="AuthorizationPassword"/>
        /// must then be set to valid credentials.
        /// Possible test server for anonymous requests: http://webdavserver.com
        /// </summary>
        private const bool DoRealWebRequests = false;
        private const string AuthorizationUrl = "https://webdav.example.com/unittest/";
        private const string AuthorizationUsername = "userX";
        private const string AuthorizationPassword = "passwordX";
        private HttpTest _httpTest;

        [SetUp]
        public void CreateHttpTest()
        {
            if (!DoRealWebRequests)
                _httpTest = new HttpTest(); // Put flurl into test mode
        }

        [TearDown]
        public void DisposeHttpTest()
        {
            if (!DoRealWebRequests)
                _httpTest.Dispose();
        }

        [Test]
        public void FileLifecycleWorks()
        {
            string fileName = "unittest.dat";
            byte[] fileContent = new byte[16];
            new Random().NextBytes(fileContent);

            // 1) Test upload
            Assert.DoesNotThrowAsync(() => UploadFileWorksAsync(fileName, fileContent));

            // 2) Test listing
            if (!DoRealWebRequests)
            {
                _httpTest.RespondWith(GetWebdavFileListResponse());
            }
            List<string> res = Task.Run(async () => await ListFileNamesWorksAsync()).Result;
            Assert.IsTrue(res.Count >= 1);
            Assert.IsTrue(res.Contains("unittest.dat"));

            // 3) Test exists
            if (!DoRealWebRequests)
            {
                _httpTest.RespondWith(GetWebdavFileListResponse());
            }
            bool exists = Task.Run(async () => await FileExistsWorksAsync(fileName)).Result;
            Assert.IsTrue(exists);

            // 4) Test download
            if (!DoRealWebRequests)
            {
                HttpContent httpContent = new ByteArrayContent(fileContent);
                _httpTest.RespondWith(() => httpContent);
            }
            Byte[] downloadedContent = Task.Run(async () => await DownloadFileWorksAsync(fileName)).Result;
            Assert.AreEqual(fileContent, downloadedContent);

            // 5) Test delete
            if (!DoRealWebRequests)
            {
                _httpTest.RespondWith(() => new ByteArrayContent(new byte[0]));
            }
            Assert.DoesNotThrowAsync(() => DeleteFileWorksAsync(fileName));

            // 6) Was really deleted?
            if (!DoRealWebRequests)
            {
                _httpTest.RespondWith(GetWebdavEmptyFileListResponse());
            }
            exists = Task.Run(async () => await FileExistsWorksAsync(fileName)).Result;
            Assert.IsFalse(exists);
        }

        private async Task UploadFileWorksAsync(string fileName, byte[] fileContent)
        {
            ICloudStorageClient client = new WebdavCloudStorageClient();
            await client.UploadFileAsync(fileName, fileContent, GetCredentials());
        }

        private async Task<byte[]> DownloadFileWorksAsync(string fileName, CloudStorageCredentials credentials = null)
        {
            ICloudStorageClient client = new WebdavCloudStorageClient();
            if (credentials == null)
                credentials = GetCredentials();
            return await client.DownloadFileAsync(fileName, credentials);
        }

        private async Task DeleteFileWorksAsync(string fileName)
        {
            ICloudStorageClient client = new WebdavCloudStorageClient();
            await client.DeleteFileAsync(fileName, GetCredentials());
        }

        private async Task<List<string>> ListFileNamesWorksAsync()
        {
            ICloudStorageClient client = new WebdavCloudStorageClient();
            List<string> result = await client.ListFileNamesAsync(GetCredentials());
            return result;
        }

        private async Task<bool> FileExistsWorksAsync(string filename)
        {
            ICloudStorageClient client = new WebdavCloudStorageClient();
            return await client.ExistsFileAsync(filename, GetCredentials());
        }

        [Test]
        public void ThrowsWithInvalidUsername()
        {
            if (!DoRealWebRequests)
                _httpTest.RespondWith("Call failed with status code 403 (Forbidden): GET ...", (int)HttpStatusCode.Forbidden);

            CloudStorageCredentials credentials = GetCredentials();
            credentials.Username = "youdontknowme";
            Assert.ThrowsAsync<AccessDeniedException>(() => DownloadFileWorksAsync("a.txt", credentials));
        }

        [Test]
        public void ThrowsWithInvalidPath()
        {
            if (!DoRealWebRequests)
                _httpTest.RespondWith("Call failed with status code 404 (Not Found): GET ...", (int)HttpStatusCode.NotFound);

            CloudStorageCredentials credentials = GetCredentials();
            credentials.Url = "https://www.example.com/youdontknowme";
            Assert.ThrowsAsync<ConnectionFailedException>(() => DownloadFileWorksAsync("a.txt", credentials));
        }

        [Test]
        public void ParseGmxWebdavResponseCorrectly()
        {
            List<string> fileNames = WebdavCloudStorageClient.ParseWebdavResponseForFileNames(GetGxmResponse());
            Assert.AreEqual(7, fileNames.Count);
            Assert.AreEqual("silentnotes_repository_demo.silentnotes", fileNames[0]);
            Assert.AreEqual("silentnotes_repository_dev.silentnotes", fileNames[1]);
            Assert.AreEqual("silentnotes_repository_unittest.silentnotes", fileNames[2]);
            Assert.AreEqual("silentnotes_repository.silentnotes", fileNames[3]);
            Assert.AreEqual("tinu-2017-09-19.data", fileNames[4]);
            Assert.AreEqual("tinu-2017-10-19.data", fileNames[5]);
            Assert.AreEqual("tinu space.data", fileNames[6]);
        }

        [Test]
        public void ParseStratoWebdavResponseCorrectly()
        {
            List<string> fileNames = WebdavCloudStorageClient.ParseWebdavResponseForFileNames(GetStratoResponse());
            Assert.AreEqual(3, fileNames.Count);
            Assert.AreEqual("Bearbeiten1.txt", fileNames[0]);
            Assert.AreEqual("test_a.txt", fileNames[1]);
            Assert.AreEqual("Bearbeiten2.txt", fileNames[2]);
        }

        [Test]
        public void ParseMailboxOrgWebdavResponseCorrectly()
        {
            List<string> fileNames = WebdavCloudStorageClient.ParseWebdavResponseForFileNames(GetMailboxOrgResponse());
            Assert.AreEqual(1, fileNames.Count);
            Assert.AreEqual("unittest.dat", fileNames[0]);
        }


        private string GetWebdavFileListResponse()
        {
            return
@"<D:multistatus xmlns:D='DAV:'>
    <D:response>
    <D:href>unittest.dat</D:href>
    <D:propstat>
        <D:prop>
        <D:displayname>unittest.dat</D:displayname>
        <D:resourcetype />
        </D:prop>
        <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
    </D:response>
</D:multistatus>";
        }

        private string GetWebdavEmptyFileListResponse()
        {
            return
@"<D:multistatus xmlns:D='DAV:'>
</D:multistatus>";
        }

        private XDocument GetGxmResponse()
        {
            string response = @"
        <D:multistatus xmlns:D='DAV:'>
          <D:response>
            <D:href>/</D:href>
            <D:propstat>
              <D:prop>
                <D:displayname>/</D:displayname>
                <D:resourcetype>
                  <D:collection />
                </D:resourcetype>
              </D:prop>
              <D:status>HTTP/1.1 200 OK</D:status>
            </D:propstat>
          </D:response>
          <D:response>
            <D:href>/Externe%20Ordner/</D:href>
            <D:propstat>
              <D:prop>
                <D:displayname>Externe Ordner</D:displayname>
                <D:resourcetype>
                  <D:collection />
                </D:resourcetype>
              </D:prop>
              <D:status>HTTP/1.1 200 OK</D:status>
            </D:propstat>
          </D:response>
          <D:response>
            <D:href>/Gel%c3%b6schte%20Dateien/</D:href>
            <D:propstat>
              <D:prop>
                <D:displayname>Gelöschte Dateien</D:displayname>
                <D:resourcetype>
                  <D:collection />
                </D:resourcetype>
              </D:prop>
              <D:status>HTTP/1.1 200 OK</D:status>
            </D:propstat>
          </D:response>
          <D:response>
            <D:href>/Meine%20Bilder/</D:href>
            <D:propstat>
              <D:prop>
                <D:displayname>Meine Bilder</D:displayname>
                <D:resourcetype>
                  <D:collection />
                </D:resourcetype>
              </D:prop>
              <D:status>HTTP/1.1 200 OK</D:status>
            </D:propstat>
          </D:response>
          <D:response>
            <D:href>/Meine%20Dokumente/</D:href>
            <D:propstat>
              <D:prop>
                <D:displayname>Meine Dokumente</D:displayname>
                <D:resourcetype>
                  <D:collection />
                </D:resourcetype>
              </D:prop>
              <D:status>HTTP/1.1 200 OK</D:status>
            </D:propstat>
          </D:response>
          <D:response>
            <D:href>/Meine%20Musikdateien/</D:href>
            <D:propstat>
              <D:prop>
                <D:displayname>Meine Musikdateien</D:displayname>
                <D:resourcetype>
                  <D:collection />
                </D:resourcetype>
              </D:prop>
              <D:status>HTTP/1.1 200 OK</D:status>
            </D:propstat>
          </D:response>
          <D:response>
            <D:href>/Neue%20Dateianlagen/</D:href>
            <D:propstat>
              <D:prop>
                <D:displayname>Neue Dateianlagen</D:displayname>
                <D:resourcetype>
                  <D:collection />
                </D:resourcetype>
              </D:prop>
              <D:status>HTTP/1.1 200 OK</D:status>
            </D:propstat>
          </D:response>
          <D:response>
            <D:href>/silentnotes_repository_demo.silentnotes</D:href>
            <D:propstat>
              <D:prop>
                <D:displayname>silentnotes_repository_demo.silentnotes</D:displayname>
                <D:resourcetype />
              </D:prop>
              <D:status>HTTP/1.1 200 OK</D:status>
            </D:propstat>
          </D:response>
          <D:response>
            <D:href>/silentnotes_repository_dev.silentnotes</D:href>
            <D:propstat>
              <D:prop>
                <D:displayname>silentnotes_repository_dev.silentnotes</D:displayname>
                <D:resourcetype />
              </D:prop>
              <D:status>HTTP/1.1 200 OK</D:status>
            </D:propstat>
          </D:response>
          <D:response>
            <D:href>/silentnotes_repository_unittest.silentnotes</D:href>
            <D:propstat>
              <D:prop>
                <D:displayname>silentnotes_repository_unittest.silentnotes</D:displayname>
                <D:resourcetype />
              </D:prop>
              <D:status>HTTP/1.1 200 OK</D:status>
            </D:propstat>
          </D:response>
          <D:response>
            <D:href>/silentnotes_repository.silentnotes</D:href>
            <D:propstat>
              <D:prop>
                <D:displayname>silentnotes_repository.silentnotes</D:displayname>
                <D:resourcetype />
              </D:prop>
              <D:status>HTTP/1.1 200 OK</D:status>
            </D:propstat>
          </D:response>
          <D:response>
            <D:href>/tinu-2017-09-19.data</D:href>
            <D:propstat>
              <D:prop>
                <D:displayname>tinu-2017-09-19.data</D:displayname>
                <D:resourcetype />
              </D:prop>
              <D:status>HTTP/1.1 200 OK</D:status>
            </D:propstat>
          </D:response>
          <D:response>
            <D:href>/tinu-2017-10-19.data</D:href>
            <D:propstat>
              <D:prop>
                <D:displayname>tinu-2017-10-19.data</D:displayname>
                <D:resourcetype />
              </D:prop>
              <D:status>HTTP/1.1 200 OK</D:status>
            </D:propstat>
          </D:response>
          <D:response>
            <D:href>/tinu%20space.data</D:href>
            <D:propstat>
              <D:prop>
                <D:displayname>tinu.data</D:displayname>
                <D:resourcetype />
              </D:prop>
              <D:status>HTTP/1.1 200 OK</D:status>
            </D:propstat>
          </D:response>
        </D:multistatus>
        ";
            using (TextReader rextReader = new StringReader(response))
            {
                return XDocument.Load(rextReader);
            }
        }

        private XDocument GetStratoResponse()
        {
            string response = @"
        <ns0:multistatus xmlns:D='DAV:' xmlns:ns0='DAV:'>
          <g0:response xmlns:lp1='DAV:' xmlns:g0='DAV:'>
            <g0:href>/users/martinstoeckli/</g0:href>
            <g0:propstat>
              <g0:prop>
                <g0:resourcetype>
                  <g0:collection />
                </g0:resourcetype>
              </g0:prop>
              <g0:status>HTTP/1.1 200 OK</g0:status>
            </g0:propstat>
            <g0:propstat>
              <g0:prop>
                <g0:displayname />
              </g0:prop>
              <g0:status>HTTP/1.1 404 Not Found</g0:status>
            </g0:propstat>
          </g0:response>
          <g0:response xmlns:lp1='DAV:' xmlns:g0='DAV:'>
            <g0:href>/users/martinstoeckli/Bearbeiten1.txt</g0:href>
            <g0:propstat>
              <g0:prop>
                <g0:resourcetype />
              </g0:prop>
              <g0:status>HTTP/1.1 200 OK</g0:status>
            </g0:propstat>
            <g0:propstat>
              <g0:prop>
                <g0:displayname />
              </g0:prop>
              <g0:status>HTTP/1.1 404 Not Found</g0:status>
            </g0:propstat>
          </g0:response>
          <g0:response xmlns:lp1='DAV:' xmlns:g0='DAV:'>
            <g0:href>/users/martinstoeckli/.hidrive/</g0:href>
            <g0:propstat>
              <g0:prop>
                <g0:resourcetype>
                  <g0:collection />
                </g0:resourcetype>
              </g0:prop>
              <g0:status>HTTP/1.1 200 OK</g0:status>
            </g0:propstat>
            <g0:propstat>
              <g0:prop>
                <g0:displayname />
              </g0:prop>
              <g0:status>HTTP/1.1 404 Not Found</g0:status>
            </g0:propstat>
          </g0:response>
          <g0:response xmlns:lp1='DAV:' xmlns:g0='DAV:'>
            <g0:href>/users/martinstoeckli/test_a.txt</g0:href>
            <g0:propstat>
              <g0:prop>
                <g0:resourcetype />
              </g0:prop>
              <g0:status>HTTP/1.1 200 OK</g0:status>
            </g0:propstat>
            <g0:propstat>
              <g0:prop>
                <g0:displayname />
              </g0:prop>
              <g0:status>HTTP/1.1 404 Not Found</g0:status>
            </g0:propstat>
          </g0:response>
          <g0:response xmlns:lp1='DAV:' xmlns:g0='DAV:'>
            <g0:href>/users/martinstoeckli/subfolder1/</g0:href>
            <g0:propstat>
              <g0:prop>
                <g0:resourcetype>
                  <g0:collection />
                </g0:resourcetype>
              </g0:prop>
              <g0:status>HTTP/1.1 200 OK</g0:status>
            </g0:propstat>
            <g0:propstat>
              <g0:prop>
                <g0:displayname />
              </g0:prop>
              <g0:status>HTTP/1.1 404 Not Found</g0:status>
            </g0:propstat>
          </g0:response>
          <g0:response xmlns:lp1='DAV:' xmlns:g0='DAV:'>
            <g0:href>/users/martinstoeckli/Bearbeiten2.txt</g0:href>
            <g0:propstat>
              <g0:prop>
                <g0:resourcetype />
              </g0:prop>
              <g0:status>HTTP/1.1 200 OK</g0:status>
            </g0:propstat>
            <g0:propstat>
              <g0:prop>
                <g0:displayname />
              </g0:prop>
              <g0:status>HTTP/1.1 404 Not Found</g0:status>
            </g0:propstat>
          </g0:response>
        </ns0:multistatus>";
            using (TextReader rextReader = new StringReader(response))
            {
                return XDocument.Load(rextReader);
            }
        }

        private XDocument GetMailboxOrgResponse()
        {
            string response = @"
<D:multistatus xmlns:D='DAV:'>
  <D:response>
    <D:href>/servlet/webdav.infostore/Userstore/Martin%20Stoeckli/</D:href>
    <D:propstat>
      <D:prop>
        <D:resourcetype>
          <D:collection />
        </D:resourcetype>
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
  <D:response>
    <D:href>/servlet/webdav.infostore/Userstore/Martin%20Stoeckli/Pictures/</D:href>
    <D:propstat>
      <D:prop>
        <D:resourcetype>
          <D:collection />
        </D:resourcetype>
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
  <D:response>
    <D:href>/servlet/webdav.infostore/Userstore/Martin%20Stoeckli/unittest.dat</D:href>
    <D:propstat>
      <D:prop>
        <D:resourcetype />
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
  <D:response>
    <D:href>/servlet/webdav.infostore/Userstore/Martin%20Stoeckli/Documents/</D:href>
    <D:propstat>
      <D:prop>
        <D:resourcetype>
          <D:collection />
        </D:resourcetype>
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
</D:multistatus>";
            using (TextReader rextReader = new StringReader(response))
            {
                return XDocument.Load(rextReader);
            }
        }

        private static CloudStorageCredentials GetCredentials()
        {
            return new CloudStorageCredentials
            {
                Url = AuthorizationUrl,
                Username = AuthorizationUsername,
                UnprotectedPassword = AuthorizationPassword
            };
        }
    }
}
