using System;
using System.Diagnostics;
using System.Security;
using NUnit.Framework;
using SilentNotes.Crypto;
using SilentNotes.Crypto.KeyDerivation;

namespace SilentNotesTest.Crypto
{
    [TestFixture]
    public class Pbkdf2Test
    {
        /// <summary>
        /// Result 2.12.2019 compiled in release mode: 10000 iterations = 103ms
        /// </summary>
        [Test]
        public void MeasureTime()
        {
            IKeyDerivationFunction kdf = new Pbkdf2();
            ICryptoRandomSource randomSource = CommonMocksAndStubs.CryptoRandomService();
            SecureString password = CryptoUtils.StringToSecureString("candidate");
            byte[] salt = randomSource.GetRandomBytes(kdf.ExpectedSaltSizeBytes);
            int iterations = 10000;

            Stopwatch watch = new Stopwatch();
            watch.Start();
            kdf.DeriveKeyFromPassword(password, 32, salt, iterations);
            watch.Stop();
            Console.WriteLine("Pbkdf2 time for {0} iterations: {1}ms", iterations, watch.ElapsedMilliseconds);
        }
    }
}
