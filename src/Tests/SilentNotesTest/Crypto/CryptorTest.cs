using System;
using System.Linq;
using System.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Crypto;
using SilentNotes.Crypto.KeyDerivation;
using SilentNotes.Crypto.SymmetricEncryption;
using SilentNotes.Services;

namespace SilentNotesTest.Crypto
{
    [TestClass]
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
            CollectionAssert.AreNotEqual(binaryMessage, cipher);
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
            CollectionAssert.AreNotEqual(message, cipher);
            byte[] message2 = encryptor.Decrypt(cipher, key, nonce);
            CollectionAssert.AreEqual(message, message2);
        }

        [TestMethod]
        public void CryptoAesGcm()
        {
            TestSymmetricEncryptionWithShortMessage(new BouncyCastleAesGcm());
            TestSymmetricEncryptionWithLongMessage(new BouncyCastleAesGcm());
        }

        [TestMethod]
        public void CryptoTwofishGcm()
        {
            TestSymmetricEncryptionWithShortMessage(new BouncyCastleTwofishGcm());
            TestSymmetricEncryptionWithLongMessage(new BouncyCastleTwofishGcm());
        }

        [TestMethod]
        public void CryptoXChaCha20()
        {
            TestSymmetricEncryptionWithShortMessage(new BouncyCastleXChaCha20());
            TestSymmetricEncryptionWithLongMessage(new BouncyCastleXChaCha20());
        }

        [TestMethod]
        public void XChaCha20Poly1305CheckTestVector()
        {
            byte[] message = HexToBytes(@"4c616469657320616e642047656e746c656d656e206f662074686520636c617373206f66202739393a204966204920636f756c64206f6666657220796f75206f6e6c79206f6e652074697020666f7220746865206675747572652c2073756e73637265656e20776f756c642062652069742e");
            byte[] key = HexToBytes(@"808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9f");
            byte[] nonce = HexToBytes(@"07000000404142434445464748494a4b0000000000000000");
            byte[] expectedCipher = HexToBytes(@"453c0693a7407f04ff4c56aedb17a3c0a1afff01174930fc22287c33dbcf0ac8b89ad929530a1bb3ab5e69f24c7f6070c8f840c9abb4f69fbfc8a7ff5126faeebbb55805ee9c1cf2ce5a57263287aec5780f04ec324c3514122cfc3231fc1a8b718a62863730a2702bb76366116bed09e0fd"+ "d4c860b7074be894fac9697399be5cc1");

            ISymmetricEncryptionAlgorithm encryptor = new BouncyCastleXChaCha20();
            byte[] cipher = encryptor.Encrypt(message, key, nonce);
            Assert.IsTrue(expectedCipher.SequenceEqual(cipher));
        }

        [TestMethod]
        public void EnsureBackwardsCompatibilityLongTimeDecryptionOfAesGcm()
        {
            // Ensure that a once stored cipher can always be decrypted even after changes in the liberary
            string base64Cipher = "dW5pdHRlc3QkYWVzX2djbSQ0NG04QXBFU1ptcXhnYll2OE5wcWl3PT0kcGJrZGYyJGgwSDdxSGZnVFlXNzBKS3lEb0JLeFE9PSQxMDAwJJsMDjdYEYXYmcqTOFRbge6iVfWo/iny4nrIOMVuoqYak6xB/MAe53G5H3AyxiTi8OENJbi9tzZStpe3p3nlDB7l+J8=";
            byte[] cipher = CryptoUtils.Base64StringToBytes(base64Cipher);
            ICryptor decryptor = new Cryptor("unittest", null);
            string decryptedMessage = CryptoUtils.BytesToString(decryptor.Decrypt(cipher, CryptoUtils.StringToSecureString("brownie")));
            Assert.AreEqual("The brown fox jumps over the lazy 🐢🖐🏿 doc.", decryptedMessage);
        }

        [TestMethod]
        public void EnsureBackwardsCompatibilityLongTimeDecryptionOfTwofishGcm()
        {
            // Ensure that a once stored cipher can always be decrypted even after changes in the liberary
            string base64Cipher = "dW5pdHRlc3QkdHdvZmlzaF9nY20kZHhMWFh4K0UrZ2MzWHdWc01rWUFxQT09JHBia2RmMiRma1BCWTdDWXp1OG5YUlJtYk9DUlp3PT0kMTAwMCRRc0ETSqDekQuBgKJ5x4Mvy02OHsivm0uJ9KchKdpGk+pmbF4Kq/EDbx9Uw54uEZUQLnK70dNKSEVtb1GyUOX1mitr";
            byte[] cipher = CryptoUtils.Base64StringToBytes(base64Cipher);
            ICryptor decryptor = new Cryptor("unittest", null);
            string decryptedMessage = CryptoUtils.BytesToString(decryptor.Decrypt(cipher, CryptoUtils.StringToSecureString("brownie")));
            Assert.AreEqual("The brown fox jumps over the lazy 🐢🖐🏿 doc.", decryptedMessage);
        }

        [TestMethod]
        public void EnsureBackwardsCompatibilityLongTimeDecryptionOfXChaCha()
        {
            // Ensure that a once stored cipher can always be decrypted even after changes in the liberary
            string base64Cipher = "dW5pdHRlc3Qgdj0yJHhjaGFjaGEyMF9wb2x5MTMwNSRsT0FJVW5wZXEyL0g3Ti96UkdCSktqaW9MdmFTUWk5eCRwYmtkZjIkdmN0dmpXVmx1NEhGbGhqbTV0SGlWQT09JDEwMDAwJCTcCqQglE3Xmfe0lg9AOhzxJXOuj7wEj+kgaSnKlZgnMyQwpQCwa9W57jnz1RhrwUuLh0X3PJpPbf7lR07Le7TFRZc8";
            byte[] cipher = CryptoUtils.Base64StringToBytes(base64Cipher);
            ICryptor decryptor = new Cryptor("unittest", null);
            string decryptedMessage = CryptoUtils.BytesToString(decryptor.Decrypt(cipher, CryptoUtils.StringToSecureString("brownie")));
            Assert.AreEqual("The brown fox jumps over the lazy 🐢🖐🏿 doc.", decryptedMessage);
        }

        [TestMethod]
        public void EnsureBackwardsCompatibilityLongTimeDecryptionOfPbkdf2()
        {
            const int KeyLength = 42;

            // Ensure that a once stored kdf can always be reproduced even after changes in the liberary
            IKeyDerivationFunction kdf = new Pbkdf2();
            SecureString password = CryptoUtils.StringToSecureString("The fox jumps");
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService(88); // Fixed nonce
            byte[] salt = randomGenerator.GetRandomBytes(kdf.ExpectedSaltSizeBytes);
            int cost = 100;
            byte[] kdfBytes = kdf.DeriveKeyFromPassword(password, KeyLength, salt, cost.ToString());
            string kdfText = CryptoUtils.BytesToBase64String(kdfBytes);
            Assert.AreEqual("3zaJwPYv0SIG2nVWghO556T1MlAzFDhWYrfGRmXE3iQv0ZmC3UrAJSd7", kdfText);
        }

        [TestMethod]
        public void EnsureBackwardsCompatibilityLongTimeDecryptionOfArgon2()
        {
            const int KeyLength = 42;

            // Ensure that a once stored kdf can always be reproduced even after changes in the liberary
            IKeyDerivationFunction kdf = new BouncyCastleArgon2();
            SecureString password = CryptoUtils.StringToSecureString("The fox jumps");
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService(88); // Fixed nonce
            byte[] salt = randomGenerator.GetRandomBytes(kdf.ExpectedSaltSizeBytes);
            var cost = new Argon2Cost
            {
                MemoryKib = 1000,
                Iterations = 1,
                Parallelism = 1,
            };
            byte[] kdfBytes = kdf.DeriveKeyFromPassword(password, KeyLength, salt, cost.Format());
            string kdfText = CryptoUtils.BytesToBase64String(kdfBytes);
            Assert.AreEqual("PalgR+kJyUk2ASW7rAYn5O85OrW2C9u51YpVWAB9CtYt7dY8tHPOg0aG", kdfText);
        }

        [TestMethod]
        public void EnsureCompatibilityToLibsodiumXChaCha20()
        {
            // Ensure that a once stored cipher can always be decrypted even after changes in the liberary.
            // This cypher was created with libsodium.
            string base64Cipher = "q8w/Zotsi3ZJp2eaAnFmMiAGgko+N2TkNgunS5+bDRNqrBBoN+XMQPSt7ojO4ODMP3Rf3aoYMNeJFL2/ZqK3AngIuQ==";
            byte[] cipher = CryptoUtils.Base64StringToBytes(base64Cipher);

            ISymmetricEncryptionAlgorithm decryptor = new BouncyCastleXChaCha20();
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService(88);
            byte[] key = randomGenerator.GetRandomBytes(decryptor.ExpectedKeySize);
            byte[] nonce = randomGenerator.GetRandomBytes(decryptor.ExpectedNonceSize);

            string decryptedMessage = CryptoUtils.BytesToString(decryptor.Decrypt(cipher, key, nonce));
            Assert.AreEqual("The brown fox jumps over the lazy 🐢🖐🏿 doc.", decryptedMessage);
        }

        [TestMethod]
        public void EnsureCompatibilityToLibsodiumArgon2()
        {
            // Code for LibSodium:
            // Span<byte> key = stackalloc byte[32];
            // CryptoPasswordHashArgon.DeriveKey(key, binaryPassword, salt, 2, 1000 * 1024);
            // kdfText = Convert.ToBase64String(key1);

            // Ensure that a once stored kdf can always be reproduced even after changes in the liberary
            IKeyDerivationFunction kdf = new BouncyCastleArgon2();
            SecureString password = CryptoUtils.StringToSecureString("The brown fox jumps over the lazy 🐢🖐🏿 dog.");
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService(8); // Fixed nonce
            byte[] salt = randomGenerator.GetRandomBytes(16);
            var cost = new Argon2Cost
            {
                MemoryKib = 1000,
                Iterations = 2,
                Parallelism = 1,
            };
            byte[] kdfBytes = kdf.DeriveKeyFromPassword(password, 32, salt, cost.Format());
            string kdfText = CryptoUtils.BytesToBase64String(kdfBytes);

            // Compare against kdf created with LibSodium
            Assert.AreEqual("oXFlS2LgiWets5UH0qfCy7rlPv86y6BmZjVu4zXAbRM=", kdfText);
        }

        [TestMethod]
        public void CryptoPbkdf2()
        {
            const int KeyLength = 42;
            IKeyDerivationFunction kdf = new Pbkdf2();

            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService();
            byte[] salt = randomGenerator.GetRandomBytes(kdf.ExpectedSaltSizeBytes);
            string cost = kdf.RecommendedCost(KeyDerivationCostType.Low);

            SecureString password = CryptoUtils.StringToSecureString("Das ist kein gutes Passwort");
            byte[] key = kdf.DeriveKeyFromPassword(password, KeyLength, salt, cost);
            Assert.AreEqual(KeyLength, key.Length);

            // Same parameters must result in the same output
            byte[] key2 = kdf.DeriveKeyFromPassword(password, KeyLength, salt, cost);
            CollectionAssert.AreEqual(key, key2);
        }

        [TestMethod]
        public void CryptoArgon2()
        {
            const int KeyLength = 42;
            IKeyDerivationFunction kdf = new BouncyCastleArgon2();

            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService();
            byte[] salt = randomGenerator.GetRandomBytes(kdf.ExpectedSaltSizeBytes);
            string cost = kdf.RecommendedCost(KeyDerivationCostType.Low);

            SecureString password = CryptoUtils.StringToSecureString("Das ist kein gutes Passwort");
            byte[] key = kdf.DeriveKeyFromPassword(password, KeyLength, salt, cost);
            Assert.AreEqual(KeyLength, key.Length);

            // Same parameters must result in the same output
            byte[] key2 = kdf.DeriveKeyFromPassword(password, KeyLength, salt, cost);
            CollectionAssert.AreEqual(key, key2);
        }

        [TestMethod]
        public void CryptorWorksWithPassword()
        {
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService();
            ICryptor encryptor = new Cryptor("sugus", randomGenerator);
            string message = "Der schnelle Fuchs stolpert über den faulen Hund.";
            byte[] binaryMessage = CryptoUtils.StringToBytes(message);
            SecureString password = CryptoUtils.StringToSecureString("Der schnelle Uhu fliegt über den faulen Hund.");

            byte[] cipher = encryptor.Encrypt(binaryMessage, password, KeyDerivationCostType.Low, BouncyCastleTwofishGcm.CryptoAlgorithmName, Pbkdf2.CryptoKdfName);
            byte[] decryptedMessage = encryptor.Decrypt(cipher, password);
            CollectionAssert.AreEqual(binaryMessage, decryptedMessage);
        }

        [TestMethod]
        public void CryptorWorksWithKey()
        {
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService();
            ICryptor encryptor = new Cryptor("sugus", randomGenerator);
            string message = "Der schnelle Fuchs stolpert über den faulen Hund.";
            byte[] binaryMessage = CryptoUtils.StringToBytes(message);
            byte[] key = randomGenerator.GetRandomBytes(32);

            byte[] cipher = encryptor.Encrypt(binaryMessage, key, BouncyCastleTwofishGcm.CryptoAlgorithmName);
            byte[] decryptedMessage = encryptor.Decrypt(cipher, key);
            CollectionAssert.AreEqual(binaryMessage, decryptedMessage);
        }

        [TestMethod]
        public void ObfuscationCanBeReversed()
        {
            SecureString obfuscationKey = CryptoUtils.StringToSecureString("A very strong passphrase...");
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService();
            byte[] binaryMessage = randomGenerator.GetRandomBytes(100);

            var obfuscatedMessage = CryptoUtils.Obfuscate(binaryMessage, obfuscationKey, randomGenerator);
            var deobfuscatedMessage = CryptoUtils.Deobfuscate(obfuscatedMessage, obfuscationKey);
            CollectionAssert.AreEqual(binaryMessage, deobfuscatedMessage);

            string plaintextMessage = "welcome home";
            string obfuscatedMessageText = CryptoUtils.Obfuscate(plaintextMessage, obfuscationKey, randomGenerator);
            string deobfuscatedMessageText = CryptoUtils.Deobfuscate(obfuscatedMessageText, obfuscationKey);
            Assert.AreEqual(plaintextMessage, deobfuscatedMessageText);
        }

        [TestMethod]
        public void EnsureBackwardsCompatibilityDeobfuscation()
        {
            // Ensure that a once stored obfuscated text can always be deobfuscated even after changes in the library
            SecureString obfuscationKey = CryptoUtils.StringToSecureString("A very strong passphrase...");
            string obfuscatedMessage = "b2JmdXNjYXRpb24kdHdvZmlzaF9nY20kU1pmWWpzWWV6MUZ0S0xqZWhHM1FCUT09JHBia2RmMiR0emVXNU9PTWNucEkxaHhWbkt2Y0Z3PT0kMTAwMCQh9UPVY34fufBoywrqb0JjKU/BMqnTABoXfaTsmEudBRVMpMb+Yx+GZIBjNbrWpqMSmWMgiIwfNBlixP0vi7ohAiv9";
            string deobfuscatedMessage = CryptoUtils.Deobfuscate(obfuscatedMessage, obfuscationKey);
            Assert.AreEqual("The brown fox jumps over the lazy 🐢🖐🏿 doc.", deobfuscatedMessage);
        }

        [TestMethod]
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
            CollectionAssert.AreEqual(binaryMessage, decryptedMessage);
        }

        [TestMethod]
        public void TestSymmetricEncryptionOfEmptyStringWithCompression()
        {
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService(88);
            ICryptor encryptor = new Cryptor("sugus", randomGenerator);
            byte[] binaryMessage = CryptoUtils.StringToBytes(string.Empty);
            SecureString password = CryptoUtils.StringToSecureString("Der schnelle Uhu fliegt über den faulen Hund.");

            byte[] cipher = encryptor.Encrypt(
                binaryMessage,
                password,
                KeyDerivationCostType.Low,
                BouncyCastleTwofishGcm.CryptoAlgorithmName,
                Pbkdf2.CryptoKdfName,
                Cryptor.CompressionGzip);
            byte[] decryptedMessage = encryptor.Decrypt(cipher, password);
            CollectionAssert.AreEqual(binaryMessage, decryptedMessage);
        }

        [TestMethod]
        public void UnknownAlgorithmThrows()
        {
            ICryptoRandomService randomGenerator = CommonMocksAndStubs.CryptoRandomService();
            ICryptor encryptor = new Cryptor("sugus", randomGenerator);
            byte[] binaryMessage = new byte[] { 88 };
            SecureString password = CryptoUtils.StringToSecureString("unittestpwd");

            Assert.ThrowsException<CryptoException>(delegate
            {
                byte[] cipher = encryptor.Encrypt(
                    binaryMessage,
                    password,
                    KeyDerivationCostType.Low,
                    "InvalidAlgorithmName",
                    Pbkdf2.CryptoKdfName);
            });

            Assert.ThrowsException<CryptoException>(delegate
            {
                byte[] cipher = encryptor.Encrypt(
                    binaryMessage,
                    password,
                    KeyDerivationCostType.Low,
                    BouncyCastleXChaCha20.CryptoAlgorithmName,
                    "InvalidKdfName");
            });
        }

        private static byte[] HexToBytes(string hex)
        {
            var hexAsBytes = new byte[hex.Length / 2];

            for (var i = 0; i < hex.Length; i += 2)
            {
                hexAsBytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }

            return hexAsBytes;
        }
    }
}
