using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestClass]
    public class ColorExtensionsTest
    {
        [TestMethod]
        public void IsDarkWorksCorrectly()
        {
            Assert.IsTrue(Color.Black.IsDark());
            Assert.IsTrue(Color.DarkBlue.IsDark());
            Assert.IsFalse(Color.White.IsDark());
            Assert.IsFalse(Color.Yellow.IsDark());

            // Edge case gray
            Color gray = Color.FromArgb(128, 128, 128);
            Assert.IsFalse(gray.IsDark());
            gray = Color.FromArgb(127, 127, 127);
            Assert.IsTrue(gray.IsDark());
        }

        [TestMethod]
        public void HexToColorFindsColor()
        {
            Assert.IsTrue(AreColorsEqual(Color.Red, ColorExtensions.HexToColor("#ff0000")));
            Assert.IsTrue(AreColorsEqual(Color.White, ColorExtensions.HexToColor("ffffff")));
            Assert.IsTrue(AreColorsEqual(Color.FromArgb(160, 176, 192), ColorExtensions.HexToColor("#a0b0c0")));
        }

        [TestMethod]
        public void HexToColorFindsColorWithAlpha()
        {
            Assert.IsTrue(AreColorsEqual(Color.Red, ColorExtensions.HexToColor("#ffff0000")));
            Assert.IsTrue(AreColorsEqual(Color.White, ColorExtensions.HexToColor("#ffffffff")));
            Assert.IsTrue(AreColorsEqual(Color.FromArgb(128, 160, 176, 192), ColorExtensions.HexToColor("#80a0b0c0")));
        }

        /// <summary>
        /// We want to compare the color value only, the Color.Equals() operator will also check
        /// how the colors where generated though (colors with known names).
        /// </summary>
        /// <param name="color1">First color to compare.</param>
        /// <param name="color2">Second color to compare.</param>
        /// <returns>Returns true if the colors are equal.</returns>
        private static bool AreColorsEqual(Color color1, Color color2)
        {
            return color1.ToArgb() == color2.ToArgb();
        }
    }
}
