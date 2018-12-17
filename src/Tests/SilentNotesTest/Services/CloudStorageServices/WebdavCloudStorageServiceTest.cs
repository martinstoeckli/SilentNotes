using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using NUnit.Framework;
using SilentNotes.Services.CloudStorageServices;

namespace SilentNotesTest.Services.CloudStorageServices
{
    /// <summary>
    /// GMX _is_ a Webdav server, the integration tests are done in the <see cref="GmxCloudStorageServiceTest"/>.
    /// </summary>
    [TestFixture]
    public class WebdavCloudStorageServiceTest
    {
        [Test]
        public void ParseGmxWebdavResponseCorrectly()
        {
            List<string> fileNames = WebdavCloudStorageService.ParseWebdavResponseForFileNames(GetGxmResponse());
            Assert.AreEqual(7, fileNames.Count);
            Assert.AreEqual("silentnotes_repository_demo.silentnotes", fileNames[0]);
            Assert.AreEqual("silentnotes_repository_dev.silentnotes", fileNames[1]);
            Assert.AreEqual("silentnotes_repository_unittest.silentnotes", fileNames[2]);
            Assert.AreEqual("silentnotes_repository.silentnotes", fileNames[3]);
            Assert.AreEqual("tinu-2017-09-19.kdbx", fileNames[4]);
            Assert.AreEqual("tinu-2017-10-19.kdbx", fileNames[5]);
            Assert.AreEqual("tinu space.kdbx", fileNames[6]);
        }

        [Test]
        public void ParseStratoWebdavResponseCorrectly()
        {
            List<string> fileNames = WebdavCloudStorageService.ParseWebdavResponseForFileNames(GetStratoResponse());
            Assert.AreEqual(3, fileNames.Count);
            Assert.AreEqual("Bearbeiten1.txt", fileNames[0]);
            Assert.AreEqual("test_a.txt", fileNames[1]);
            Assert.AreEqual("Bearbeiten2.txt", fileNames[2]);
        }

        private XDocument GetGxmResponse()
        {
            string response = @"
<D:multistatus xmlns:D='DAV:'>
  <D:response>
    <D:href>/</D:href>
    <D:propstat>
      <D:prop>
        <D:displayname>/</D:displayname>
        <D:resourcetype>
          <D:collection />
        </D:resourcetype>
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
  <D:response>
    <D:href>/Externe%20Ordner/</D:href>
    <D:propstat>
      <D:prop>
        <D:displayname>Externe Ordner</D:displayname>
        <D:resourcetype>
          <D:collection />
        </D:resourcetype>
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
  <D:response>
    <D:href>/Gel%c3%b6schte%20Dateien/</D:href>
    <D:propstat>
      <D:prop>
        <D:displayname>Gelöschte Dateien</D:displayname>
        <D:resourcetype>
          <D:collection />
        </D:resourcetype>
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
  <D:response>
    <D:href>/Meine%20Bilder/</D:href>
    <D:propstat>
      <D:prop>
        <D:displayname>Meine Bilder</D:displayname>
        <D:resourcetype>
          <D:collection />
        </D:resourcetype>
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
  <D:response>
    <D:href>/Meine%20Dokumente/</D:href>
    <D:propstat>
      <D:prop>
        <D:displayname>Meine Dokumente</D:displayname>
        <D:resourcetype>
          <D:collection />
        </D:resourcetype>
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
  <D:response>
    <D:href>/Meine%20Musikdateien/</D:href>
    <D:propstat>
      <D:prop>
        <D:displayname>Meine Musikdateien</D:displayname>
        <D:resourcetype>
          <D:collection />
        </D:resourcetype>
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
  <D:response>
    <D:href>/Neue%20Dateianlagen/</D:href>
    <D:propstat>
      <D:prop>
        <D:displayname>Neue Dateianlagen</D:displayname>
        <D:resourcetype>
          <D:collection />
        </D:resourcetype>
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
  <D:response>
    <D:href>/silentnotes_repository_demo.silentnotes</D:href>
    <D:propstat>
      <D:prop>
        <D:displayname>silentnotes_repository_demo.silentnotes</D:displayname>
        <D:resourcetype />
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
  <D:response>
    <D:href>/silentnotes_repository_dev.silentnotes</D:href>
    <D:propstat>
      <D:prop>
        <D:displayname>silentnotes_repository_dev.silentnotes</D:displayname>
        <D:resourcetype />
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
  <D:response>
    <D:href>/silentnotes_repository_unittest.silentnotes</D:href>
    <D:propstat>
      <D:prop>
        <D:displayname>silentnotes_repository_unittest.silentnotes</D:displayname>
        <D:resourcetype />
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
  <D:response>
    <D:href>/silentnotes_repository.silentnotes</D:href>
    <D:propstat>
      <D:prop>
        <D:displayname>silentnotes_repository.silentnotes</D:displayname>
        <D:resourcetype />
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
  <D:response>
    <D:href>/tinu-2017-09-19.kdbx</D:href>
    <D:propstat>
      <D:prop>
        <D:displayname>tinu-2017-09-19.kdbx</D:displayname>
        <D:resourcetype />
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
  <D:response>
    <D:href>/tinu-2017-10-19.kdbx</D:href>
    <D:propstat>
      <D:prop>
        <D:displayname>tinu-2017-10-19.kdbx</D:displayname>
        <D:resourcetype />
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
  <D:response>
    <D:href>/tinu%20space.kdbx</D:href>
    <D:propstat>
      <D:prop>
        <D:displayname>tinu.kdbx</D:displayname>
        <D:resourcetype />
      </D:prop>
      <D:status>HTTP/1.1 200 OK</D:status>
    </D:propstat>
  </D:response>
</D:multistatus>
";
            using (TextReader rextReader = new StringReader(response))
            {
                return XDocument.Load(rextReader);
            }
        }

        private XDocument GetStratoResponse()
        {
            string response = @"
<ns0:multistatus xmlns:D='DAV:' xmlns:ns0='DAV:'>
  <g0:response xmlns:lp1='DAV:' xmlns:g0='DAV:'>
    <g0:href>/users/martinstoeckli/</g0:href>
    <g0:propstat>
      <g0:prop>
        <g0:resourcetype>
          <g0:collection />
        </g0:resourcetype>
      </g0:prop>
      <g0:status>HTTP/1.1 200 OK</g0:status>
    </g0:propstat>
    <g0:propstat>
      <g0:prop>
        <g0:displayname />
      </g0:prop>
      <g0:status>HTTP/1.1 404 Not Found</g0:status>
    </g0:propstat>
  </g0:response>
  <g0:response xmlns:lp1='DAV:' xmlns:g0='DAV:'>
    <g0:href>/users/martinstoeckli/Bearbeiten1.txt</g0:href>
    <g0:propstat>
      <g0:prop>
        <g0:resourcetype />
      </g0:prop>
      <g0:status>HTTP/1.1 200 OK</g0:status>
    </g0:propstat>
    <g0:propstat>
      <g0:prop>
        <g0:displayname />
      </g0:prop>
      <g0:status>HTTP/1.1 404 Not Found</g0:status>
    </g0:propstat>
  </g0:response>
  <g0:response xmlns:lp1='DAV:' xmlns:g0='DAV:'>
    <g0:href>/users/martinstoeckli/.hidrive/</g0:href>
    <g0:propstat>
      <g0:prop>
        <g0:resourcetype>
          <g0:collection />
        </g0:resourcetype>
      </g0:prop>
      <g0:status>HTTP/1.1 200 OK</g0:status>
    </g0:propstat>
    <g0:propstat>
      <g0:prop>
        <g0:displayname />
      </g0:prop>
      <g0:status>HTTP/1.1 404 Not Found</g0:status>
    </g0:propstat>
  </g0:response>
  <g0:response xmlns:lp1='DAV:' xmlns:g0='DAV:'>
    <g0:href>/users/martinstoeckli/test_a.txt</g0:href>
    <g0:propstat>
      <g0:prop>
        <g0:resourcetype />
      </g0:prop>
      <g0:status>HTTP/1.1 200 OK</g0:status>
    </g0:propstat>
    <g0:propstat>
      <g0:prop>
        <g0:displayname />
      </g0:prop>
      <g0:status>HTTP/1.1 404 Not Found</g0:status>
    </g0:propstat>
  </g0:response>
  <g0:response xmlns:lp1='DAV:' xmlns:g0='DAV:'>
    <g0:href>/users/martinstoeckli/subfolder1/</g0:href>
    <g0:propstat>
      <g0:prop>
        <g0:resourcetype>
          <g0:collection />
        </g0:resourcetype>
      </g0:prop>
      <g0:status>HTTP/1.1 200 OK</g0:status>
    </g0:propstat>
    <g0:propstat>
      <g0:prop>
        <g0:displayname />
      </g0:prop>
      <g0:status>HTTP/1.1 404 Not Found</g0:status>
    </g0:propstat>
  </g0:response>
  <g0:response xmlns:lp1='DAV:' xmlns:g0='DAV:'>
    <g0:href>/users/martinstoeckli/Bearbeiten2.txt</g0:href>
    <g0:propstat>
      <g0:prop>
        <g0:resourcetype />
      </g0:prop>
      <g0:status>HTTP/1.1 200 OK</g0:status>
    </g0:propstat>
    <g0:propstat>
      <g0:prop>
        <g0:displayname />
      </g0:prop>
      <g0:status>HTTP/1.1 404 Not Found</g0:status>
    </g0:propstat>
  </g0:response>
</ns0:multistatus>";
            using (TextReader rextReader = new StringReader(response))
            {
                return XDocument.Load(rextReader);
            }
        }
    }
}
