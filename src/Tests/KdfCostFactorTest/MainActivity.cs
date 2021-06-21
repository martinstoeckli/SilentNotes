using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using AndroidX.AppCompat.App;

namespace KdfCostFactorTest
{
    /// <summary>
    /// The purpose of this app is to measure the time a key-derivation-function (KDF) needs, so
    /// that its parameters can be adjusted.
    /// Measure the time only on real devices (not emulators) and always keep the "release" mode
    /// when compiling.
    /// </summary>
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private const int KeySizeBytes = 32; // 256 bits
        private const int SaltSizeBytes = 16; // 128 bits

        protected override void OnCreate(Bundle savedInstanceState)
        {
            // The cost factor should be set so the function needs as much time as the application
            // can bear. The more time it requires the more safe from brute-forcing we get, though
            // we have to consider mobile devices with less calculation power.
            int costFactor = 10000;

            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            // Do a first calculation and do not use it for measurement.
            byte[] salt = GetRandomBytes(SaltSizeBytes);
            byte[] key = DeriveKeyFromPassword("How long will it take to generate a key?", KeySizeBytes, salt, costFactor);

            // This time we mesaure the required time.
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            key = DeriveKeyFromPassword("How long will it take to generate a key?", KeySizeBytes, salt, costFactor);
            stopWatch.Stop();
            long time = stopWatch.ElapsedMilliseconds;

            TextView textView = FindViewById(Resource.Id.time) as TextView;
            textView.Text = string.Format("The measured time for cost factor {0} is {1}ms", costFactor, time);
        }

        /// <summary>
        /// The candidate to test: PBKDF2
        /// </summary>
        private byte[] DeriveKeyFromPassword(string password, int expectedKeySizeBytes, byte[] salt, int cost)
        {
            byte[] binaryPassword = Encoding.UTF8.GetBytes(password);
            Rfc2898DeriveBytes kdf = new Rfc2898DeriveBytes(binaryPassword, salt, cost);
            byte[] result = kdf.GetBytes(expectedKeySizeBytes);
            return result;
        }

        private byte[] GetRandomBytes(int numberOfBytes)
        {
            byte[] result = new byte[numberOfBytes];
            Random nonCryptoRandom = new Random();
            nonCryptoRandom.NextBytes(result);
            return result;
        }
    }
}