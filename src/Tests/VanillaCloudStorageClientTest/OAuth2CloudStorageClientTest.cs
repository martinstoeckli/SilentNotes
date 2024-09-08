﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Flurl.Http.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VanillaCloudStorageClient;
using VanillaCloudStorageClient.OAuth2;

namespace VanillaCloudStorageClientTest
{
    [TestClass]
    public class OAuth2CloudStorageClientTest
    {
        private const string State = "7ysv8L9s4LB9CZpA";
        private HttpTest _httpTest;

        [TestInitialize]
        public void CreateHttpTest()
        {
            _httpTest = new HttpTest(); // Put flurl into test mode
        }

        [TestCleanup]
        public void DisposeHttpTest()
        {
            _httpTest.Dispose();
        }

        [TestMethod]
        public void BuildOAuth2AuthorizationRequestUrlWorks()
        {
            IOAuth2CloudStorageClient client = new TestClient(GetDropboxConfig());
            string url = client.BuildAuthorizationRequestUrl(State, null);
            Assert.AreEqual("https://www.dropbox.com/oauth2/authorize?response_type=token&client_id=oaid&redirect_uri=com.example.myapp%3A%2F%2Foauth2redirect%2F&state=7ysv8L9s4LB9CZpA", url);
        }

        [TestMethod]
        public void FetchTokenCanInterpretGoogleResponse()
        {
            _httpTest.RespondWith(
@"{
  ""access_token"": ""aaaa.BBBBB-CDEF"",
  ""expires_in"": 3600,
  ""refresh_token"": ""8/A1AAbbZZ9"",
  ""scope"": ""https://www.googleapis.com/auth/drive.appdata"",
  ""token_type"": ""Bearer""
}");

            string redirectedUrl = "com.example.myapp://oauth2redirect/?state=7ysv8L9s4LB9CZpA&code=ABCDEF&scope=https://www.googleapis.com/auth/drive.appdata";

            // Fetch token
            IOAuth2CloudStorageClient client = new TestClient(GetGoogleConfig());
            CloudStorageToken token = Task.Run(async () => await FetchTokenAsync(client, redirectedUrl)).Result;

            Assert.AreEqual("aaaa.BBBBB-CDEF", token.AccessToken);
            Assert.AreEqual("8/A1AAbbZZ9", token.RefreshToken);
        }

        [TestMethod]
        public void FetchTokenThrowsWithWrongState()
        {
            string redirectedUrl = "com.example.myapp://oauth2redirect/?state=IsWrongA&code=ABCDEF&scope=https://www.googleapis.com/auth/drive.appdata";

            // Fetch token
            IOAuth2CloudStorageClient client = new TestClient(GetGoogleConfig());
            Assert.ThrowsExceptionAsync<CloudStorageException>(async () => await FetchTokenAsync(client, redirectedUrl));
        }

        [TestMethod]
        public void FetchTokenReturnsNullForDeniedAccess()
        {
            string redirectedUrl = "com.example.myapp://oauth2redirect/?error=access_denied";

            // Fetch token
            IOAuth2CloudStorageClient client = new TestClient(GetGoogleConfig());
            CloudStorageToken token = Task.Run(async () => await FetchTokenAsync(client, redirectedUrl)).Result;

            Assert.IsNull(token);
        }

        private async Task<CloudStorageToken> FetchTokenAsync(IOAuth2CloudStorageClient client, string redirectedUrl)
        {
            return await client.FetchTokenAsync(redirectedUrl, State, null);
        }

        [TestMethod]
        public void RefreshTokenCanInterpretGoogleResponse()
        {
            _httpTest.RespondWith(
@"{
  ""access_token"": ""aaaa.BBBBB-CDEF"",
  ""expires_in"": 3600,
  ""scope"": ""https://www.googleapis.com/auth/drive.appdata"",
  ""token_type"": ""Bearer""
}");

            CloudStorageToken oldToken = new CloudStorageToken
            {
                AccessToken = "dummy",
                RefreshToken = "8/A1AAbbZZ9",
            };

            IOAuth2CloudStorageClient client = new TestClient(GetGoogleConfig());
            CloudStorageToken newToken = Task.Run(async () => await RefreshTokenAsync(client, oldToken)).Result;

            Assert.AreEqual("aaaa.BBBBB-CDEF", newToken.AccessToken);
            Assert.AreEqual("8/A1AAbbZZ9", newToken.RefreshToken);
        }

        [TestMethod]
        public void RefreshToken_WhenRefreshTokenExpired_Throws()
        {
            _httpTest.RespondWith(
@"{
    ""error"": ""invalid_grant"",
    ""error_description"": ""AADSTS70000: The user could not be authenticated as the grant is expired. The user must sign in again.\r\nTrace ID: afe52903-9975-4945-a9bf-f07b390ff101\r\nCorrelation ID: a90a7d1c-25de-4bda-b739-dd5911524a69\r\nTimestamp: 2000-01-01 11:11:11Z"",
    ""error_codes"": [70000],
    ""timestamp"": ""2000-01-01 11:11:11Z"",
    ""trace_id"": ""afe52903-9975-4945-a9bf-f07b390ff101"",
    ""correlation_id"": ""a90a7d1c-25de-4bda-b739-dd5911524a69"",
    ""error_uri"": ""https://login.microsoftonline.com/error?code=70000""
}", 400);

            CloudStorageToken oldToken = new CloudStorageToken
            {
                AccessToken = "dummy",
                RefreshToken = "8/A1AAbbZZ9",
            };

            IOAuth2CloudStorageClient client = new TestClient(GetGoogleConfig());
            Assert.ThrowsExceptionAsync<RefreshTokenExpiredException>(() => RefreshTokenAsync(client, oldToken));
        }

        private async Task<CloudStorageToken> RefreshTokenAsync(IOAuth2CloudStorageClient client, CloudStorageToken token)
        {
            return await client.RefreshTokenAsync(token);
        }

        private OAuth2Config GetDropboxConfig()
        {
            return new OAuth2Config
            {
                AuthorizeServiceEndpoint = "https://www.dropbox.com/oauth2/authorize",
                TokenServiceEndpoint = null,
                ClientId = "oaid",
                RedirectUrl = "com.example.myapp://oauth2redirect/",
                Flow = AuthorizationFlow.Token,
                Scope = null,
                ClientSecretHandling = ClientSecretHandling.DoNotSend,
            };
        }

        private OAuth2Config GetGoogleConfig()
        {
            return new OAuth2Config
            {
                AuthorizeServiceEndpoint = "https://accounts.google.com/o/oauth2/v2/auth",
                TokenServiceEndpoint = "https://www.googleapis.com/oauth2/v4/token",
                ClientId = "oaid",
                RedirectUrl = "com.example.myapp://oauth2redirect/",
                Flow = AuthorizationFlow.Code,
                Scope = "https://www.googleapis.com/auth/drive.appdata",
                ClientSecretHandling = ClientSecretHandling.SendEmptyParam,
            };
        }

        /// <summary>
        /// Non abstract test class.
        /// </summary>
        private class TestClient : OAuth2CloudStorageClient
        {
            public TestClient(OAuth2Config config)
                : base(config)
            {
            }

            public override CloudStorageCredentialsRequirements CredentialsRequirements => throw new NotImplementedException();

            public override Task<byte[]> DownloadFileAsync(string filename, CloudStorageCredentials credentials)
            {
                throw new NotImplementedException();
            }

            public override Task<List<string>> ListFileNamesAsync(CloudStorageCredentials credentials)
            {
                throw new NotImplementedException();
            }

            public override Task UploadFileAsync(string filename, byte[] fileContent, CloudStorageCredentials credentials)
            {
                throw new NotImplementedException();
            }

            public override Task DeleteFileAsync(string filename, CloudStorageCredentials credentials)
            {
                throw new NotImplementedException();
            }
        }
    }
}
