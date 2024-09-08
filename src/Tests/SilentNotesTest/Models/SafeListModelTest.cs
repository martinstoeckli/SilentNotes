using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Models;
using SilentNotes.Services;

namespace SilentNotesTest.Models
{
    [TestClass]
    public class SafeListModelTest
    {
        [TestMethod]
        public void FindOldestOpenSafeWorksCorrectly()
        {
            SafeListModel list = new SafeListModel();
            var keyService = CommonMocksAndStubs.SafeKeyService();
            byte[] testKey = new byte[0];

            // Search in empty list
            Assert.IsNull(list.FindOldestOpenSafe(keyService));

            // Search with only one element
            SafeModel safe2002 = new SafeModel { CreatedAt = new DateTime(2002, 02, 02) };
            list.Add(safe2002);
            keyService.AddKey(safe2002.Id, testKey);
            Assert.AreSame(safe2002, list.FindOldestOpenSafe(keyService));

            // Add newer element
            SafeModel safe2003 = new SafeModel { CreatedAt = new DateTime(2003, 03, 03) };
            list.Add(safe2003);
            keyService.AddKey(safe2003.Id, testKey);
            Assert.AreSame(safe2002, list.FindOldestOpenSafe(keyService));

            // Add closed safe
            SafeModel safe2000 = new SafeModel { CreatedAt = new DateTime(2000, 01, 01) };
            list.Add(safe2000);
            Assert.AreSame(safe2002, list.FindOldestOpenSafe(keyService));

            // Add older element
            SafeModel safe2001 = new SafeModel { CreatedAt = new DateTime(2001, 01, 01) };
            list.Add(safe2001);
            keyService.AddKey(safe2001.Id, testKey);
            Assert.AreSame(safe2001, list.FindOldestOpenSafe(keyService));
        }
    }
}
