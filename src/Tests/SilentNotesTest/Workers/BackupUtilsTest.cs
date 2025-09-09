using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotesTest.Workers
{
    [TestClass]
    public class BackupUtilsTest
    {
        [TestMethod]
        public async Task CreateBackup_StoresBackup()
        {
            var folderPickerService = new Mock<IFolderPickerService>();
            folderPickerService
                .Setup(m => m.PickFolder())
                .ReturnsAsync(() => true);
            NoteRepositoryModel repository = CreateTestRepository();
            var repositoryService = CommonMocksAndStubs.RepositoryStorageServiceMock(repository);

            await BackupUtils.CreateBackup(folderPickerService.Object, repositoryService.Object);

            folderPickerService.Verify(m => m.TrySaveFileToPickedFolder(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Once);
        }

        [TestMethod]
        public async Task RestoreBackup_ReturnsTrueIfCanceledByUser()
        {
            var filePickerService = new Mock<IFilePickerService>();
            filePickerService
                .Setup(m => m.PickFile())
                .ReturnsAsync(() => false); // Canceled by user
            NoteRepositoryModel repository = CreateTestRepository();
            var repositoryService = CommonMocksAndStubs.RepositoryStorageServiceMock(repository);

            bool result = await BackupUtils.TryRestoreBackup(filePickerService.Object, repositoryService.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task RestoreBackup_ReturnsTrueIfSuccessful()
        {
            NoteRepositoryModel repository = new NoteRepositoryModel();
            repository.Notes.Add(new NoteModel { HtmlContent = "The fox jumps over the lazy doc." });
            byte[] zipArchiveContent = CreateValidBackupContent(repository);

            var filePickerService = new Mock<IFilePickerService>();
            filePickerService
                .Setup(m => m.PickFile())
                .ReturnsAsync(() => true);
            filePickerService
                .Setup(m => m.ReadPickedFile())
                .ReturnsAsync(zipArchiveContent);
            var repositoryService = new Mock<IRepositoryStorageService>();
            repositoryService
                .Setup(m => m.TryLoadRepositoryFromFile(It.IsAny<byte[]>(), out repository))
                .Returns(() => true);

            bool result = await BackupUtils.TryRestoreBackup(filePickerService.Object, repositoryService.Object);
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task RestoreBackup_ReturnsFalseIfNoNotes()
        {
            NoteRepositoryModel repository = new NoteRepositoryModel(); // Repository wit 0 notes
            byte[] zipArchiveContent = CreateValidBackupContent(repository);

            var filePickerService = new Mock<IFilePickerService>();
            filePickerService
                .Setup(m => m.PickFile())
                .ReturnsAsync(() => true);
            filePickerService
                .Setup(m => m.ReadPickedFile())
                .ReturnsAsync(zipArchiveContent);
            var repositoryService = new Mock<IRepositoryStorageService>();
            repositoryService
                .Setup(m => m.TryLoadRepositoryFromFile(It.IsAny<byte[]>(), out repository))
                .Returns(() => true);

            bool result = await BackupUtils.TryRestoreBackup(filePickerService.Object, repositoryService.Object);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task RestoreBackup_ReturnsFalseIfNotValidArchive()
        {
            byte[] fileContent = Encoding.UTF8.GetBytes("Not a zip file content");

            var filePickerService = new Mock<IFilePickerService>();
            filePickerService
                .Setup(m => m.PickFile())
                .ReturnsAsync(() => true);
            filePickerService
                .Setup(m => m.ReadPickedFile())
                .ReturnsAsync(fileContent);
            var repositoryService = new Mock<IRepositoryStorageService>();

            bool result = await BackupUtils.TryRestoreBackup(filePickerService.Object, repositoryService.Object);
            Assert.IsFalse(result);
        }

        private static NoteRepositoryModel CreateTestRepository()
        {
            NoteRepositoryModel repository = new NoteRepositoryModel();
            repository.Notes.Add(new NoteModel { HtmlContent = "<html></html>" });
            return repository;
        }

        private static byte[] CreateValidBackupContent(NoteRepositoryModel repository, string repositoryFileName = NoteRepositoryModel.RepositoryFileName)
        {
            var repositoryEntry = new CompressUtils.CompressEntry { Name = repositoryFileName };
            using (var xmlStream = new MemoryStream())
            {
                XmlUtils.SerializeToXmlStream(repository, xmlStream, Encoding.UTF8);
                repositoryEntry.Data = xmlStream.ToArray();
            }
            return CompressUtils.CreateZipArchive(new[] { repositoryEntry });
        }
    }
}
