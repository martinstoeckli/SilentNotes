using System;
using NUnit.Framework;
using VanillaCloudStorageClient;
using VanillaCloudStorageClient.OAuth2;

namespace VanillaCloudStorageClientTest.OAuth2
{
    [TestFixture]
    public class OAuth2UtilsTest
    {
        [Test]
        public void BuildAuthorizationRequestUrlUsesAllParameters()
        {
            OAuth2Config config = new OAuth2Config
            {
                AuthorizeServiceEndpoint = @"https://example.com/authorize",
                ClientId = "fwser2sewr689f7",
                RedirectUrl = "com.example.myapp://oauth2redirect/",
            };
            string state = "7ysv8L9s4LB9CZpA";
            string url = OAuth2Utils.BuildAuthorizationRequestUrl(config, state, null);

            Assert.AreEqual(@"https://example.com/authorize?response_type=token&client_id=fwser2sewr689f7&redirect_uri=com.example.myapp%3A%2F%2Foauth2redirect%2F&state=7ysv8L9s4LB9CZpA", url);
        }

        [Test]
        public void BuildAuthorizationRequestUrlEscapesParameters()
        {
            OAuth2Config config = new OAuth2Config
            {
                AuthorizeServiceEndpoint = @"https://example.com/authorize",
                ClientId = "a:a",
                RedirectUrl = "b:b",
            };
            string state = "c:c";
            string url = OAuth2Utils.BuildAuthorizationRequestUrl(config, state, null);

            Assert.IsTrue(url.Contains("client_id=a%3Aa"));
            Assert.IsTrue(url.Contains("redirect_uri=b%3Ab"));
            Assert.IsTrue(url.Contains("state=c%3Ac"));
        }

        [Test]
        public void BuildAuthorizationRequestUrlThrowsWithMissingRedirectUrlForTokenFlow()
        {
            OAuth2Config config = new OAuth2Config
            {
                AuthorizeServiceEndpoint = @"https://example.com/authorize",
                ClientId = "a",
                RedirectUrl = null, // Must exist for token workflow
            };
            Assert.Throws<InvalidParameterException>(() => OAuth2Utils.BuildAuthorizationRequestUrl(config, "state", null));
        }

        [Test]
        public void BuildAuthorizationRequestUrlLeavesOutOptionalParameters()
        {
            OAuth2Config config = new OAuth2Config
            {
                // Scope won't be set
                AuthorizeServiceEndpoint = @"https://example.com/authorize",
                ClientId = "a",
                RedirectUrl = "b",
                Flow = AuthorizationFlow.Code,
            };

            // The scope paramter should not be part of the url
            string url = OAuth2Utils.BuildAuthorizationRequestUrl(config, "c", null);
            Assert.IsTrue(!url.Contains("token", StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void BuildAuthorizationRequestUrlUsesCodeVerifier()
        {
            OAuth2Config config = new OAuth2Config
            {
                AuthorizeServiceEndpoint = @"https://example.com/authorize",
                ClientId = "a",
                RedirectUrl = "b",
                Flow = AuthorizationFlow.Code,
            };

            // The scope paramter should not be part of the url
            string codeVerifier = "ccc";
            string url = OAuth2Utils.BuildAuthorizationRequestUrl(config, "c", codeVerifier);
            Assert.IsTrue(url.Contains("code_challenge=", StringComparison.InvariantCultureIgnoreCase));
            Assert.IsTrue(url.Contains("code_challenge_method=S256", StringComparison.InvariantCultureIgnoreCase));
        }

        [Test]
        public void ParseRealWorldDropboxSuccessResponse()
        {
            string redirectUrl = "com.example.myapp://oauth2redirect/#access_token=vQGkdNzLZ9AAAAAAApZW8zLyjRRRRRcic2MqEx1A1AdyaPcLdbKKOLNg2I8z0we-&token_type=bearer&state=68D8kUO0ubb78C8k&uid=111111111&account_id=abcd%3AAADucBH8TWrbYMWUHlrWQ4TGdcyC55pzBKk";
            AuthorizationResponse response = OAuth2Utils.ParseAuthorizationResponseUrl(redirectUrl);

            Assert.IsTrue(response.IsAccessGranted);
            Assert.AreEqual("vQGkdNzLZ9AAAAAAApZW8zLyjRRRRRcic2MqEx1A1AdyaPcLdbKKOLNg2I8z0we-", response.Token);
            Assert.IsNull(response.Code);
            Assert.AreEqual("68D8kUO0ubb78C8k", response.State);
            Assert.IsNull(response.Error);
        }

        [Test]
        public void ParseRealWorldDropboxRejectResponse()
        {
            string redirectUrl = "com.example.myapp://oauth2redirect/#state=AJ7CQLlJEwNn2AVL&error_description=The+user+chose+not+to+give+your+app+access+to+their+Dropbox+account.&error=access_denied";
            AuthorizationResponse response = OAuth2Utils.ParseAuthorizationResponseUrl(redirectUrl);

            Assert.IsFalse(response.IsAccessGranted);
            Assert.IsNull(response.Token);
            Assert.IsNull(response.Code);
            Assert.AreEqual("AJ7CQLlJEwNn2AVL", response.State);
            Assert.AreEqual(AuthorizationResponseError.AccessDenied, response.Error);
        }

        [Test]
        public void ParseRealWorldGoogleSuccessResponse()
        {
            string redirectUrl = "com.example.myapp:/oauth2redirect/?state=AJ7CQLlJEwNn2AVL&code=4/aQHgCtVfeTg--SEAyJ6pYHvcCtZZZZZckvGcT5OpPjNuEEEEcvUJzQSaAALzD_DSfenHwHXItOE2Ax55j25-bbY&scope=https://www.googleapis.com/auth/drive.appdata";
            AuthorizationResponse response = OAuth2Utils.ParseAuthorizationResponseUrl(redirectUrl);

            Assert.IsTrue(response.IsAccessGranted);
            Assert.AreEqual("4/aQHgCtVfeTg--SEAyJ6pYHvcCtZZZZZckvGcT5OpPjNuEEEEcvUJzQSaAALzD_DSfenHwHXItOE2Ax55j25-bbY", response.Code);
            Assert.IsNull(response.Token);
            Assert.AreEqual("AJ7CQLlJEwNn2AVL", response.State);
            Assert.IsNull(response.Error);
        }

        [Test]
        public void ParseRealWorldGoogleRejectResponse()
        {
            string redirectUrl = "com.example.myapp:/oauth2redirect/?error=access_denied&state=AJ7CQLlJEwNn2AVL";
            AuthorizationResponse response = OAuth2Utils.ParseAuthorizationResponseUrl(redirectUrl);

            Assert.IsFalse(response.IsAccessGranted);
            Assert.IsNull(response.Token);
            Assert.IsNull(response.Code);
            Assert.AreEqual("AJ7CQLlJEwNn2AVL", response.State);
            Assert.AreEqual(AuthorizationResponseError.AccessDenied, response.Error);
        }
    }
}