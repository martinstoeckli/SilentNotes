using System.Net;
using SilentNotes.Crypto;
using SilentNotes.Crypto.SymmetricEncryption;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="IAttachementService"/> interface.
    /// </summary>
    public class AttachementService : IAttachementService
    {
        private const string AttachementFileIdentifier = ".silentnotesatt";

        private readonly string _appDataDirectory;
        private readonly IFileService _fileService;
        private readonly ICryptor _cryptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="AttachementService"/> class.
        /// </summary>
        /// <param name="fileService">Service which can read and write files.</param>
        /// <param name="appDataDirectory">Directory where the application can write its data to.</param>
        public AttachementService(string appDataDirectory, IFileService fileService, ICryptor cryptor)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(appDataDirectory);
            ArgumentNullException.ThrowIfNull(fileService);
            ArgumentNullException.ThrowIfNull(cryptor);

            _appDataDirectory = appDataDirectory;
            _fileService = fileService;
            _cryptor = cryptor;
        }

        /// <summary>
        /// Determines whether the specified URI belongs to a SilentNotes attachment.
        /// </summary>
        /// <param name="uri">The URI to check, or null.</param>
        /// <returns>Returns true if the URI belongs to a SilentNotes attachement, otherwise false.</returns>
        public static bool IsAttachementUri(Uri uri)
        {
            return uri != null && uri.IsAbsoluteUri && (uri?.AbsoluteUri?.Contains(AttachementFileIdentifier) ?? false);
        }

        /// <inheritdoc/>
        public async Task<string> SaveAttachement(byte[] content, string fileExtension, ICryptoRandomSource randomSource)
        {
            ArgumentNullException.ThrowIfNull(randomSource);

            Guid id = Guid.NewGuid();
            string attachementFileName = FormatAttachementFileName(id);
            string attachementPath = Path.Combine(_appDataDirectory, attachementFileName);

            // Since attachements are immutable, and are not reencrypted when the user chooses
            // another algorithm in the settings, we make an appropriate choice of the algorithm.
            byte[] encryptionKey = randomSource.GetRandomBytes(BouncyCastleXChaCha20.KeySizeBytes);
            byte[] encryptedContent = _cryptor.Encrypt(content, encryptionKey, BouncyCastleXChaCha20.CryptoAlgorithmName);

            await _fileService.WriteAsync(attachementPath, encryptedContent);
            string urlFileName = attachementFileName + '.' + fileExtension.Trim('.');
            string base64Key = CryptoUtils.BytesToBase64String(encryptionKey);
            return string.Format("{0}?key={1}", urlFileName, Uri.EscapeDataString(base64Key));
        }

        /// <inheritdoc/>
        public WebResourceResponse LoadAttachement(Uri resourceUri, IReadOnlyDictionary<string, string> queryParameters)
        {
            if (!IsAttachementUri(resourceUri))
                return null;

            string attachementFileName = Path.GetFileNameWithoutExtension(resourceUri.AbsoluteUri);
            string attachementPath = Path.Combine(_appDataDirectory, attachementFileName);
            if (_fileService.Exists(attachementPath))
            {
                string encodedKey = queryParameters.GetValueOrDefault("key");
                byte[] key = CryptoUtils.Base64StringToBytes(encodedKey);
                return new WebResourceResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    ReasonPhrase = new HttpResponseMessage(HttpStatusCode.OK).ReasonPhrase,
                    MimeType = GetImageMimeType(resourceUri.AbsolutePath),
                    AttachementLoader = LoadDecryptedImageAsync(attachementPath, key)
                };
            }
            else
            {
                return new WebResourceResponse
                {
                    StatusCode = (int)HttpStatusCode.NotFound,
                    ReasonPhrase = new HttpResponseMessage(HttpStatusCode.NotFound).ReasonPhrase,
                    MimeType = null,
                    AttachementLoader = null
                };
            }
        }

        private static string FormatAttachementFileName(Guid id)
        {
            return string.Format("{0}{1}", id.ToString("D"), AttachementFileIdentifier);
        }

        private static string GetImageMimeType(string fileName)
        {
            string fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
            switch (fileExtension)
            {
                case ".jpg" or ".jpeg": return "image/jpeg";
                case ".png": return "image/png";
                case ".svg": return "image/svg+xml";
                default: return "application/octet-stream";
            }
        }

        private async Task<Stream> LoadDecryptedImageAsync(string attachementPath, byte[] key)
        {
            try
            {
                byte[] encrypted = await _fileService.ReadAsync(attachementPath);
                byte[] decrypted = _cryptor.Decrypt(encrypted, key);
                return new MemoryStream(decrypted);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
