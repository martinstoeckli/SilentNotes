using NUnit.Framework;
using SilentNotes.HtmlView;
using System.Threading;

namespace SilentNotesTest.HtmView
{
    [TestFixture]
    public class HtmlViewDuplicateEventFinderTest
    {
        [Test]
        public void FirstEventIsNotADuplicate()
        {
            HtmlViewDuplicateEventFinder duplicateFinder = new HtmlViewDuplicateEventFinder();
            Assert.IsFalse(duplicateFinder.IsDuplicateEvent("abc"));
        }

        [Test]
        public void CorrectlyDetectDuplicate()
        {
            HtmlViewDuplicateEventFinder duplicateFinder = new HtmlViewDuplicateEventFinder();
            duplicateFinder.IsDuplicateEvent("abc");
            Assert.IsTrue(duplicateFinder.IsDuplicateEvent("abc"));
            Assert.IsTrue(duplicateFinder.IsDuplicateEvent("abc"));
        }

        [Test]
        public void DontDetectDifferentEvents()
        {
            HtmlViewDuplicateEventFinder duplicateFinder = new HtmlViewDuplicateEventFinder();
            duplicateFinder.IsDuplicateEvent("abc");
            Assert.IsFalse(duplicateFinder.IsDuplicateEvent("zzz"));
            Assert.IsFalse(duplicateFinder.IsDuplicateEvent("yyy"));
        }

        [Test]
        public void DontDetectDuplicatesIfTimeSpanIsTooLong()
        {
            HtmlViewDuplicateEventFinder duplicateFinder = new HtmlViewDuplicateEventFinder();
            duplicateFinder.IsDuplicateEvent("abc");
            Thread.Sleep(300);
            Assert.IsFalse(duplicateFinder.IsDuplicateEvent("abc"));
        }
    }
}
