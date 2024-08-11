using System;
using Moq;
using SilentNotes.Models;
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
        /// Creates an <see cref="ILanguageService"/> mock which, instead of returning a localized
        /// text, returns the resource key itself.
        /// </summary>
        /// <param name="key">The key of the language resource.</param>
        /// <returns>Mock for a language service.</returns>
        public static ILanguageService LanguageService(string key)
        {
            return LanguageService(new string[] { key });
        }

        /// <summary>
        /// Creates an <see cref="ILanguageService"/> mock which, instead of returning a localized
        /// text, returns the resource key itself.
        /// </summary>
        /// <param name="keys">The keys of the language resources.</param>
        /// <returns>Mock for a language service.</returns>
        public static ILanguageService LanguageService(IEnumerable<string> keys)
        {
            var result = new Mock<ILanguageService>();
            foreach (string keyAndText in keys)
            {
                result
                    .SetupGet(m => m[It.Is<string>(k => k == keyAndText)])
                    .Returns(keyAndText);
            }
            return result.Object;
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
        /// Creates a <see cref="ISettingsService"/> mock.
        /// </summary>
        /// <returns>Mock for settings service.</returns>
        public static ISettingsService SettingsService()
        {
            return SettingsService(new SettingsModel());
        }

        /// <summary>
        /// Creates a <see cref="ISettingsService"/> mock which returns <paramref name="settings"/>
        /// if asked.
        /// </summary>
        /// <param name="settings">The settings to provide.</param>
        /// <returns>Mock for settings service.</returns>
        public static ISettingsService SettingsService(SettingsModel settings)
        {
            Mock<ISettingsService> result = new Mock<ISettingsService>();
            result.
                Setup(m => m.LoadSettingsOrDefault()).Returns(settings);
            return result.Object;
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
        /// Creates a random service for unittests with a predictable output.
        /// </summary>
        /// <param name="predictableOutput">Returns output only containing this byte.</param>
        /// <returns>Random service.</returns>
        public static ICryptoRandomService CryptoRandomService(byte predictableOutput)
        {
            return new RandomSource4UnitTest(predictableOutput);
        }

        /// <summary>
        /// Creates an environment service for unittests.
        /// </summary>
        /// <returns>Environment service.</returns>
        public static IEnvironmentService EnvironmentService()
        {
            Mock<IEnvironmentService> environmentService = new Mock<IEnvironmentService>();
            environmentService
                .SetupGet(p => p.Os)
                .Returns(SilentNotes.Services.OperatingSystem.Windows);
            return environmentService.Object;
        }

        public static byte[] FilledByteArray(int length, byte fill)
        {
            byte[] result = new byte[length];
            for (int index = 0; index < length; index++)
                result[index] = fill;
            return result;
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
                Setup(m => m.GetByKey(It.IsAny<string>())).
                Returns(cloudStorageClient);
            return result.Object;
        }

        /// <summary>
        /// Creates an <see cref="IRepositoryStorageService"/> mock, which returns a given
        /// <paramref name="repository"/>.
        /// </summary>
        /// <param name="repository">The repository to return when calling <see cref="IRepositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel)"/>.</param>
        /// <returns>Mock for repository storage service.</returns>
        public static Mock<IRepositoryStorageService> RepositoryStorageServiceMock(NoteRepositoryModel repository)
        {
            Mock<IRepositoryStorageService> result = new Mock<IRepositoryStorageService>();
            result.
                Setup(m => m.LoadRepositoryOrDefault(out repository));
            return result;
        }

        /// <summary>
        /// Implementation of the <see cref="ICryptoRandomSource"/> and <see cref="ICryptoRandomService"/>
        /// interface, for usage in UnitTests. Uses an unsafe random generator, so the system's entropy
        /// is not drained.
        /// </summary>
        private class RandomSource4UnitTest : ICryptoRandomService
        {
            private readonly Random _nonCryptoRandom;
            private readonly byte _predictableOutput;

            /// <summary>
            /// Initializes a new instance of the <see cref="RandomSource4UnitTest"/> class.
            /// It uses a non safe random generator to build the byte arrays.
            /// </summary>
            public RandomSource4UnitTest()
            {
                _nonCryptoRandom = new Random();
            }

            public RandomSource4UnitTest(byte predictableOutput)
            {
                _predictableOutput = predictableOutput;
            }

            public byte[] GetRandomBytes(int numberOfBytes)
            {
                if (numberOfBytes <= 0)
                    throw new ArgumentOutOfRangeException("numberOfBytes");

                if (_nonCryptoRandom != null)
                {
                    byte[] result = new byte[numberOfBytes];
                    _nonCryptoRandom.NextBytes(result);
                    return result;
                }
                else
                {
                    return FilledByteArray(numberOfBytes, _predictableOutput);
                }
            }
        }
    }
}
