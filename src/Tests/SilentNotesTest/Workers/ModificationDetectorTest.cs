using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestClass]
    public class ModificationDetectorTest
    {
        [TestMethod]
        public void IsModified_DetectsChange()
        {
            var values = new long[] { 11, 88 };
            ModificationDetector detector = new ModificationDetector(() => ModificationDetector.CombineHashCodes(values));

            Assert.IsFalse(detector.IsModified());
            values[0] = 22;
            Assert.IsTrue(detector.IsModified());
        }

        [TestMethod]
        public void MemorizeCurrentState_ResetsFingerprint()
        {
            var values = new long[] { 11, 88 };
            ModificationDetector detector = new ModificationDetector(() => ModificationDetector.CombineHashCodes(values));
            values[0] = 22;
            Assert.IsTrue(detector.IsModified());
            detector.MemorizeCurrentState();
            Assert.IsFalse(detector.IsModified());
        }

        [TestMethod]
        public void CombineHashCodes_CalculatesSameCode_ForSameValues()
        {
            long hashCode1 = ModificationDetector.CombineHashCodes(new long[] { 11, 88, 99 });
            long hashCode2 = ModificationDetector.CombineHashCodes(new long[] { 11, 88, 99 });
            Assert.AreEqual(hashCode1, hashCode2);
        }

        [TestMethod]
        public void CombineHashCodes_CalculatesDifferentCodes_ForDifferentOrder()
        {
            long hashCode1 = ModificationDetector.CombineHashCodes(new long[] { 11, 88, 99 });
            long hashCode2 = ModificationDetector.CombineHashCodes(new long[] { 11, 99, 88 });
            Assert.AreNotEqual(hashCode1, hashCode2);
        }

        [TestMethod]
        public void CombineHashCodes_WorksWithEmptyList()
        {
            long hashCode1 = ModificationDetector.CombineHashCodes(new long[0]);
            Assert.AreEqual(0, hashCode1);
        }

        [TestMethod]
        public void CombineHashCodes_DoesNotOverflow()
        {
            ModificationDetector.CombineHashCodes(new long[] { long.MaxValue, long.MaxValue });
        }

        [TestMethod]
        public void CombineHashCodes_ChainingUsesOriginalHashCode()
        {
            long originalHashCode = ModificationDetector.CombineHashCodes(new long[] { 11, 88, 99 });
            long hashCode = ModificationDetector.CombineHashCodes(new long[] { 77 }, originalHashCode);
            Assert.AreNotEqual(originalHashCode, hashCode);
        }

        [TestMethod]
        public void CombineWithStringHash_UsesSha()
        {
            long hashCode = ModificationDetector.CombineWithStringHash("the lazy fox", 0);
            Assert.AreEqual(4208979722577018513, hashCode); // This hash is reproduceable
        }

        [TestMethod]

        public void CombineWithStringHash_WorksWithNull()
        {
            long hashCode = ModificationDetector.CombineWithStringHash(null, 88);
            Assert.AreEqual(88, hashCode); // orignal hash unchanged

            // Empty string is not the same as null
            hashCode = ModificationDetector.CombineWithStringHash(string.Empty, 0);
            Assert.AreEqual(655463537771531689, hashCode); // No crash, reproduceable hash
        }
    }
}
