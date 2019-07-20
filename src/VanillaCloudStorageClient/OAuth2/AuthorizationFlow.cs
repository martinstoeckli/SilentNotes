using System;

namespace VanillaCloudStorageClient.OAuth2
{
    /// <summary>
    /// Enumeration of possible flow types of an authorization request.
    /// </summary>
    public enum AuthorizationFlow
    {
        /// <summary>
        /// The token-flow or implicit-grant-flow returns the bearer token to the url specified in
        /// <see cref="OAuth2Config.RedirectUrl"/>, rather than requiring your app to
        /// make a second call to a server. This is useful for pure client-side apps, such as
        /// mobile apps, and therefore cannot keep secrets.
        /// </summary>
        Token,

        /// <summary>
        /// The code-flow returns a code to the url specified in <see cref="OAuth2Config.RedirectUrl"/>
        /// which should then be converted into a bearer token using the oauth2 token endpoint.
        /// This is the recommended flow for apps that are running on a server and therefore can
        /// keep secrets.
        /// </summary>
        Code,
    }
}
