using Microsoft.VisualStudio.TestTools.UnitTesting;
using VanillaCloudStorageClient.OAuth2;

namespace VanillaCloudStorageClientTest.OAuth2
{
    [TestClass]
    public class AuthorizationResponseErrorTest
    {
        [TestMethod]
        public void ParsesAllErrorCodesCorrectly()
        {
            // see: https://tools.ietf.org/html/rfc6749#section-4.1.2.1
            Assert.AreEqual(AuthorizationResponseError.InvalidRequest, AuthorizationResponseErrorExtensions.StringToAuthorizationResponseError("invalid_request"));
            Assert.AreEqual(AuthorizationResponseError.UnauthorizedClient, AuthorizationResponseErrorExtensions.StringToAuthorizationResponseError("unauthorized_client"));
            Assert.AreEqual(AuthorizationResponseError.AccessDenied, AuthorizationResponseErrorExtensions.StringToAuthorizationResponseError("access_denied"));
            Assert.AreEqual(AuthorizationResponseError.UnsupportedResponseType, AuthorizationResponseErrorExtensions.StringToAuthorizationResponseError("unsupported_response_type"));
            Assert.AreEqual(AuthorizationResponseError.InvalidScope, AuthorizationResponseErrorExtensions.StringToAuthorizationResponseError("invalid_scope"));
            Assert.AreEqual(AuthorizationResponseError.ServerError, AuthorizationResponseErrorExtensions.StringToAuthorizationResponseError("server_error"));
            Assert.AreEqual(AuthorizationResponseError.TemporarilyUnavailable, AuthorizationResponseErrorExtensions.StringToAuthorizationResponseError("temporarily_unavailable"));
        }

        [TestMethod]
        public void ParsesNullErrorCodeCorrectly()
        {
            Assert.IsNull(AuthorizationResponseErrorExtensions.StringToAuthorizationResponseError(null));
            Assert.IsNull(AuthorizationResponseErrorExtensions.StringToAuthorizationResponseError(string.Empty));
        }

        [TestMethod]
        public void ParsesUnknownErrorCodeCorrectly()
        {
            Assert.AreEqual(AuthorizationResponseError.Unknown, AuthorizationResponseErrorExtensions.StringToAuthorizationResponseError("a service should never return this error code"));
        }
    }
}
