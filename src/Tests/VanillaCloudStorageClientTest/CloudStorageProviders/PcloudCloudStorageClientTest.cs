using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VanillaCloudStorageClient;
using VanillaCloudStorageClient.CloudStorageProviders;

namespace VanillaCloudStorageClientTest.CloudStorageProviders
{
    [TestClass]
    public class PcloudCloudStorageClientTest
    {
        /// <summary>
        /// Setting <see cref="DoRealWebRequests"/> to true, causes the unit tests to act as
        /// integration tests, doing real web requests to the API. The constants
        /// • <see cref="ClientId"/>
        /// • <see cref="RedirectUrl"/>
        /// • <see cref="PcloudAccessToken"/>
        /// must then be set to valid credentials.
        /// </summary>
        private const bool DoRealWebRequests = false;
        private const string ClientId = "GetIdFromPCloudPortal";
        private const string RedirectUrl = "com.example.myapp://oauth2redirect/";
        private const string PcloudAccessToken = "GetItWith:ReallyDoFetchToken";
        private const string ClientSecret = "GetSecretFromPCloudPortal";

        private const string State = "7ysv8L9s4LB9CZpA";
        private const string CodeVerifier = null;
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
        [Ignore("Opens the authorization page in the system browse, to get a real authorization-code")]
        public void ReallyDoOpenAuthorizationPageInBrowser()
        {
            IOAuth2CloudStorageClient client = CreatePCloudStorageClient() as IOAuth2CloudStorageClient;
            string requestUrl = client.BuildAuthorizationRequestUrl(State, CodeVerifier);

            Process browserProcess = new Process();
            browserProcess.StartInfo.UseShellExecute = true;
            browserProcess.StartInfo.FileName = requestUrl;
            browserProcess.Start();
        }

        [TestMethod]
        [Ignore("Gets a real access-token")]
        public async Task ReallyDoFetchToken()
        {
            // Replace this example url with the redirected url from the browser.
            const string PCloudRedirectedUrl = "com.example.appname://oauth2redirect/?code=AAAAZmfGBXkZxjiiKbvywzFFFFQqjMMKTzFj6yyy&locationid=2&hostname=eapi.pcloud.com&state=8ysv8L9s4LB9CZp3";
            if (!DoRealWebRequests)
                return;

            // Fetch token
            var client = CreatePCloudStorageClient() as IOAuth2CloudStorageClient;
            CloudStorageToken token = await client.FetchTokenAsync(PCloudRedirectedUrl, State, CodeVerifier);

            Assert.IsNotNull(token.AccessToken);
        }

        [TestMethod]
        public async Task FileLifecycleWorks()
        {
            string fileName = "unittest.dat";
            byte[] fileContent = new byte[16];
            new Random().NextBytes(fileContent);

            // 1) Test upload
            if (!DoRealWebRequests)
            {
                _httpTest.RespondWith(string.Empty);
            }
            await UploadFileWorksAsync(fileName, fileContent);

            // 2) Test listing
            if (!DoRealWebRequests)
            {
                _httpTest.RespondWith(GetPCloudFileListResponse());
            }
            List<string> res = await ListFileNamesWorksAsync();
            Assert.IsTrue(res.Count >= 1);
            Assert.IsTrue(res.Contains("unittest.dat"));

            // 3) Test exists
            if (!DoRealWebRequests)
            {
                _httpTest.RespondWith(GetPCloudFileListResponse());
            }
            bool exists = await FileExistsWorksAsync(fileName);
            Assert.IsTrue(exists);

            // 4) Test download
            if (!DoRealWebRequests)
            {
                _httpTest
                    .RespondWith(GetPCloudFileListResponse())
                    .RespondWith(GetPCloudFileLinkResponse())
                    .RespondWith(() => new ByteArrayContent(fileContent));
            }
            Byte[] downloadedContent = await DownloadFileWorksAsync(fileName);
            CollectionAssert.AreEqual(fileContent, downloadedContent);

            // 5) Test delete
            if (!DoRealWebRequests)
            {
                _httpTest
                    .RespondWith(GetPCloudFileListResponse())
                    .RespondWith(string.Empty);
            }
            await DeleteFileWorksAsync(fileName);

            // 6) Was really deleted?
            if (!DoRealWebRequests)
            {
                _httpTest.RespondWith(GetPCloudFileListEmptyResponse());
            }
            exists = await FileExistsWorksAsync(fileName);
            Assert.IsFalse(exists);
        }

        private async Task UploadFileWorksAsync(string fileName, byte[] fileContent)
        {
            ICloudStorageClient client = CreatePCloudStorageClient();
            var credentials = new CloudStorageCredentials { Token = new CloudStorageToken { AccessToken = PcloudAccessToken } };
            await client.UploadFileAsync(fileName, fileContent, credentials);
        }

        private async Task<byte[]> DownloadFileWorksAsync(string fileName)
        {
            ICloudStorageClient client = CreatePCloudStorageClient();
            var credentials = new CloudStorageCredentials { Token = new CloudStorageToken { AccessToken = PcloudAccessToken } };
            return await client.DownloadFileAsync(fileName, credentials);
        }

        private async Task DeleteFileWorksAsync(string fileName)
        {
            ICloudStorageClient client = CreatePCloudStorageClient();
            var credentials = new CloudStorageCredentials { Token = new CloudStorageToken { AccessToken = PcloudAccessToken } };
            await client.DeleteFileAsync(fileName, credentials);
        }

        private async Task<List<string>> ListFileNamesWorksAsync()
        {
            ICloudStorageClient client = CreatePCloudStorageClient();
            var credentials = new CloudStorageCredentials { Token = new CloudStorageToken { AccessToken = PcloudAccessToken } };
            List<string> result = await client.ListFileNamesAsync(credentials);
            return result;
        }

        private async Task<bool> FileExistsWorksAsync(string filename)
        {
            ICloudStorageClient client = CreatePCloudStorageClient();
            var credentials = new CloudStorageCredentials { Token = new CloudStorageToken { AccessToken = PcloudAccessToken } };
            return await client.ExistsFileAsync(filename, credentials);
        }

        private ICloudStorageClient CreatePCloudStorageClient()
        {
            return new PcloudCloudStorageClient(ClientId, RedirectUrl, PcloudCloudStorageClient.DataCenter.Europe, ClientSecret);
        }

        private string GetPCloudFileListResponse()
        {
            return
@"{
	""result"": 0,
	""metadata"": {
		""name"": """",
		""isfolder"": true,
		""contents"": [
			{
				""isfolder"": false,
				""fileid"": 58130557888,
				""name"": ""unittest.dat""
			}
		]
	}
}";
        }

        private string GetPCloudFileListEmptyResponse()
        {
            return
@"{
	""result"": 0,
	""metadata"": {
		""name"": """",
		""isfolder"": true,
		""contents"": [
		]
	}
}";
        }

        private string GetPCloudFileLinkResponse()
        {
            return
@"{
	""result"": 0,
	""dwltag"": ""uuaSz6l3Qv5aSYbPPO2222"",
	""hash"": 12980384994410688888,
	""size"": 16,
	""expires"": ""Sun, 12 Jan 2025 21:44:26 +0000"",
	""path"": ""\/DLZCCCCICZrSzBIJ7Z0CCCZZ8Yu2XkZ3kZZCCCZZQZLHZ4RZ8VZYXhbrap1TbSiiPdrlVNXiVqM0wpk\/unittest.dat"",
	""hosts"": [
		""edede.pcloud.com"",
		""evcev6.pcloud.com""
	]
}";
        }
    }
}
