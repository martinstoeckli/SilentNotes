using System.Net;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SilentNotes.Crypto;
using SilentNotes.Services;

namespace SilentNotesTest.Services
{
    [TestClass]
    public class AttachementServiceTest
    {
        private const string AttachementDirectory = "appdata";
        private const string AttachementUrl = "https://0.0.0.1/0ec4e088-f2c9-4ade-8521-dc2e7905a4e9.silentnotesatt.png?key=rILwye5tIUYmMYJ4OfvuYFJv%2BjLcxdvX%2B9erdLhje4g%3D";

        [TestMethod]
        [DataRow("https://0.0.0.0/0ec4e088.silentnotesatt.png?key=abc")]
        [DataRow("https://0.0.0.0/sub/0ec4e088.silentnotesatt.jpg")]
        [DataRow("https://0.0.0.0/0ec4e088.silentnotesatt.svg")]
        [DataRow("https://0ec4e088.silentnotesatt.svg")]
        public void IsAttachementUri_DetectsAttachementUrls(string url)
        {
            Assert.IsTrue(AttachementService.IsAttachementUri(new Uri(url)));
        }

        [TestMethod]
        [DataRow("https://0.0.0.0/index.html")]
        [DataRow("https://0.0.0.0/image.png?key=abc")]
        [DataRow("https://0.0.0.0/")]
        public void IsAttachementUri_RejectsNonAttachementUrls(string url)
        {
            Assert.IsFalse(AttachementService.IsAttachementUri(new Uri(url)));
        }

        [TestMethod]
        public void IsAttachementUri_ReturnsFalseForNull()
        {
            Assert.IsFalse(AttachementService.IsAttachementUri(null));
        }

        [TestMethod]
        public async Task SaveAttachement_WritesEncryptedContentAndReturnsFileNameWithKey()
        {
            var content = new byte[] { 1, 2, 3 };
            var encryptedContent = new byte[] { 9, 8, 7, 6 };
            var cryptorMock = new Mock<ICryptor>();
            cryptorMock
                .Setup(m => m.Encrypt(content, It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(encryptedContent);
            var fileServiceMock = new Mock<IFileService>();

            var attachementService = new AttachementService(AttachementDirectory, fileServiceMock.Object, cryptorMock.Object);
            string result = await attachementService.SaveAttachement(content, ".png", CommonMocksAndStubs.CryptoRandomService());

            fileServiceMock.Verify(
                m => m.WriteAsync(It.IsAny<string>(), It.Is<byte[]>(c => c.SequenceEqual(encryptedContent)), It.IsAny<CancellationToken>()),
                Times.Once);
            Assert.IsTrue(result.Contains(".silentnotesatt.png?key="));
            Assert.IsTrue(Guid.TryParse(result.Substring(0, result.IndexOf('.')), out _));
            string encodedKey = result.Substring(result.IndexOf("key=") + 4);
            Assert.IsFalse(encodedKey.Contains('=')); // The '=' from base64 encoding must not be part of the url
            CryptoUtils.Base64StringToBytes(Uri.UnescapeDataString(encodedKey)); // Must not throw / must contain a base64 string
        }

        [TestMethod]
        public async Task SaveAttachement_GeneratesUniqueFileNames()
        {
            var content = new byte[] { 1, 2, 3 };
            var cryptorMock = new Mock<ICryptor>();
            var fileServiceMock = new Mock<IFileService>();

            var attachementService = new AttachementService(AttachementDirectory, fileServiceMock.Object, cryptorMock.Object);
            string result1 = await attachementService.SaveAttachement(content, ".png", CommonMocksAndStubs.CryptoRandomService());
            string result2 = await attachementService.SaveAttachement(content, ".png", CommonMocksAndStubs.CryptoRandomService());

            string idPart1 = result1.Substring(0, result1.IndexOf(".silentnotesatt"));
            string idPart2 = result2.Substring(0, result1.IndexOf(".silentnotesatt"));
            Assert.AreNotEqual(idPart1, idPart2);
        }

        [TestMethod]
        public async Task SaveAttachement_ResultIsRecognizedAsAttachementUri()
        {
            var content = new byte[] { 1, 2, 3 };
            var cryptorMock = new Mock<ICryptor>();
            var fileServiceMock = new Mock<IFileService>();

            var attachementService = new AttachementService(AttachementDirectory, fileServiceMock.Object, cryptorMock.Object);
            string result = await attachementService.SaveAttachement(content, ".png", CommonMocksAndStubs.CryptoRandomService());

            Assert.IsTrue(AttachementService.IsAttachementUri(new Uri("https://0.0.0.1/" + result)));
        }

        [TestMethod]
        public async Task SaveAttachement_PropagatesWriteException()
        {
            var content = new byte[] { 1, 2, 3 };
            var cryptorMock = new Mock<ICryptor>();
            var fileServiceMock = new Mock<IFileService>();
            fileServiceMock
                .Setup(m => m.WriteAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("Simulated write error"));

            var attachementService = new AttachementService(AttachementDirectory, fileServiceMock.Object, cryptorMock.Object);
            await Assert.ThrowsExactlyAsync<IOException>(() =>
                attachementService.SaveAttachement(content, ".png", CommonMocksAndStubs.CryptoRandomService()));
        }

        /// <summary>
        /// Can be used to create a real encrypted attachement for testing purposes.
        /// The attachement file is written to the same directory.
        /// </summary>
        [TestMethod]
        [Ignore]
        public async Task GenerateRealEncryptedAttachement()
        {
            // Path to the (image) file to encrypt.
            const string directory = @"D:\temp";
            const string attachementFileName = "BackgroundRose.jpg";

            byte[] content = await File.ReadAllBytesAsync(Path.Combine(directory, attachementFileName));
            ICryptoRandomSource randomSource = new CryptoRandomService();

            var attachementService = new AttachementService(
                directory,
                new FileService(),
                new Cryptor("SilentAttachement", randomSource));

            // The url to the attachement which can be used in the <img src="url" loading="lazy"/> tag
            string url = await attachementService.SaveAttachement(content, Path.GetExtension(attachementFileName), randomSource);
        }

        [TestMethod]
        public void LoadAttachement_ReturnsNullForNonAttachementUri()
        {
            var fileServiceMock = new Mock<IFileService>();
            var encryptorMock = new Mock<ICryptor>();
            var attachementService = new AttachementService(AttachementDirectory, fileServiceMock.Object, encryptorMock.Object);

            var queryParameters = CreateQueryParameters();
            var response = attachementService.LoadAttachement(new Uri("https://0.0.0.0/index.html"), queryParameters);

            Assert.IsNull(response);
        }

        [TestMethod]
        public void LoadAttachement_ReturnsNotFoundForMissingFile()
        {
            var fileServiceMock = new Mock<IFileService>();
            fileServiceMock
                .Setup(m => m.Exists(It.IsAny<string>()))
                .Returns(false);
            var encryptorMock = new Mock<ICryptor>();
            var attachementService = new AttachementService(AttachementDirectory, fileServiceMock.Object, encryptorMock.Object);

            var queryParameters = CreateQueryParameters();
            var response = attachementService.LoadAttachement(new Uri(AttachementUrl), queryParameters);

            Assert.IsNotNull(response);
            Assert.AreEqual((int)HttpStatusCode.NotFound, response.StatusCode);
            Assert.AreEqual("Not Found", response.ReasonPhrase);
        }

        [TestMethod]
        public async Task LoadAttachement_ReturnsOkWithContentForExistingFile()
        {
            var encryptionKey = new byte[] { 11, 22, 33 };
            var deryptedContent = new byte[] { 44, 55, 66 };

            var fileServiceMock = new Mock<IFileService>();
            fileServiceMock
                .Setup(m => m.Exists(It.IsAny<string>()))
                .Returns(true);
            var encryptorMock = new Mock<ICryptor>();
            encryptorMock
                .Setup(m => m.Decrypt(It.IsAny<byte[]>(), It.Is<byte[]>(k => k.SequenceEqual(encryptionKey))))
                .Returns(deryptedContent);
            var attachementService = new AttachementService(AttachementDirectory, fileServiceMock.Object, encryptorMock.Object);

            var queryParameters = CreateQueryParameters(encryptionKey);
            var response = attachementService.LoadAttachement(new Uri(AttachementUrl), queryParameters);

            Assert.IsNotNull(response);
            Assert.AreEqual((int)HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("OK", response.ReasonPhrase);
            Assert.AreEqual("image/png", response.MimeType);

            Stream contentStream = await response.AttachementLoader;
            using (var memoryStream = new MemoryStream())
            {
                contentStream.CopyTo(memoryStream);
                Assert.IsTrue(deryptedContent.SequenceEqual(memoryStream.ToArray()));
            }
        }

        [TestMethod]
        [DataRow(".png", "image/png")]
        [DataRow(".PNG", "image/png")]
        [DataRow(".jpg", "image/jpeg")]
        [DataRow(".jpeg", "image/jpeg")]
        [DataRow(".svg", "image/svg+xml")]
        [DataRow(".xyz", "application/octet-stream")]
        public void LoadAttachement_DetectsMimeTypeFromExtension(string extension, string expectedMimeType)
        {
            var fileServiceMock = new Mock<IFileService>();
            fileServiceMock
                .Setup(m => m.Exists(It.IsAny<string>()))
                .Returns(true);
            var encryptorMock = new Mock<ICryptor>();
            var attachementService = new AttachementService(AttachementDirectory, fileServiceMock.Object, encryptorMock.Object);

            var uri = new Uri("https://0.0.0.1/0ec4e088-f2c9-4ade-8521-dc2e7905a4e9.silentnotesatt" + extension);
            Dictionary<string, string> queryParameters = CreateQueryParameters(new byte[] { 11, 22, 33 });
            var response = attachementService.LoadAttachement(uri, queryParameters);
            Assert.AreEqual(expectedMimeType, response.MimeType);
        }

        [TestMethod]
        public async Task LoadAttachement_LoaderReturnsNullIfReadingFails()
        {
            var encryptionKey = new byte[] { 11, 22, 33 };

            var fileServiceMock = new Mock<IFileService>();
            fileServiceMock
                .Setup(m => m.Exists(It.IsAny<string>()))
                .Returns(true);
            fileServiceMock
                .Setup(m => m.ReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new IOException("Simulated read error"));
            var encryptorMock = new Mock<ICryptor>();
            var attachementService = new AttachementService(AttachementDirectory, fileServiceMock.Object, encryptorMock.Object);

            var queryParameters = CreateQueryParameters(encryptionKey);
            var response = attachementService.LoadAttachement(new Uri(AttachementUrl), queryParameters);

            Assert.AreEqual((int)HttpStatusCode.OK, response.StatusCode);
            Assert.IsNull(await response.AttachementLoader);
        }

        private static Dictionary<string, string> CreateQueryParameters(byte[] key = null)
        {
            var result = new Dictionary<string, string>();
            if (key != null)
                result["key"] = CryptoUtils.BytesToBase64String(key);
            return result;
        }

        /// <summary>
        /// Helper class to generate cryptographically secure randomness.
        /// </summary>
        private class CryptoRandomService : ICryptoRandomService
        {
            /// <inheritdoc/>
            public byte[] GetRandomBytes(int numberOfBytes)
            {
                return RandomNumberGenerator.GetBytes(numberOfBytes);
            }
        }
    }
}
