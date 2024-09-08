using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Models;

namespace SilentNotesTest.Models
{
    [TestClass]
    public class SettingsModelTest
    {
        [TestMethod]
        public void NewModelHasRevisionSet()
        {
            SettingsModel model = new SettingsModel();
            Assert.AreEqual(SettingsModel.NewestSupportedRevision, model.Revision);
        }

        [TestMethod]
        public void SettingTransferCodeAddsCurrentCodeToHistory()
        {
            SettingsModel model = new SettingsModel();

            model.TransferCode = "a";
            Assert.AreEqual("a", model.TransferCode);
            Assert.AreEqual(0, model.TransferCodeHistory.Count);

            model.TransferCode = "b";
            Assert.AreEqual("b", model.TransferCode);
            Assert.AreEqual(1, model.TransferCodeHistory.Count);
            Assert.AreEqual("a", model.TransferCodeHistory[0]);

            model.TransferCode = "c";
            Assert.AreEqual("c", model.TransferCode);
            Assert.AreEqual(2, model.TransferCodeHistory.Count);
            Assert.AreEqual("b", model.TransferCodeHistory[0]);
            Assert.AreEqual("a", model.TransferCodeHistory[1]);
        }

        [TestMethod]
        public void SettingTransferCodeFromHistoryRemovesHistoryEntry()
        {
            SettingsModel model = new SettingsModel();

            model.TransferCode = "a";
            model.TransferCodeHistory.Add("y");
            model.TransferCodeHistory.Add("z");

            model.TransferCode = "y";
            Assert.AreEqual("y", model.TransferCode);
            Assert.AreEqual(2, model.TransferCodeHistory.Count);
            Assert.AreEqual("a", model.TransferCodeHistory[0]);
            Assert.AreEqual("z", model.TransferCodeHistory[1]);
        }
    }
}
