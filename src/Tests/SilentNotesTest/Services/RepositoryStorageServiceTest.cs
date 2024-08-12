using System;
using System.Xml.Linq;
using Moq;
using NUnit.Framework;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotesTest.Services
{
    [TestFixture]
    public class RepositoryStorageServiceTest
    {
        [Test]
        public void LoadsSettingsReturnsStoredSettings()
        {
            NoteRepositoryModel storedSettings = new NoteRepositoryModel { Id = new Guid(), Revision = NoteRepositoryModel.NewestSupportedRevision };
            XDocument xml = XmlUtils.SerializeToXmlDocument(storedSettings);
            Mock<IXmlFileService> fileService = new Mock<IXmlFileService>();
            fileService.
                Setup(m => m.TryLoad(It.IsAny<string>(), out xml)).
                Returns(true);
            fileService.
                Setup(m => m.Exists(It.IsAny<string>())).
                Returns(true);

            RepositoryStorageServiceBase service = new TestableService(fileService.Object);
            RepositoryStorageLoadResult result = service.LoadRepositoryOrDefault(out NoteRepositoryModel repository);

            // Loaded existing settings and did not store it
            Assert.AreEqual(RepositoryStorageLoadResult.SuccessfullyLoaded, result);
            Assert.IsNotNull(repository);
            Assert.AreEqual(storedSettings.Id, repository.Id);
            fileService.Verify(m => m.TrySerializeAndSave(It.IsAny<string>(), It.IsAny<NoteRepositoryModel>()), Times.Never);
        }

        [Test]
        public void LoadRepositoryOrDefault_DoesNotOverwriteIvalidRepository()
        {
            XDocument xml = new XDocument(new XElement("Invalid"));
            Mock<IXmlFileService> fileService = new Mock<IXmlFileService>();
            fileService.
                Setup(m => m.TryLoad(It.IsAny<string>(), out xml)).
                Returns(true);
            fileService.
                Setup(m => m.Exists(It.IsAny<string>())).
                Returns(true);

            RepositoryStorageServiceBase service = new TestableService(fileService.Object);
            RepositoryStorageLoadResult result = service.LoadRepositoryOrDefault(out NoteRepositoryModel repository);

            // Loaded existing settings and did not store it
            Assert.AreEqual(RepositoryStorageLoadResult.InvalidRepository, result, "Please remove DEMO from Directory.Build.props and run again");
            Assert.AreSame(NoteRepositoryModel.InvalidRepository, repository);
            fileService.Verify(m => m.TrySerializeAndSave(It.IsAny<string>(), It.IsAny<NoteRepositoryModel>()), Times.Never);
        }

        [Test]
        public void TrySaveRepository_DoesNotOverwriteWithIvalidRepository()
        {
            Mock<IXmlFileService> fileService = new Mock<IXmlFileService>();

            RepositoryStorageServiceBase service = new TestableService(fileService.Object);
            bool res = service.TrySaveRepository(NoteRepositoryModel.InvalidRepository);

            // Did not try to save the file.
            Assert.IsFalse(res);
            fileService.Verify(m => m.TrySerializeAndSave(It.IsAny<string>(), It.IsAny<NoteRepositoryModel>()), Times.Never);
        }

        [Test]
        public void LoadSettingsCreatesDefaultIfNoFileFound()
        {
            Mock<IXmlFileService> fileService = new Mock<IXmlFileService>();
            fileService.
                Setup(m => m.Exists(It.IsAny<string>())).
                Returns(false);

            RepositoryStorageServiceBase service = new TestableService(fileService.Object);
            RepositoryStorageLoadResult result = service.LoadRepositoryOrDefault(out NoteRepositoryModel repository);

            // Created new settings and stored it
            Assert.AreEqual(RepositoryStorageLoadResult.CreatedNewEmptyRepository, result);
            Assert.IsNotNull(repository);
            fileService.Verify(m => m.TrySerializeAndSave(It.IsAny<string>(), It.IsAny<NoteRepositoryModel>()), Times.Once);
        }

        [Test]
        public void Version1ConfigWillBeUpdated()
        {
            XDocument xml = XDocument.Parse(Version1Repository);
            Mock<IXmlFileService> fileService = new Mock<IXmlFileService>();
            fileService.
                Setup(m => m.TryLoad(It.IsAny<string>(), out xml)).
                Returns(true);
            fileService.
                Setup(m => m.Exists(It.IsAny<string>())).
                Returns(true);

            RepositoryStorageServiceBase service = new TestableService(fileService.Object);
            RepositoryStorageLoadResult result = service.LoadRepositoryOrDefault(out NoteRepositoryModel repository);

            Assert.AreEqual(RepositoryStorageLoadResult.SuccessfullyLoaded, result);
            Assert.IsNotNull(repository);
            Assert.AreEqual(NoteRepositoryModel.NewestSupportedRevision, repository.Revision);
            fileService.Verify(m => m.TrySerializeAndSave(It.IsAny<string>(), It.IsAny<NoteRepositoryModel>()), Times.Once);
        }

        /// <summary>
        /// Make abstract class testable
        /// </summary>
        private class TestableService : RepositoryStorageServiceBase
        {
            public TestableService(IXmlFileService xmlFileService)
                : base(xmlFileService, new Mock<ILanguageService>().Object)
            {
            }

            public override string GetLocation()
            {
                return string.Empty;
            }
        }

        private const string Version1Repository =
@"<?xml version='1.0' encoding='utf-8'?>
<silentnotes xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' id='093b917a-f69f-4dd3-91b7-ad175fe0a4c1' revision='1' order_modified_at='2017-09-10T10:19:25.585917Z'>
  <notes>
  </notes>
</silentnotes>";
    }
}
