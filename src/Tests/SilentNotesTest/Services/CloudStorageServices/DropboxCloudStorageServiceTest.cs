using Moq;
using NUnit.Framework;
using SilentNotes;
using SilentNotes.Services;
using SilentNotes.Services.CloudStorageServices;

namespace SilentNotesTest.Services.CloudStorageServices
{
    [TestFixture]
    public class DropboxCloudStorageServiceTest
    {
        private ICryptoRandomService _randomSource;

        [OneTimeSetUp]
        public void TestSetup()
        {
            _randomSource = new RandomSource4UnitTest();
        }

        [Test]
        public void ShowOauth2LoginPageOpensBrowser()
        {
            Mock<INativeBrowserService> nativeBrowserService = new Mock<INativeBrowserService>();

            IOauth2CloudStorageService service = new DropboxCloudStorageService(nativeBrowserService.Object, _randomSource);
            service.ShowOauth2LoginPage();

            nativeBrowserService.Verify(
                x => x.OpenWebsiteInApp(It.Is<string>(
                    s => s.Contains("https://www.dropbox.com/oauth2/authorize") && s.Contains("client_id=2drl5n333") && s.Contains("redirect_uri=ch.martinstoeckli.silentnotes") && s.Contains("response_type=token"))),
                Times.Once);
        }
    }
}
