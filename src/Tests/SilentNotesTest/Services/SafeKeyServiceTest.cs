using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Crypto;
using SilentNotes.Crypto.KeyDerivation;
using SilentNotes.Crypto.SymmetricEncryption;
using SilentNotes.Models;
using SilentNotes.Services;

namespace SilentNotesTest.Services
{
    [TestClass]
    public class SafeKeyServiceTest
    {
        [TestMethod]
        public void TryOpenSafe_CanOpenSafe()
        {
            SecureString pwd = CryptoUtils.StringToSecureString("mysecret");
            ICryptoRandomService randomService = CommonMocksAndStubs.CryptoRandomService();

            var safe = new SafeModel();
            var key = randomService.GetRandomBytes(32);
            safe.SerializeableKey = SafeModel.EncryptKey(key, pwd, randomService, BouncyCastleAesGcm.CryptoAlgorithmName, Pbkdf2.CryptoKdfName);

            ISafeKeyService service = new SafeKeyService();
            bool res = service.TryOpenSafe(safe, pwd);

            Assert.IsTrue(res);
            service.TryGetKey(safe.Id, out byte[] safeKey);
            Assert.IsTrue(key.SequenceEqual(safeKey));
        }

        [TestMethod]
        public void TryOpenSafe_DoesntOpenSafeWithWrongPassword()
        {
            SecureString pwd = CryptoUtils.StringToSecureString("mysecret");
            ICryptoRandomService randomService = CommonMocksAndStubs.CryptoRandomService();

            var safe = new SafeModel();
            var key = randomService.GetRandomBytes(32);
            safe.SerializeableKey = SafeModel.EncryptKey(key, pwd, randomService, BouncyCastleAesGcm.CryptoAlgorithmName, Pbkdf2.CryptoKdfName);

            SecureString wrongPwd = CryptoUtils.StringToSecureString("invalid");
            ISafeKeyService service = new SafeKeyService();
            bool res = service.TryOpenSafe(safe, wrongPwd);

            Assert.IsFalse(res);
            Assert.IsFalse(service.TryGetKey(safe.Id, out byte[] safeKey));
            Assert.IsNull(safeKey);
        }

        [TestMethod]
        public void CloseSafe_CleansKey()
        {
            byte[] key = new byte[] { 8, 9, 3 };
            Guid safeId = Guid.NewGuid();
            var keyService = CommonMocksAndStubs.SafeKeyService()
                .AddKey(safeId, key);

            keyService.CloseSafe(safeId);
            Assert.IsFalse(keyService.IsSafeOpen(safeId));
            byte[] emptyKey = new byte[] { 0, 0, 0 };
            Assert.IsTrue(emptyKey.SequenceEqual(key));
        }

        [TestMethod]
        public void CloseAllSafes_RemovesKey()
        {
            byte[] key = new byte[] { 8, 9, 3 };
            Guid safeId1 = Guid.NewGuid();
            Guid safeId2 = Guid.NewGuid();
            var keyService = CommonMocksAndStubs.SafeKeyService()
                .AddKey(safeId1, new byte[] { 8, 9, 3 })
                .AddKey(safeId2, new byte[] { 7, 4, 3});

            keyService.CloseAllSafes();
            Assert.AreEqual(0, keyService.Count);
        }
    }
}
