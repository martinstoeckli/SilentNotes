using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestFixture]
    public class ModificationDetectorTest
    {
        [Test]
        public void IsModified_DetectsChange()
        {
            var values = new long[] { 11, 88 };
            ModificationDetector detector = new ModificationDetector(() => ModificationDetector.CombineHashCodes(values));

            Assert.IsFalse(detector.IsModified());
            values[0] = 22;
            Assert.IsTrue(detector.IsModified());
        }

        [Test]
        public void MemorizeCurrentState_ResetsFingerprint()
        {
            var values = new long[] { 11, 88 };
            ModificationDetector detector = new ModificationDetector(() => ModificationDetector.CombineHashCodes(values));
            values[0] = 22;
            Assert.IsTrue(detector.IsModified());
            detector.MemorizeCurrentState();
            Assert.IsFalse(detector.IsModified());
        }

        [Test]
        public void CombineHashCodes_CalculatesSameCode_ForSameValues()
        {
            long hashCode1 = ModificationDetector.CombineHashCodes(new long[] { 11, 88, 99 });
            long hashCode2 = ModificationDetector.CombineHashCodes(new long[] { 11, 88, 99 });
            Assert.AreEqual(hashCode1, hashCode2);
        }

        [Test]
        public void CombineHashCodes_CalculatesDifferentCodes_ForDifferentOrder()
        {
            long hashCode1 = ModificationDetector.CombineHashCodes(new long[] { 11, 88, 99 });
            long hashCode2 = ModificationDetector.CombineHashCodes(new long[] { 11, 99, 88 });
            Assert.AreNotEqual(hashCode1, hashCode2);
        }

        [Test]
        public void CombineHashCodes_WorksWithEmptyList()
        {
            long hashCode1 = ModificationDetector.CombineHashCodes(new long[0]);
            Assert.AreEqual(0, hashCode1);
        }

        [Test]
        public void CombineHashCodes_DoesNotOverflow()
        {
            Assert.DoesNotThrow(() => ModificationDetector.CombineHashCodes(new long[] { long.MaxValue, long.MaxValue }));
        }

        [Test]
        public void CombineHashCodes_ChainingUsesOriginalHashCode()
        {
            long originalHashCode = ModificationDetector.CombineHashCodes(new long[] { 11, 88, 99 });
            long hashCode = ModificationDetector.CombineHashCodes(new long[] { 77 }, originalHashCode);
            Assert.AreNotEqual(originalHashCode, hashCode);
        }

        [Test]
        public void CombineWithStringHash_UsesSha()
        {
            long hashCode = ModificationDetector.CombineWithStringHash("the lazy fox", 0);
            Assert.AreEqual(4208979722577018513, hashCode); // This hash is reproduceable
        }

        [Test]

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
