using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SilentNotes.Crypto;
using SilentNotes.Crypto.KeyDerivation;
using SilentNotes.Crypto.SymmetricEncryption;

namespace KdfTest
{
    internal class MainViewModel
    {
        private const int ProfilingRounds = 5;
        private readonly Pbkdf2 _pbkdf2;
        private readonly BouncyCastleArgon2 _argon2;
        private readonly Random _nonCryptoRandom;

        public MainViewModel()
        {
            _pbkdf2 = new Pbkdf2();
            _argon2 = new BouncyCastleArgon2();
            _nonCryptoRandom = new Random();

            MeasurePbkdf2Command = new RelayCommand(MeasurePbkdf2);
            MeasureArgon2Command = new RelayCommand(MeasureArgon2);
            Pbkdf2CostFactor = _pbkdf2.RecommendedCost(KeyDerivationCostType.High);
            Argon2CostFactor = _argon2.RecommendedCost(KeyDerivationCostType.High);
        }

        /// <summary>
        /// Gets of sets the cost factor for PBKDF2
        /// </summary>
        public string Pbkdf2CostFactor { get; set; }

        /// <summary>
        /// Gets the measured time for PBKDF2
        /// </summary>
        public int Pbkdf2Time { get; private set; }

        public ICommand MeasurePbkdf2Command { get; }

        private void MeasurePbkdf2()
        {
            var password = CryptoUtils.StringToSecureString("The fox jumps over the lazy dog.");
            var expectedKeySize = new BouncyCastleXChaCha20().ExpectedKeySize;
            var salt = GetNonCryptoRandomBytes(_pbkdf2.ExpectedSaltSizeBytes);
            string cost = Pbkdf2CostFactor;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int index = 0; index < ProfilingRounds; index++)
            {
                _pbkdf2.DeriveKeyFromPassword(password, expectedKeySize, salt, cost);
            }
            stopwatch.Stop();

            int measuredTime = (int)stopwatch.ElapsedMilliseconds / ProfilingRounds;
            Pbkdf2Time = measuredTime;
        }

        /// <summary>
        /// Gets of sets the cost factor for Argon2
        /// </summary>
        public string Argon2CostFactor { get; set; }

        /// <summary>
        /// Gets the measured time for Argon2
        /// </summary>
        public int Argon2Time { get; private set; }

        public ICommand MeasureArgon2Command { get; }

        private void MeasureArgon2()
        {
            var password = CryptoUtils.StringToSecureString("The fox jumps over the lazy dog.");
            var expectedKeySize = new BouncyCastleXChaCha20().ExpectedKeySize;
            var salt = GetNonCryptoRandomBytes(_argon2.ExpectedSaltSizeBytes);
            string cost = Argon2CostFactor;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int index = 0; index < ProfilingRounds; index++)
            {
                _argon2.DeriveKeyFromPassword(password, expectedKeySize, salt, cost);
            }
            stopwatch.Stop();

            int measuredTime = (int)stopwatch.ElapsedMilliseconds / ProfilingRounds;
            Argon2Time = measuredTime;
        }

        private byte[] GetNonCryptoRandomBytes(int numberOfBytes)
        {
            byte[] result = new byte[numberOfBytes];
            _nonCryptoRandom.NextBytes(result);
            return result;
        }
    }
}
