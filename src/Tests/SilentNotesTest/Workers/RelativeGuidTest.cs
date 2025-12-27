using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestClass]
    public class RelativeGuidTest
    {
        [TestMethod]
        public void CreateRelativeGuid_WorksWithPositiveAndNegativeShift()
        {
            Guid originalGuid = new Guid("6a541929-b9a8-4704-b95c-89584f431343");

            Guid positiveRelativeGuid = RelativeGuid.CreateRelativeGuid(originalGuid, 2);
            Assert.AreEqual(new Guid("6a541929-b9a8-4704-b95c-89584f431345"), positiveRelativeGuid);

            Guid negativeRelativeGuid = RelativeGuid.CreateRelativeGuid(originalGuid, -2);
            Assert.AreEqual(new Guid("6a541929-b9a8-4704-b95c-89584f431341"), negativeRelativeGuid);
        }

        [TestMethod]
        public void CreateRelativeGuid_WorksWithOverflow()
        {
            Guid originalGuid = new Guid("6a541929-b9a8-4704-b95c-0000fffffffe");
            Guid positiveRelativeGuid = RelativeGuid.CreateRelativeGuid(originalGuid, 3);
            Assert.AreEqual(new Guid("6a541929-b9a8-4704-b95c-000000000001"), positiveRelativeGuid);

            originalGuid = new Guid("6a541929-b9a8-4704-b95c-000000000001");
            Guid negativeRelativeGuid = RelativeGuid.CreateRelativeGuid(originalGuid, -3);
            Assert.AreEqual(new Guid("6a541929-b9a8-4704-b95c-0000fffffffe"), negativeRelativeGuid);
        }

        [TestMethod]
        public void CompareRelativeGuids_WorksWithPositiveAndNegativeDistance()
        {
            Guid originalGuid = new Guid("6a541929-b9a8-4704-b95c-89584f431343");

            Guid positiveRelativeGuid = new Guid("6a541929-b9a8-4704-b95c-89584f431345");
            int positiveShift = RelativeGuid.CompareRelativeGuids(positiveRelativeGuid, originalGuid);
            Assert.AreEqual(2, positiveShift);

            Guid negativeRelativeGuid = new Guid("6a541929-b9a8-4704-b95c-89584f431341");
            int negativeShift = RelativeGuid.CompareRelativeGuids(negativeRelativeGuid, originalGuid);
            Assert.AreEqual(-2, negativeShift);
        }

        [TestMethod]
        public void CompareRelativeGuids_WorksWithOverflow()
        {
            Guid originalGuid = new Guid("6a541929-b9a8-4704-b95c-0000fffffffe");

            Guid positiveRelativeGuid = new Guid("6a541929-b9a8-4704-b95c-000000000001");
            int positiveShift = RelativeGuid.CompareRelativeGuids(positiveRelativeGuid, originalGuid);
            Assert.AreEqual(3, positiveShift);

            originalGuid = new Guid("6a541929-b9a8-4704-b95c-000000000001");
            Guid negativeRelativeGuid = new Guid("6a541929-b9a8-4704-b95c-0000fffffffe");
            int negativeShift = RelativeGuid.CompareRelativeGuids(negativeRelativeGuid, originalGuid);
            Assert.AreEqual(-3, negativeShift);
        }

        [TestMethod]
        public void AreGuidsRelated_ChecksAllButTheLastBytes()
        {
            Guid originalGuid;
            Guid relatedGuid;

            // Only the last 8 hex characters differ in related Guids
            originalGuid = new Guid("6a541929-b9a8-4704-b95c-89584f431343");
            relatedGuid = new Guid("6a541929-b9a8-4704-b95c-89584f431344");
            Assert.IsTrue(RelativeGuid.AreGuidsRelated(originalGuid, relatedGuid));

            originalGuid = new Guid("6a541929-b9a8-4704-b95c-89584f431343");
            relatedGuid = new Guid("6a541929-b9a8-4704-b95c-8958af431343");
            Assert.IsTrue(RelativeGuid.AreGuidsRelated(originalGuid, relatedGuid));

            originalGuid = new Guid("6a541929-b9a8-4704-b95c-89584f431343");
            relatedGuid = new Guid("6a541929-b9a8-4704-b95c-895a4f431344");
            Assert.IsFalse(RelativeGuid.AreGuidsRelated(originalGuid, relatedGuid));

            originalGuid = new Guid("6a541929-b9a8-4704-b95c-89584f431343");
            relatedGuid = new Guid("1a541929-b9a8-4704-b95c-89584f431344");
            Assert.IsFalse(RelativeGuid.AreGuidsRelated(originalGuid, relatedGuid));
        }

        [TestMethod]
        public void CreateAndCompare_FuzzyTest()
        {
            Random random = new Random();
            for (int i = 0; i < 10000; i++)
            {
                Guid originalGuid = Guid.NewGuid();
                int shift = random.Next(int.MinValue, int.MaxValue);

                Guid relativeGuid = RelativeGuid.CreateRelativeGuid(originalGuid, shift);
                int comparedShift = RelativeGuid.CompareRelativeGuids(relativeGuid, originalGuid);
                Assert.AreEqual(shift, comparedShift);
            }
        }
    }
}
