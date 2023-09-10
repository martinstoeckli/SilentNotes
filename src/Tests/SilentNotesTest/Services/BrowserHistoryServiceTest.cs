using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SilentNotes.Services;

namespace SilentNotesTest.Services
{
    [TestFixture]
    public class BrowserHistoryServiceTest
    {
        [Test]
        public void GetRelativeUri_KeepsRelativeLocations()
        {
            var browserHistory = new BrowserHistoryService();
            Assert.AreEqual("/myway", browserHistory.GetRelativeUri("/myway", "https://0.0.0.0/"));
            Assert.AreEqual("/myway", browserHistory.GetRelativeUri("/myway", "https://0.0.0.0"));
            Assert.AreEqual("myway", browserHistory.GetRelativeUri("myway", "https://0.0.0.0/"));
            Assert.AreEqual("/myway", browserHistory.GetRelativeUri("/myway", string.Empty));
        }

        [Test]
        public void GetRelativeUri_KeepsLeadingSlash()
        {
            var browserHistory = new BrowserHistoryService();
            Assert.AreEqual("/myway", browserHistory.GetRelativeUri("https://0.0.0.0/myway", "https://0.0.0.0/"));
            Assert.AreEqual("/myway", browserHistory.GetRelativeUri("https://0.0.0.0/myway", "https://0.0.0.0"));
        }

        [Test]
        public void UpdateHistoryOnNavigation_AddsNewLocation()
        {
            var browserHistory = new BrowserHistoryService();
            Assert.IsNull(browserHistory.PreviousLocation);

            browserHistory.UpdateHistoryOnNavigation("/page1", "https://0.0.0.0/");
            Assert.AreEqual("/page1", browserHistory.CurrentLocation);
            Assert.IsNull(browserHistory.PreviousLocation);
            Assert.AreEqual(1, browserHistory.Count);

            browserHistory.UpdateHistoryOnNavigation("/page2", "https://0.0.0.0/");
            Assert.AreEqual("/page2", browserHistory.CurrentLocation);
            Assert.AreEqual("/page1", browserHistory.PreviousLocation);
            Assert.AreEqual(2, browserHistory.Count);
        }

        [Test]
        public void UpdateHistoryOnNavigation_RemovesLocation_WhenNavigatingBack()
        {
            var browserHistory = new BrowserHistoryService();
            browserHistory.UpdateHistoryOnNavigation("/page1", "https://0.0.0.0/");
            browserHistory.UpdateHistoryOnNavigation("/page2", "https://0.0.0.0/");

            browserHistory.UpdateHistoryOnNavigation("/page1", "https://0.0.0.0/");
            Assert.AreEqual(1, browserHistory.Count);
        }

        [Test]
        public void UpdateHistoryOnNavigation_KeepsLocation_WhenReloadsPage()
        {
            var browserHistory = new BrowserHistoryService();
            browserHistory.UpdateHistoryOnNavigation("/page1", "https://0.0.0.0/");
            browserHistory.UpdateHistoryOnNavigation("/page2", "https://0.0.0.0/");

            browserHistory.UpdateHistoryOnNavigation("/page2", "https://0.0.0.0/");
            Assert.AreEqual("/page2", browserHistory.CurrentLocation);
            Assert.AreEqual(2, browserHistory.Count);
        }

        [Test]
        public void RemoveCurrent_RemovesLastItem()
        {
            var browserHistory = new BrowserHistoryService();
            browserHistory.UpdateHistoryOnNavigation("/page1", "https://0.0.0.0/");
            browserHistory.UpdateHistoryOnNavigation("/page2", "https://0.0.0.0/");

            browserHistory.RemoveCurrent();
            Assert.AreEqual("/page1", browserHistory.CurrentLocation);
            Assert.AreEqual(1, browserHistory.Count);

            browserHistory.RemoveCurrent();
            Assert.IsNull(browserHistory.CurrentLocation);
            Assert.AreEqual(0, browserHistory.Count);

            browserHistory.RemoveCurrent();
            Assert.IsNull(browserHistory.CurrentLocation);
        }
    }
}
