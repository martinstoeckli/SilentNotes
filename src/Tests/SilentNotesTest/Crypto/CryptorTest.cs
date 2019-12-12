using System;
using System.Security;
using NUnit.Framework;
using SilentNotes.Crypto;
using SilentNotes.Crypto.KeyDerivation;
using SilentNotes.Crypto.SymmetricEncryption;
using SilentNotes.Services;

namespace SilentNotesTest.Crypto
{
    [TestFixture]
    public class CryptorTest
    {
        private void TestSymmetricEncryptionWithShortMessage(ISymmetricEncryptionAlgorithm encryptor)
        {
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService();
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
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService();
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
        public void EnsureBackwardsCompatibilityLongTimeDecryptionOfAesGcm()
        {
            // Ensure that a once stored cipher can always be decrypted even after changes in the liberary
            string base64Cipher = "dW5pdHRlc3QkYWVzX2djbSQ0NG04QXBFU1ptcXhnYll2OE5wcWl3PT0kcGJrZGYyJGgwSDdxSGZnVFlXNzBKS3lEb0JLeFE9PSQxMDAwJJsMDjdYEYXYmcqTOFRbge6iVfWo/iny4nrIOMVuoqYak6xB/MAe53G5H3AyxiTi8OENJbi9tzZStpe3p3nlDB7l+J8=";
            byte[] cipher = CryptoUtils.Base64StringToBytes(base64Cipher);
            ICryptor decryptor = new Cryptor("unittest", null);
            string decryptedMessage = CryptoUtils.BytesToString(decryptor.Decrypt(cipher, CryptoUtils.StringToSecureString("brownie")));
            Assert.AreEqual("The brown fox jumps over the lazy 🐢🖐🏿 doc.", decryptedMessage);
        }

        [Test]
        public void EnsureBackwardsCompatibilityDecryptionOfTwofishGcm()
        {
            // Ensure that a once stored cipher can always be decrypted even after changes in the liberary
            string base64Cipher = "dW5pdHRlc3QkdHdvZmlzaF9nY20kZHhMWFh4K0UrZ2MzWHdWc01rWUFxQT09JHBia2RmMiRma1BCWTdDWXp1OG5YUlJtYk9DUlp3PT0kMTAwMCRRc0ETSqDekQuBgKJ5x4Mvy02OHsivm0uJ9KchKdpGk+pmbF4Kq/EDbx9Uw54uEZUQLnK70dNKSEVtb1GyUOX1mitr";
            byte[] cipher = CryptoUtils.Base64StringToBytes(base64Cipher);
            ICryptor decryptor = new Cryptor("unittest", null);
            string decryptedMessage = CryptoUtils.BytesToString(decryptor.Decrypt(cipher, CryptoUtils.StringToSecureString("brownie")));
            Assert.AreEqual("The brown fox jumps over the lazy 🐢🖐🏿 doc.", decryptedMessage);
        }

        [Test]
        public void CryptoPbkdf2()
        {
            const int KeyLength = 42;
            IKeyDerivationFunction kdf = new Pbkdf2();

            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService();
            byte[] salt = randomGenerator.GetRandomBytes(kdf.ExpectedSaltSizeBytes);
            int cost = kdf.RecommendedCost(KeyDerivationCostType.Low);

            SecureString password = CryptoUtils.StringToSecureString("Das ist kein gutes Passwort");
            byte[] key = kdf.DeriveKeyFromPassword(password, KeyLength, salt, cost);
            Assert.AreEqual(KeyLength, key.Length);

            // Same parameters must result in the same output
            byte[] key2 = kdf.DeriveKeyFromPassword(password, KeyLength, salt, cost);
            Assert.AreEqual(key, key2);
        }

        [Test]
        public void CryptorWorksWithPassword()
        {
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService();
            ICryptor encryptor = new Cryptor("sugus", randomGenerator);
            string message = "Der schnelle Fuchs stolpert über den faulen Hund.";
            byte[] binaryMessage = CryptoUtils.StringToBytes(message);
            SecureString password = CryptoUtils.StringToSecureString("Der schnelle Uhu fliegt über den faulen Hund.");

            byte[] cipher = encryptor.Encrypt(binaryMessage, password, KeyDerivationCostType.Low, BouncyCastleTwofishGcm.CryptoAlgorithmName);
            byte[] decryptedMessage = encryptor.Decrypt(cipher, password);
            Assert.AreEqual(binaryMessage, decryptedMessage);
        }

        [Test]
        public void CryptorWorksWithKey()
        {
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService();
            ICryptor encryptor = new Cryptor("sugus", randomGenerator);
            string message = "Der schnelle Fuchs stolpert über den faulen Hund.";
            byte[] binaryMessage = CryptoUtils.StringToBytes(message);
            byte[] key = randomGenerator.GetRandomBytes(32);

            byte[] cipher = encryptor.Encrypt(binaryMessage, key, BouncyCastleTwofishGcm.CryptoAlgorithmName);
            byte[] decryptedMessage = encryptor.Decrypt(cipher, key);
            Assert.AreEqual(binaryMessage, decryptedMessage);
        }

        [Test]
        public void ObfuscationCanBeReversed()
        {
            SecureString obfuscationKey = CryptoUtils.StringToSecureString("A very strong passphrase...");
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService();
            byte[] binaryMessage = randomGenerator.GetRandomBytes(100);

            var obfuscatedMessage = CryptoUtils.Obfuscate(binaryMessage, obfuscationKey, randomGenerator);
            var deobfuscatedMessage = CryptoUtils.Deobfuscate(obfuscatedMessage, obfuscationKey);
            Assert.AreEqual(binaryMessage, deobfuscatedMessage);

            string plaintextMessage = "welcome home";
            string obfuscatedMessageText = CryptoUtils.Obfuscate(plaintextMessage, obfuscationKey, randomGenerator);
            string deobfuscatedMessageText = CryptoUtils.Deobfuscate(obfuscatedMessageText, obfuscationKey);
            Assert.AreEqual(plaintextMessage, deobfuscatedMessageText);
        }

        [Test]
        public void EnsureBackwardsCompatibilityDeobfuscation()
        {
            // Ensure that a once stored obfuscated text can always be deobfuscated even after changes in the liberary
            SecureString obfuscationKey = CryptoUtils.StringToSecureString("A very strong passphrase...");
            string obfuscatedMessage = "b2JmdXNjYXRpb24kdHdvZmlzaF9nY20kU1pmWWpzWWV6MUZ0S0xqZWhHM1FCUT09JHBia2RmMiR0emVXNU9PTWNucEkxaHhWbkt2Y0Z3PT0kMTAwMCQh9UPVY34fufBoywrqb0JjKU/BMqnTABoXfaTsmEudBRVMpMb+Yx+GZIBjNbrWpqMSmWMgiIwfNBlixP0vi7ohAiv9";
            string deobfuscatedMessage = CryptoUtils.Deobfuscate(obfuscatedMessage, obfuscationKey);
            Assert.AreEqual("The brown fox jumps over the lazy 🐢🖐🏿 doc.", deobfuscatedMessage);
        }

        [Test]
        public void TestSymmetricEncryptionWithCompression()
        {
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService(88);
            ICryptor encryptor = new Cryptor("sugus", randomGenerator);
            byte[] binaryMessage = CommonMocksAndStubs.FilledByteArray(1024, 88);
            SecureString password = CryptoUtils.StringToSecureString("Der schnelle Uhu fliegt über den faulen Hund.");

            byte[] cipher = encryptor.Encrypt(
                binaryMessage,
                password,
                KeyDerivationCostType.Low,
                BouncyCastleTwofishGcm.CryptoAlgorithmName,
                Pbkdf2.CryptoKdfName,
                Cryptor.CompressionGzip);
            byte[] decryptedMessage = encryptor.Decrypt(cipher, password);
            Assert.AreEqual(binaryMessage, decryptedMessage);
        }

        [Test]
        public void UnknownAlgorithmThrows()
        {
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService();
            ICryptor encryptor = new Cryptor("sugus", randomGenerator);
            byte[] binaryMessage = new byte[] { 88 };
            SecureString password = CryptoUtils.StringToSecureString("unittestpwd");

            Assert.Throws<CryptoException>(delegate
            {
                byte[] cipher = encryptor.Encrypt(
                    binaryMessage,
                    password,
                    KeyDerivationCostType.Low,
                    "InvalidAlgorithmName");
            });
        }
    }
}
