using NUnit.Framework;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestFixture]
    public class FloatingPointUtilsTest
    {
        [Test]
        public void FormatInvariantIgnoresCurrentCulture()
        {
            double value = 8.88888888;
            string cultureValue;
            string invariantValue;

            // Switch to culture de with a comma as decimal separator
            using (var cultureSwitcher = new CultureSwitcher4UnitTest("de-De"))
            {
                cultureValue = value.ToString("0.###");
                invariantValue = FloatingPointUtils.FormatInvariant(value);
            }
            Assert.AreEqual("8,889", cultureValue);
            Assert.AreEqual("8.889", invariantValue); // formatted with decimal point
        }
    }
}
