using System;
using System.Collections.Generic;
using System.Text;
using SilentNotes.Crypto;

namespace SilentNotes.Services
{
    /// <summary>
    /// This service can load encrypted SilentNotes attachements.
    /// Attachements are stored encrypted as separate files in the form '0ec4e088-f2c9-4ade-8521-dc2e7905a4e9.silentnotesatt'.
    /// They are referred from inside the notes as img HTML tags, the link includes the file extension
    /// and the key to decrypt the attachement. Those requests can be intercepted by the app.
    /// <example>
    /// Stored encrypted file: 0ec4e088-f2c9-4ade-8521-dc2e7905a4e9.silentnotesatt
    /// Url with key in note: 0ec4e088-f2c9-4ade-8521-dc2e7905a4e9.silentnotesatt.png?key=keyToDecryptTheAttachement
    /// </example>
    /// </summary>
    /// <remarks>
    /// The key is stored in readable form as part of the note img source, the note containing the
    /// img can be encrypted though, when placed inside a safe. This way the attachement is automatically
    /// protected when the note is encrypted (inside a safe) and readable when the note is unprotected.
    /// </remarks>
    public interface IAttachementService
    {
        /// <summary>
        /// Saves the content of an attachement encrypted to the local storage.
        /// </summary>
        /// <param name="content">The binary content to store.</param>
        /// <param name="fileExtension">The file extension matching this attachement, e.g. ".png"</param>
        /// <param name="randomSource">A cryptographically secure random source.</param>
        /// <returns>Relative to the stored attachement. Example: 0ec4e088-f2c9-4ade-8521-dc2e7905a4e9.silentnotesatt.png?key=keyToDecryptTheAttachement</returns>
        Task<string> SaveAttachement(byte[] content, string fileExtension, ICryptoRandomSource randomSource);

        /// <summary>
        /// Loads the encrypted content of the attachement from the local storage and creates a
        /// response for an intercepted WebResourceRequest of the WebView.
        /// </summary>
        /// <param name="resourceUri">The requested URI as stored in the note, containing the
        /// identifier '.silentnotesatt' and the key to decrypt the attachement.
        /// Example: 0ec4e088-f2c9-4ade-8521-dc2e7905a4e9.silentnotesatt.png?key=myRandomSecret
        /// </param>
        /// <param name="queryParameters">The query parameters of the request.</param>
        /// <returns>All necessary information to call the WebViewWebResourceRequestedEventArgs.SetResponse()
        /// function, or null if the <paramref name="resourceUri"/> doesn't belong to a SilentNotes
        /// attachement.</returns>
        WebResourceResponse LoadAttachement(Uri resourceUri, IReadOnlyDictionary<string, string> queryParameters);
    }

    /// <summary>
    /// Contains all necessary parameters for an intercepted WebResourceRequest of the WebView.
    /// </summary>
    public class WebResourceResponse
    {
        /// <summary>
        /// The HTML status code, e.g. 200 for HttpStatusCode.OK.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// The verbose description of the <see cref="StatusCode"/>, e.g. "OK" for HttpStatusCode.OK.
        /// </summary>
        public string ReasonPhrase { get; set; }

        /// <summary>
        /// The mime type of the resource, e.g. "image/jpeg"
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// A async function which can return an (decrypted) stream with the content of the resource.
        /// </summary>
        public Task<Stream> AttachementLoader { get; set; }
    }
}
