using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Crypto.KeyDerivation;

namespace SilentNotesTest.Crypto
{
    [TestClass]
    public class Argon2CostTest
    {
        [TestMethod]
        public void FormatReturnsValidCostString()
        {
            Argon2Cost cost = new Argon2Cost
            {
                MemoryKib = 1024,
                Iterations = 3,
                Parallelism = 2
            };

            string costString = cost.Format();
            Assert.AreEqual("m=1024,t=3,p=2", costString);
        }

        [TestMethod]
        public void TryParseReadsCorrectCostString()
        {
            Argon2Cost cost;
            Assert.IsTrue(Argon2Cost.TryParse("m=1024,t=3,p=2", out cost));
            Assert.AreEqual(1024, cost.MemoryKib);
            Assert.AreEqual(3, cost.Iterations);
            Assert.AreEqual(2, cost.Parallelism);

            Assert.IsTrue(Argon2Cost.TryParse("t=3,p=2,m=1024", out cost)); // different order
            Assert.AreEqual(1024, cost.MemoryKib);
            Assert.AreEqual(3, cost.Iterations);
            Assert.AreEqual(2, cost.Parallelism);
        }

        [TestMethod]
        public void TryParseFailsWithInvalidCostString()
        {
            Argon2Cost cost;
            Assert.IsFalse(Argon2Cost.TryParse(null, out cost));
            Assert.IsFalse(Argon2Cost.TryParse("", out cost));
            Assert.IsFalse(Argon2Cost.TryParse("m=1024,t=3", out cost)); // missing part
            Assert.IsFalse(Argon2Cost.TryParse("m=1'024,t=3,p=2", out cost)); // invalid number format
            Assert.IsFalse(Argon2Cost.TryParse("m=1024;t=3;p=2", out cost)); // invalid delimiter
            Assert.IsFalse(Argon2Cost.TryParse("1024,3,2", out cost)); // no parameter keys
        }
    }
}
