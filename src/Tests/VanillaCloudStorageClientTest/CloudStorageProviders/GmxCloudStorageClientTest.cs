using System.Threading.Tasks;
using Flurl.Http.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VanillaCloudStorageClient;
using VanillaCloudStorageClient.CloudStorageProviders;

namespace VanillaCloudStorageClientTest.CloudStorageProviders
{
    /// <summary>
    /// Most functionallity is already tested by the <see cref="WebdavCloudStorageClient"/>.
    /// </summary>
    [TestClass]
    public class GmxCloudStorageClientTest
    {
        [TestMethod]
        public void ChoosesCorrectUrlForGmxNetEmail()
        {
            // Put flurl into test mode
            using (HttpTest httpTest = new HttpTest())
            {
                httpTest.RespondWith("a");

                var credentials = new CloudStorageCredentials
                {
                    Username = "example@gmx.net", // gmx.net domain
                    UnprotectedPassword = "dummy"
                };

                byte[] res = Task.Run(async () => await DownloadFileWorksAsync("a.txt", credentials)).Result;

                httpTest.ShouldHaveCalled("https://webdav.mc.gmx.net/*");
            }
        }

        [TestMethod]
        public void ChoosesCorrectUrlForGmxComEmail()
        {
            // Put flurl into test mode
            using (HttpTest httpTest = new HttpTest())
            {
                httpTest.RespondWith("a");

                var credentials = new CloudStorageCredentials
                {
                    Username = "example@gmx.com", // gmx.com domain
                    UnprotectedPassword = "dummy",
                    Url = "http://example.com" // should be overwritten
                };

                byte[] res = Task.Run(async () => await DownloadFileWorksAsync("a.txt", credentials)).Result;

                httpTest.ShouldHaveCalled("https://storage-file-eu.gmx.com/*");
            }
        }

        private async Task<byte[]> DownloadFileWorksAsync(string fileName, CloudStorageCredentials credentials)
        {
            ICloudStorageClient client = new GmxCloudStorageClient(false);
            byte[] result = await client.DownloadFileAsync(fileName, credentials);
            return result;
        }
    }
}
