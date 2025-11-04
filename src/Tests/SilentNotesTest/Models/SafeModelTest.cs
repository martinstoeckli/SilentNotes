using System;
using System.Linq;
using System.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Crypto;
using SilentNotes.Crypto.KeyDerivation;
using SilentNotes.Crypto.SymmetricEncryption;
using SilentNotes.Models;

namespace SilentNotesTest.Models
{
    [TestClass]
    public class SafeModelTest
    {
        [TestMethod]
        public void CloneCopiesAllProperties()
        {
            SafeModel note1 = new SafeModel
            {
                Id = Guid.NewGuid(),
                CreatedAt = new DateTime(2000, 10, 22, 18, 55, 30),
                ModifiedAt = new DateTime(2001, 10, 22, 18, 55, 30),
                MaintainedAt = new DateTime(2002, 10, 22, 18, 55, 30),
                SerializeableKey = "sugus",
            };
            SafeModel note2 = note1.Clone();

            Assert.AreEqual(note1.Id, note2.Id);
            Assert.AreEqual(note1.CreatedAt, note2.CreatedAt);
            Assert.AreEqual(note1.ModifiedAt, note2.ModifiedAt);
            Assert.AreEqual(note1.MaintainedAt, note2.MaintainedAt);
            Assert.AreEqual(note1.SerializeableKey, note2.SerializeableKey);
        }

        [TestMethod]
        public void EncryptedKeyCanBeDecrypted()
        {
            byte[] key = new byte[] { 88, 99, 11 };
            SecureString password = CryptoUtils.StringToSecureString("testpassword");
            ICryptoRandomSource randomSource = CommonMocksAndStubs.CryptoRandomService();

            string encryptedKey = SafeModel.EncryptKey(key, password, randomSource, BouncyCastleAesGcm.CryptoAlgorithmName, Pbkdf2.CryptoKdfName);
            bool res = SafeModel.TryDecryptKey(encryptedKey, password, out byte[] decryptedKey, out bool needsReEncryption);

            Assert.IsTrue(res);
            Assert.IsTrue(key.SequenceEqual(decryptedKey));
            Assert.IsFalse(needsReEncryption);
        }

        [TestMethod]
        public void EnsureBackwardsCompatibilityDecryption()
        {
            string encryptedKey = "U2lsZW50U2FmZSB2PTIkYWVzX2djbSQ2ZXVQN3NSQ2dHQStadTJGeE80QkJRPT0kcGJrZGYyJEs0TjY5dmllRTBvaEg1UlVvVDUydGc9PSQxMDAwMCQkT1cFrC+EM9E5PlM4uPGUv0HsOQ==";
            SecureString password = CryptoUtils.StringToSecureString("testpassword");
            bool res = SafeModel.TryDecryptKey(encryptedKey, password, out byte[] decryptedKey, out _);

            byte[] originalKey = new byte[] { 88, 99, 11 };
            Assert.IsTrue(res);
            Assert.IsTrue(originalKey.SequenceEqual(decryptedKey));
        }
    }
}
