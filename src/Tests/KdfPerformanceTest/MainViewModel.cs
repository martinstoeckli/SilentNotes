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
        private readonly Random _nonCryptoRandom;

        public MainViewModel()
        {
            MeasurePbkdf2Command = new RelayCommand(MeasurePbkdf2);
            _pbkdf2 = new Pbkdf2();
            _nonCryptoRandom = new Random();

            Pbkdf2CostFactor = _pbkdf2.RecommendedCost(KeyDerivationCostType.High);
        }

        /// <summary>
        /// Gets of sets the cost factor for PBKDF2
        /// </summary>
        public int Pbkdf2CostFactor { get; set; }

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
            int cost = Pbkdf2CostFactor;

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

        private byte[] GetNonCryptoRandomBytes(int numberOfBytes)
        {
            byte[] result = new byte[numberOfBytes];
            _nonCryptoRandom.NextBytes(result);
            return result;
        }
    }
}
