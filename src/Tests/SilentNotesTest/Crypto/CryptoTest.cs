using System;
using NUnit.Framework;
using SilentNotes.Crypto;
using SilentNotes.Crypto.KeyDerivation;
using SilentNotes.Crypto.SymmetricEncryption;
using SilentNotes.Services;

namespace SilentNotesTest.Crypto
{
    [TestFixture]
    public class CryptoTest
    {
        private void TestSymmetricEncryptionWithShortMessage(ISymmetricEncryptionAlgorithm encryptor)
        {
            ICryptoRandomService randomGenerator = new RandomSource4UnitTest();
            byte[] key = randomGenerator.GetRandomBytes(encryptor.ExpectedKeySize);
            byte[] nonce = randomGenerator.GetRandomBytes(encryptor.ExpectedNonceSize);
            string message = "Dies ist ein kurzer Text";

            byte[] binaryMessage = CryptoUtils.StringToBytes(message);
            byte[] cipher = encryptor.Encrypt(binaryMessage, key, nonce);
            Assert.AreNotEqual(binaryMessage, cipher);
            string message2 = CryptoUtils.BytesToString(encryptor.Decrypt(cipher, key, nonce));
            Assert.AreEqual(message, message2);
        }

        private void TestSymmetricEncryptionWithLongMessage(ISymmetricEncryptionAlgorithm encryptor)
        {
            ICryptoRandomService randomGenerator = new RandomSource4UnitTest();
            byte[] key = randomGenerator.GetRandomBytes(encryptor.ExpectedKeySize);
            byte[] nonce = randomGenerator.GetRandomBytes(encryptor.ExpectedNonceSize);
            byte[] message = randomGenerator.GetRandomBytes(1523687);

            byte[] cipher = encryptor.Encrypt(message, key, nonce);
            Assert.AreNotEqual(message, cipher);
            byte[] message2 = encryptor.Decrypt(cipher, key, nonce);
            Assert.AreEqual(message, message2);
        }

        [Test]
        public void CryptoAesGcm()
        {
            TestSymmetricEncryptionWithShortMessage(new BouncyCastleAesGcm());
            TestSymmetricEncryptionWithLongMessage(new BouncyCastleAesGcm());
        }

        [Test]
        public void CryptoTwofishGcm()
        {
            TestSymmetricEncryptionWithShortMessage(new BouncyCastleTwofishGcm());
            TestSymmetricEncryptionWithLongMessage(new BouncyCastleTwofishGcm());
        }

        [Test]
        public void EnsureLongTimeDecryptionOfAesGcm()
        {
            // Ensure that a once stored cipher can always be decrypted even after changes in the liberary
            string base64Cipher = "dW5pdHRlc3QkYWVzX2djbSQ0NG04QXBFU1ptcXhnYll2OE5wcWl3PT0kcGJrZGYyJGgwSDdxSGZnVFlXNzBKS3lEb0JLeFE9PSQxMDAwJJsMDjdYEYXYmcqTOFRbge6iVfWo/iny4nrIOMVuoqYak6xB/MAe53G5H3AyxiTi8OENJbi9tzZStpe3p3nlDB7l+J8=";
            byte[] cipher = CryptoUtils.Base64StringToBytes(base64Cipher);
            EncryptorDecryptor decryptor = new EncryptorDecryptor("unittest");
            string decryptedMessage = CryptoUtils.BytesToString(decryptor.Decrypt(cipher, "brownie"));
            Assert.AreEqual("The brown fox jumps over the lazy 🐢🖐🏿 doc.", decryptedMessage);
        }

        [Test]
        public void EnsureLongTimeDecryptionOfTwofishGcm()
        {
            // Ensure that a once stored cipher can always be decrypted even after changes in the liberary
            string base64Cipher = "dW5pdHRlc3QkdHdvZmlzaF9nY20kZHhMWFh4K0UrZ2MzWHdWc01rWUFxQT09JHBia2RmMiRma1BCWTdDWXp1OG5YUlJtYk9DUlp3PT0kMTAwMCRRc0ETSqDekQuBgKJ5x4Mvy02OHsivm0uJ9KchKdpGk+pmbF4Kq/EDbx9Uw54uEZUQLnK70dNKSEVtb1GyUOX1mitr";
            byte[] cipher = CryptoUtils.Base64StringToBytes(base64Cipher);
            EncryptorDecryptor decryptor = new EncryptorDecryptor("unittest");
            string decryptedMessage = CryptoUtils.BytesToString(decryptor.Decrypt(cipher, "brownie"));
            Assert.AreEqual("The brown fox jumps over the lazy 🐢🖐🏿 doc.", decryptedMessage);
        }

        [Test]
        public void CryptoPbkdf2()
        {
            const int KeyLength = 42;
            IKeyDerivationFunction kdf = new Pbkdf2();

            ICryptoRandomService randomGenerator = new RandomSource4UnitTest();
            byte[] salt = randomGenerator.GetRandomBytes(kdf.ExpectedSaltSizeBytes);
            int cost = kdf.RecommendedCost(KeyDerivationCostType.Low);

            string password = "Das ist kein gutes Passwort";
            byte[] key = kdf.DeriveKeyFromPassword(password, KeyLength, salt, cost);
            Assert.AreEqual(KeyLength, key.Length);

            // Same parameters must result in the same output
            byte[] key2 = kdf.DeriveKeyFromPassword(password, KeyLength, salt, cost);
            Assert.AreEqual(key, key2);
        }

        [Test]
        public void CryptoTestEncryptor()
        {
            EncryptorDecryptor encryptor = new EncryptorDecryptor("sugus");
            ICryptoRandomService randomGenerator = new RandomSource4UnitTest();
            string message = "Der schnelle Fuchs stolpert über den faulen Hund.";
            byte[] binaryMessage = CryptoUtils.StringToBytes(message);
            string password = "Der schnelle Uhu fliegt über den faulen Hund.";

            byte[] cipher = encryptor.Encrypt(binaryMessage, password, KeyDerivationCostType.Low, randomGenerator, BouncyCastleTwofishGcm.CryptoAlgorithmName);
            byte[] decryptedMessage = encryptor.Decrypt(cipher, password);
            Assert.AreEqual(binaryMessage, decryptedMessage);
        }

        [Test]
        public void GenerateRandomBase62StringGeneratesValidStrings()
        {
            ICryptoRandomService randomGenerator = new RandomSource4UnitTest();

            // check if length always matches the required length
            for (int length = 0; length < 300; length++)
            {
                string randomString = CryptoUtils.GenerateRandomBase62String(length, randomGenerator);
                Assert.AreEqual(length, randomString.Length);
                Assert.IsTrue(IsInBase62Alphabet(randomString));
            }
        }

        private bool IsInBase62Alphabet(string randomString)
        {
            foreach (char c in randomString)
            {
                if (!"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".Contains(c.ToString()))
                    return false;
            }
            return true;
        }
    }
}
