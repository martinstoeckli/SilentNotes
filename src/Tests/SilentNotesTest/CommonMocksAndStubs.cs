using System;
using Moq;
using SilentNotes.Services;
using VanillaCloudStorageClient;

namespace SilentNotesTest
{
    /// <summary>
    /// Collection of often used mocks and stubs.
    /// </summary>
    internal static class CommonMocksAndStubs
    {
        /// <summary>
        /// Creates an <see cref="ILanguageService"/> mock which does nothing.
        /// </summary>
        /// <returns>Mock for a language service.</returns>
        public static ILanguageService LanguageService()
        {
            return new Mock<ILanguageService>().Object;
        }

        /// <summary>
        /// Creates a dummy <see cref="IFeedbackService"/> which does nothing.
        /// </summary>
        /// <returns>Dummy service doing nothing.</returns>
        public static IFeedbackService FeedbackService()
        {
            return new DummyFeedbackService();
        }

        /// <summary>
        /// Creates a random service for unittests.
        /// </summary>
        /// <returns>Random service.</returns>
        public static ICryptoRandomService CryptoRandomService()
        {
            return new RandomSource4UnitTest();
        }

        /// <summary>
        /// Creates an <see cref="ICloudStorageClientFactory"/> mock.
        /// </summary>
        /// <returns>Mock for a cloud storage client factory.</returns>
        public static ICloudStorageClientFactory CloudStorageClientFactory()
        {
            return CloudStorageClientFactory(new Mock<ICloudStorageClient>().Object);
        }

        /// <summary>
        /// Creates an <see cref="ICloudStorageClientFactory"/> mock from a given
        /// <paramref name="cloudStorageClient"/>.
        /// </summary>
        /// <param name="cloudStorageClient">Cloud storage client which should be returned.</param>
        /// <returns>Mock for a cloud storage client factory.</returns>
        public static ICloudStorageClientFactory CloudStorageClientFactory(ICloudStorageClient cloudStorageClient)
        {
            Mock<ICloudStorageClientFactory> result = new Mock<ICloudStorageClientFactory>();
            result.
                Setup(m => m.GetOrCreate(It.IsAny<string>())).
                Returns(cloudStorageClient);
            return result.Object;
        }

        /// <summary>
        /// Implementation of the <see cref="ICryptoRandomSource"/> and <see cref="ICryptoRandomService"/>
        /// interface, for usage in UnitTests. Uses an unsafe random generator, so the system's entropy
        /// is not drained.
        /// </summary>
        private class RandomSource4UnitTest : ICryptoRandomService
        {
            private Random _nonCryptoRandom;

            /// <summary>
            /// Initializes a new instance of the <see cref="RandomSource4UnitTest"/> class.
            /// It uses a non safe random generator to build the byte arrays.
            /// </summary>
            public RandomSource4UnitTest()
            {
                _nonCryptoRandom = new Random();
            }

            public byte[] GetRandomBytes(int numberOfBytes)
            {
                if (numberOfBytes <= 0)
                    throw new ArgumentOutOfRangeException("numberOfBytes");

                byte[] result = new byte[numberOfBytes];
                _nonCryptoRandom.NextBytes(result);
                return result;
            }
        }
    }
}
