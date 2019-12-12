using System;
using NUnit.Framework;
using SilentNotes.Models;

namespace SilentNotesTest.Models
{
    [TestFixture]
    public class SafeListModelTest
    {
        [Test]
        public void FindOldestOpenSafeWorksCorrectly()
        {
            SafeListModel list = new SafeListModel();
            byte[] testKey = new byte[0];

            // Search in empty list
            Assert.IsNull(list.FindOldestOpenSafe());

            // Search with only one element
            SafeModel safe2002 = new SafeModel { CreatedAt = new DateTime(2002, 02, 02), Key = testKey };
            list.Add(safe2002);
            Assert.AreSame(safe2002, list.FindOldestOpenSafe());

            // Add newer element
            SafeModel safe2003 = new SafeModel { CreatedAt = new DateTime(2003, 03, 03), Key = testKey };
            list.Add(safe2003);
            Assert.AreSame(safe2002, list.FindOldestOpenSafe());

            // Add closed safe
            SafeModel safe2000 = new SafeModel { CreatedAt = new DateTime(2000, 01, 01), Key = null };
            list.Add(safe2000);
            Assert.AreSame(safe2002, list.FindOldestOpenSafe());

            // Add older element
            SafeModel safe2001 = new SafeModel { CreatedAt = new DateTime(2001, 01, 01), Key = testKey };
            list.Add(safe2001);
            Assert.AreSame(safe2001, list.FindOldestOpenSafe());
        }
    }
}
