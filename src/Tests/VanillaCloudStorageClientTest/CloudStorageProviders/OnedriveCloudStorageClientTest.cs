﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http.Testing;
using NUnit.Framework;
using VanillaCloudStorageClient;
using VanillaCloudStorageClient.CloudStorageProviders;

#pragma warning disable CS0162 // Unreachable code detected
namespace VanillaCloudStorageClientTest.CloudStorageProviders
{
    [TestFixture]
    public class OnedriveCloudStorageClientTest
    {
        /// <summary>
        /// Setting <see cref="DoRealWebRequests"/> to true, causes the unit tests to act as
        /// integration tests, doing real web requests to the API. The constants
        /// • <see cref="ClientId"/>
        /// • <see cref="RedirectUrl"/>
        /// • <see cref="OnedriveAccessToken"/>
        /// must then be set to valid credentials.
        /// </summary>
        private const bool DoRealWebRequests = false;
        private const string ClientId = "cid";
        private const string RedirectUrl = "com.example.myapp://oauth2redirect/";

        private const string OnedriveRedirectedUrl = "ch.martinstoeckli.silentnotes://oauth2redirect/?code=M.R3_BAY.6d2c377a-4ea9-dece-9c0b-82397f777c9c&state=7ysv8L9s4LB9CZpA";
        private const string OnedriveAccessToken = "GetItWith:ReallyDoFetchToken_or_ReallyDoRefreshToken";
        private const string OnedriveRefreshToken = "GetItWith:ReallyDoFetchToken";

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
        [Ignore("Opens the authorization page in the system browse, to get a real authorization-code")]
        public void ReallyDoOpenAuthorizationPageInBrowser()
        {
            IOAuth2CloudStorageClient client = new OnedriveCloudStorageClient(ClientId, RedirectUrl);
            string requestUrl = client.BuildAuthorizationRequestUrl(State, null);

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
            IOAuth2CloudStorageClient client = new OnedriveCloudStorageClient(ClientId, RedirectUrl);
            CloudStorageToken token = await client.FetchTokenAsync(OnedriveRedirectedUrl, State, CodeVerifier);

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
                RefreshToken = OnedriveRefreshToken,
            };

            // Refresh token
            IOAuth2CloudStorageClient client = new OnedriveCloudStorageClient(ClientId, RedirectUrl);
            CloudStorageToken newToken = await client.RefreshTokenAsync(oldToken);

            Assert.IsNotNull(newToken.AccessToken);
            Assert.AreNotEqual(oldToken.AccessToken, newToken.AccessToken);
            Assert.AreEqual(oldToken.RefreshToken, newToken.RefreshToken);
        }

        [Test]
        public async Task FileLifecycleWorks()
        {
            string fileName = "unittest.dat";
            byte[] fileContent = new byte[16];
            new Random().NextBytes(fileContent);

            // 1) Test upload
            if (!DoRealWebRequests)
            {
                _httpTest.RespondWith(@"{ ""id"": ""fakeid"" }");
                _httpTest.RespondWith(GetOneDriveUploadSession());
            }
            Assert.DoesNotThrowAsync(() => UploadFileWorksAsync(fileName, fileContent));

            // 2) Test listing
            if (!DoRealWebRequests)
            {
                _httpTest.RespondWith(GetOnedriveFileListResponse());
            }
            List<string> res = await ListFileNamesWorksAsync();
            Assert.IsTrue(res.Count >= 1);
            Assert.IsTrue(res.Contains("unittest.dat"));

            // 3) Test exists
            bool exists = await FileExistsWorksAsync(fileName);
            Assert.IsTrue(exists);

            // 4) Test download
            if (!DoRealWebRequests)
            {
                HttpContent httpContent = new ByteArrayContent(fileContent);
                _httpTest.RespondWith(() => httpContent);
            }
            Byte[] downloadedContent = await DownloadFileWorksAsync(fileName);
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
                HttpContent content = null;
                _httpTest.RespondWith(() => content, 404);
            }
            exists = await FileExistsWorksAsync(fileName);
            Assert.IsFalse(exists);
        }

        private async Task UploadFileWorksAsync(string fileName, byte[] fileContent)
        {
            ICloudStorageClient client = new OnedriveCloudStorageClient(ClientId, RedirectUrl);
            var credentials = new CloudStorageCredentials { Token = new CloudStorageToken { AccessToken = OnedriveAccessToken } };
            await client.UploadFileAsync(fileName, fileContent, credentials);
        }

        private async Task<byte[]> DownloadFileWorksAsync(string fileName)
        {
            ICloudStorageClient client = new OnedriveCloudStorageClient(ClientId, RedirectUrl);
            var credentials = new CloudStorageCredentials { Token = new CloudStorageToken { AccessToken = OnedriveAccessToken } };
            return await client.DownloadFileAsync(fileName, credentials);
        }

        private async Task DeleteFileWorksAsync(string fileName)
        {
            ICloudStorageClient client = new OnedriveCloudStorageClient(ClientId, RedirectUrl);
            var credentials = new CloudStorageCredentials { Token = new CloudStorageToken { AccessToken = OnedriveAccessToken } };
            await client.DeleteFileAsync(fileName, credentials);
        }

        private async Task<List<string>> ListFileNamesWorksAsync()
        {
            ICloudStorageClient client = new OnedriveCloudStorageClient(ClientId, string.Empty);
            var credentials = new CloudStorageCredentials { Token = new CloudStorageToken { AccessToken = OnedriveAccessToken } };
            List<string> result = await client.ListFileNamesAsync(credentials);
            return result;
        }

        private async Task<bool> FileExistsWorksAsync(string filename)
        {
            ICloudStorageClient client = new OnedriveCloudStorageClient(ClientId, string.Empty);
            var credentials = new CloudStorageCredentials { Token = new CloudStorageToken { AccessToken = OnedriveAccessToken } };
            return await client.ExistsFileAsync(filename, credentials);
        }

        private string GetOneDriveUploadSession()
        {
            return @"{""@odata.context"":""https://graph.microsoft.com/v1.0/$metadata#microsoft.graph.uploadSession"",""uploadUrl"":""https://api.onedrive.com/rup/FAKEID"",""expirationDateTime"":""2019-07-28T18:15:14.475Z"",""nextExpectedRanges"":[""0-""]}";
        }

        private string GetOnedriveFileListResponse()
        {
            return
@"{
""@odata.context"": ""https://graph.microsoft.com/v1.0/$metadata#users('FAKEUSER')/drive/special('approot')/children(name,file)"",
""@odata.count"": 3,
""value"": [{
        ""name"": ""subdir""
    }, {
        ""name"": ""a.txt"",
        ""file"": {
            ""mimeType"": ""text/plain"",
            ""hashes"": {
                ""quickXorHash"": ""FAKEHASH"",
                ""sha1Hash"": ""FAKEHASH""
            }
        }
    }, {
        ""name"": ""unittest.dat"",
        ""file"": {
            ""mimeType"": ""application/octet-stream"",
            ""hashes"": {
                ""quickXorHash"": ""/FAKEHASH="",
                ""sha1Hash"": ""FAKEHASH""
            }
        }
    }
]}";
        }
    }
}
