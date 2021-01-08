using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http.Testing;
using NUnit.Framework;
using VanillaCloudStorageClient;
using VanillaCloudStorageClient.CloudStorageProviders;

namespace VanillaCloudStorageClientTest.CloudStorageProviders
{
    [TestFixture]
    public class DropboxCloudStorageClientTest
    {
        /// <summary>
        /// Setting <see cref="DoRealWebRequests"/> to true, causes the unit tests to act as
        /// integration tests, doing real web requests to the API. The constants
        /// • <see cref="ClientId"/>
        /// • <see cref="RedirectUrl"/>
        /// • <see cref="DropboxAccessToken"/>
        /// must then be set to a valid Dropbox bearer token.
        /// </summary>
        private const bool DoRealWebRequests = false;
        private const string ClientId = "cid";
        private const string RedirectUrl = "com.example.myapp://oauth2redirect/";

        private const string DropboxRedirectedUrl = "GetItWith:ReallyDoOpenAuthorizationPageInBrowser";
        private const string DropboxAccessToken = "GetItWith:ReallyDoFetchToken_or_ReallyDoRefreshToken";
        private const string DropboxRefreshToken = "GetItWith:ReallyDoFetchToken";

        private const string State = "7ysv8L9s4LB9CZpA";
        private const string CodeVerifier = "abcdefghijklmnopqrstuvwxyabcdefghijklmnopqrstuvwxy";
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
        [Ignore("Opens the authorization page in the system browse, to get a real access-token")]
        public void ReallyDoOpenAuthorizationPageInBrowser()
        {
            IOAuth2CloudStorageClient client = new DropboxCloudStorageClient(ClientId, RedirectUrl);
            string requestUrl = client.BuildAuthorizationRequestUrl(State, CodeVerifier);

            Process browserProcess = new Process();
            browserProcess.StartInfo.UseShellExecute = true;
            browserProcess.StartInfo.FileName = requestUrl;
            browserProcess.Start();
        }

        [Test]
        [Ignore("Gets a real access-token")]
        public async Task ReallyDoFetchToken()
        {
            if (!DoRealWebRequests)
                return;

            // Fetch token
            IOAuth2CloudStorageClient client = new DropboxCloudStorageClient(ClientId, RedirectUrl);
            CloudStorageToken token = await client.FetchTokenAsync(DropboxRedirectedUrl, State, CodeVerifier);

            Assert.IsNotNull(token.AccessToken);
            Assert.IsNotNull(token.RefreshToken);
        }

        [Test]
        [Ignore("Refreshes a real token")]
        public async Task ReallyDoRefreshToken()
        {
            if (!DoRealWebRequests)
                return;

            CloudStorageToken oldToken = new CloudStorageToken
            {
                RefreshToken = DropboxRefreshToken,
            };

            // Refresh token
            IOAuth2CloudStorageClient client = new DropboxCloudStorageClient(ClientId, RedirectUrl);
            CloudStorageToken newToken = await client.RefreshTokenAsync(oldToken);

            Assert.IsNotNull(newToken.AccessToken);
            Assert.AreNotEqual(oldToken.AccessToken, newToken.AccessToken);
            Assert.AreEqual(oldToken.RefreshToken, newToken.RefreshToken);
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
                _httpTest.RespondWith(GetDropboxFileListResponse());
            }
            List<string> res = Task.Run(async () => await ListFileNamesWorksAsync()).Result;
            Assert.IsTrue(res.Count >= 1);
            Assert.IsTrue(res.Contains("unittest.dat"));

            // 3) Test exists
            if (!DoRealWebRequests)
            {
                _httpTest.RespondWith(GetDropboxFileListResponse());
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
                _httpTest.RespondWith(GetDropboxEmptyFileListResponse());
            }
            exists = Task.Run(async () => await FileExistsWorksAsync(fileName)).Result;
            Assert.IsFalse(exists);
        }

        private async Task UploadFileWorksAsync(string fileName, byte[] fileContent)
        {
            ICloudStorageClient client = new DropboxCloudStorageClient(ClientId, RedirectUrl);
            var credentials = new CloudStorageCredentials { Token = new CloudStorageToken { AccessToken = DropboxAccessToken } };
            await client.UploadFileAsync(fileName, fileContent, credentials);
        }

        private async Task<byte[]> DownloadFileWorksAsync(string fileName, string accessToken = null)
        {
            ICloudStorageClient client = new DropboxCloudStorageClient(ClientId, RedirectUrl);
            var credentials = new CloudStorageCredentials { Token = new CloudStorageToken { AccessToken = DropboxAccessToken } };
            if (!string.IsNullOrEmpty(accessToken))
                credentials.Token.AccessToken = accessToken;
            return await client.DownloadFileAsync(fileName, credentials);
        }

        private async Task DeleteFileWorksAsync(string fileName)
        {
            ICloudStorageClient client = new DropboxCloudStorageClient(ClientId, RedirectUrl);
            var credentials = new CloudStorageCredentials { Token = new CloudStorageToken { AccessToken = DropboxAccessToken } };
            await client.DeleteFileAsync(fileName, credentials);
        }

        private async Task<List<string>> ListFileNamesWorksAsync()
        {
            ICloudStorageClient client = new DropboxCloudStorageClient(ClientId, string.Empty);
            var credentials = new CloudStorageCredentials { Token = new CloudStorageToken { AccessToken = DropboxAccessToken } };
            List<string> result = await client.ListFileNamesAsync(credentials);
            return result;
        }

        private async Task<bool> FileExistsWorksAsync(string filename)
        {
            ICloudStorageClient client = new DropboxCloudStorageClient(ClientId, string.Empty);
            var credentials = new CloudStorageCredentials { Token = new CloudStorageToken { AccessToken = DropboxAccessToken } };
            return await client.ExistsFileAsync(filename, credentials);
        }

        [Test]
        public void ThrowsAccessDeniedExceptionWithInvalidToken()
        {
            if (!DoRealWebRequests)
                _httpTest.RespondWith("Call failed with status code 401 (invalid_access_token/...): POST https://content.dropboxapi.com/2/files/download", (int)HttpStatusCode.Unauthorized);

            // Make token invalid by changing the last character (to keep the structure of the token)
            string invalidAccessToken = IncreaseLastChar(DropboxAccessToken);
            Assert.ThrowsAsync<AccessDeniedException>(() => DownloadFileWorksAsync("a.txt", invalidAccessToken));
        }

        private string GetDropboxFileListResponse()
        {
            return "{\"entries\": [{\".tag\": \"file\", \"name\": \"unittest.dat\", \"path_lower\": \"/unittest.dat\", \"path_display\": \"/unittest.dat\", \"id\": \"id:pUjQZXcYnZAAAAAAAAAAFg\", \"client_modified\": \"2018-12-29T20:40:25Z\", \"server_modified\": \"2018-12-29T20:40:25Z\", \"rev\": \"af96956d0\", \"size\": 26778, \"is_downloadable\": true, \"content_hash\": \"FAKEID2\"}, {\".tag\": \"file\", \"name\": \"silentnotes_repository_demo.silentnotes\", \"path_lower\": \"/silentnotes_repository_demo.silentnotes\", \"path_display\": \"/silentnotes_repository_demo.silentnotes\", \"id\": \"id:FAKEID3\", \"client_modified\": \"2018-12-29T21:10:55Z\", \"server_modified\": \"2018-12-29T21:10:55Z\", \"rev\": \"df96956d0\", \"size\": 4433, \"is_downloadable\": true, \"content_hash\": \"2904c9ee982f803211c462509eaa1e093260a88c1949e761aafe79103ab58e68\"}, {\".tag\": \"file\", \"name\": \"a.txt\", \"path_lower\": \"/a.txt\", \"path_display\": \"/a.txt\", \"id\": \"id:FAKEID4\", \"client_modified\": \"2019-06-03T09:38:02Z\", \"server_modified\": \"2019-06-03T09:38:03Z\", \"rev\": \"28f96956d0\", \"size\": 1, \"is_downloadable\": true, \"content_hash\": \"bf5d3affb73efd2ec6c36ad3112dd933efed63c4e1cbffcfa88e2759c144f2d8\"}, {\".tag\": \"file\", \"name\": \"b.txt\", \"path_lower\": \"/b.txt\", \"path_display\": \"/b.txt\", \"id\": \"id:pUjQZXcYnZAAAAAAAAAAIg\", \"client_modified\": \"2019-06-03T09:38:03Z\", \"server_modified\": \"2019-06-03T09:38:03Z\", \"rev\": \"29f96956d0\", \"size\": 1, \"is_downloadable\": true, \"content_hash\": \"39361160903c6695c6804b7157c7bd10013e9ba89b1f954243bc8e3990b08db9\"}, {\".tag\": \"file\", \"name\": \"c.txt\", \"path_lower\": \"/c.txt\", \"path_display\": \"/c.txt\", \"id\": \"id:pUjQZXcYnZAAAAAAAAAAIw\", \"client_modified\": \"2019-06-03T09:38:04Z\", \"server_modified\": \"2019-06-03T09:38:04Z\", \"rev\": \"2af96956d0\", \"size\": 1, \"is_downloadable\": true, \"content_hash\": \"xxxxxxxxxxxxxxxx\"}], \"cursor\": \"XXXXXXXX\", \"has_more\": false}";
        }

        private string GetDropboxEmptyFileListResponse()
        {
            return "{\"entries\": [], \"cursor\": \"XXXXXXXX\", \"has_more\": false}";
        }

        private static string IncreaseLastChar(string text)
        {
            StringBuilder sb = new StringBuilder(text);
            int last = sb.Length - 1;
            char lastChar = sb[last];
            lastChar++;
            sb[last] = lastChar;
            return sb.ToString();
        }
    }
}
