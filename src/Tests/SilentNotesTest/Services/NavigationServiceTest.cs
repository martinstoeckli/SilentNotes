using NUnit.Framework;
using SilentNotes.Services;

namespace SilentNotesTest.Services
{
    [TestFixture]
    public class NavigationServiceTest
    {
        [Test]
        public void GetRelativeUri_KeepsRelativeLocations()
        {
            Assert.AreEqual("/myway", NavigationService.GetRelativeUri("/myway", "https://0.0.0.0/"));
            Assert.AreEqual("/myway", NavigationService.GetRelativeUri("/myway", "https://0.0.0.0"));
            Assert.AreEqual("myway", NavigationService.GetRelativeUri("myway", "https://0.0.0.0/"));
            Assert.AreEqual("/myway", NavigationService.GetRelativeUri("/myway", string.Empty));
        }

        [Test]
        public void GetRelativeUri_KeepsLeadingSlash()
        {
            Assert.AreEqual("/myway", NavigationService.GetRelativeUri("https://0.0.0.0/myway", "https://0.0.0.0/"));
            Assert.AreEqual("/myway", NavigationService.GetRelativeUri("https://0.0.0.0/myway", "https://0.0.0.0"));
        }

        [Test]
        public void ExtractRouteName_WorksCorrectly()
        {
            Assert.AreEqual("/myway", NavigationService.ExtractRouteName("https://0.0.0.0/myway/param1/param2", "https://0.0.0.0/"));
            Assert.AreEqual("/myway", NavigationService.ExtractRouteName("/myway/param1", "https://0.0.0.0"));
            Assert.AreEqual("/myway", NavigationService.ExtractRouteName("/myway/param1/", "https://0.0.0.0"));
        }
    }
}
