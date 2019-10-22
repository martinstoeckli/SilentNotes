using System;
using Moq;
using NUnit.Framework;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.StoryBoards;
using SilentNotes.StoryBoards.PullPushStory;
using VanillaCloudStorageClient;

namespace SilentNotesTest.StoryBoards.PullPushStory
{
    [TestFixture]
    public class StoreMergedRepositoryAndQuitStepTest
    {
        [Test]
        public void RejectWhenCloudNoteDoesNotExist()
        {
            NoteRepositoryModel cloudRepositoryModel = new NoteRepositoryModel();

            NoteModel localNoteModel = new NoteModel();
            NoteRepositoryModel localRepositoryModel = new NoteRepositoryModel();
            localRepositoryModel.Notes.Add(localNoteModel);

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.LoadFromSession<NoteRepositoryModel>(It.Is<PullPushStorySessionKey>(p => p == PullPushStorySessionKey.CloudRepository))).
                Returns(cloudRepositoryModel);
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out localRepositoryModel));
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();

            // Run step
            var step = new StoreMergedRepositoryAndQuitStep(
                PullPushStoryStepId.StoreMergedRepositoryAndQuit,
                storyBoard.Object,
                localNoteModel.Id,
                PullPushDirection.PullFromServer,
                CommonMocksAndStubs.LanguageService(),
                CommonMocksAndStubs.FeedbackService(),
                settingsService.Object,
                CommonMocksAndStubs.CryptoRandomService(),
                repositoryStorageService.Object,
                CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object)); ;
            Assert.DoesNotThrowAsync(step.Run);

            // repository is not stored to the local device, nor to the cloud
            repositoryStorageService.Verify(m => m.TrySaveRepository(It.IsAny<NoteRepositoryModel>()), Times.Never);
            cloudStorageClient.Verify(m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CloudStorageCredentials>()), Times.Never);
        }

        [Test]
        public void RejectWhenNotesAreEqual()
        {
            NoteModel noteModel = new NoteModel();
            NoteRepositoryModel repositoryModel = new NoteRepositoryModel();
            repositoryModel.Notes.Add(noteModel);

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.LoadFromSession<NoteRepositoryModel>(It.Is<PullPushStorySessionKey>(p => p == PullPushStorySessionKey.CloudRepository))).
                Returns(repositoryModel); // same as from repositoryStorageService
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(new SettingsModel());
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repositoryModel)); // same as from storyBoard
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();

            // Run step
            var step = new StoreMergedRepositoryAndQuitStep(
                PullPushStoryStepId.StoreMergedRepositoryAndQuit,
                storyBoard.Object,
                noteModel.Id,
                PullPushDirection.PullFromServer,
                CommonMocksAndStubs.LanguageService(),
                CommonMocksAndStubs.FeedbackService(),
                settingsService.Object,
                CommonMocksAndStubs.CryptoRandomService(),
                repositoryStorageService.Object,
                CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object)); ;
            Assert.DoesNotThrowAsync(step.Run);

            // repository is not stored to the local device, nor to the cloud
            repositoryStorageService.Verify(m => m.TrySaveRepository(It.IsAny<NoteRepositoryModel>()), Times.Never);
            cloudStorageClient.Verify(m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CloudStorageCredentials>()), Times.Never);
        }

        [Test]
        public void PullStoresCorrectData()
        {
            NoteModel cloudNoteModel = new NoteModel
            {
                HtmlContent = "cloudContent",
                InRecyclingBin = true,
                ModifiedAt = new DateTime(1900, 10, 22)
            };
            NoteRepositoryModel cloudRepositoryModel = new NoteRepositoryModel();
            cloudRepositoryModel.Notes.Add(cloudNoteModel);

            NoteModel localNoteModel = new NoteModel 
            {
                Id = cloudNoteModel.Id,
                HtmlContent="localContent",
                InRecyclingBin = false,
                ModifiedAt = new DateTime(2000, 10, 22) 
            };
            NoteRepositoryModel localRepositoryModel = new NoteRepositoryModel();
            localRepositoryModel.Notes.Add(localNoteModel);

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.LoadFromSession<NoteRepositoryModel>(It.Is<PullPushStorySessionKey>(p => p == PullPushStorySessionKey.CloudRepository))).
                Returns(cloudRepositoryModel);
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(new SettingsModel());
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out localRepositoryModel));
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();

            // Run step
            var step = new StoreMergedRepositoryAndQuitStep(
                PullPushStoryStepId.StoreMergedRepositoryAndQuit,
                storyBoard.Object,
                cloudNoteModel.Id,
                PullPushDirection.PullFromServer,
                CommonMocksAndStubs.LanguageService(),
                CommonMocksAndStubs.FeedbackService(),
                settingsService.Object,
                CommonMocksAndStubs.CryptoRandomService(),
                repositoryStorageService.Object,
                CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object)); ;
            Assert.DoesNotThrowAsync(step.Run);

            // Local note object still exists and contains the content of the cloud
            NoteModel newLocalNote = localRepositoryModel.Notes.FindById(cloudNoteModel.Id);
            Assert.AreSame(localNoteModel, newLocalNote);
            Assert.AreEqual("cloudContent", newLocalNote.HtmlContent);
            Assert.AreEqual(true, newLocalNote.InRecyclingBin);
            Assert.AreEqual(new DateTime(1900, 10, 22), newLocalNote.ModifiedAt);

            // repository was stored to the local device, but not to the cloud
            repositoryStorageService.Verify(m => m.TrySaveRepository(It.IsAny<NoteRepositoryModel>()), Times.Once);
            cloudStorageClient.Verify(m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CloudStorageCredentials>()), Times.Never);
        }

        [Test]
        public void PushStoresCorrectData()
        {
            NoteModel cloudNoteModel = new NoteModel
            {
                HtmlContent = "cloudContent",
                InRecyclingBin = true,
                ModifiedAt = new DateTime(2000, 10, 22)
            };
            NoteRepositoryModel cloudRepositoryModel = new NoteRepositoryModel();
            cloudRepositoryModel.Notes.Add(cloudNoteModel);

            NoteModel localNoteModel = new NoteModel
            {
                Id = cloudNoteModel.Id,
                HtmlContent = "localContent",
                InRecyclingBin = false,
                ModifiedAt = new DateTime(1900, 10, 22)
            };
            NoteRepositoryModel localRepositoryModel = new NoteRepositoryModel { Id = cloudNoteModel.Id };
            localRepositoryModel.Notes.Add(localNoteModel);

            Mock<IStoryBoard> storyBoard = new Mock<IStoryBoard>();
            storyBoard.
                Setup(m => m.LoadFromSession<NoteRepositoryModel>(It.Is<PullPushStorySessionKey>(p => p == PullPushStorySessionKey.CloudRepository))).
                Returns(cloudRepositoryModel);
            Mock<ISettingsService> settingsService = new Mock<ISettingsService>();
            settingsService.
                Setup(m => m.LoadSettingsOrDefault()).Returns(new SettingsModel { TransferCode = "AAAAAAAAAAAA", Credentials = new SerializeableCloudStorageCredentials() });
            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out localRepositoryModel));
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();

            // Run step
            var step = new StoreMergedRepositoryAndQuitStep(
                PullPushStoryStepId.StoreMergedRepositoryAndQuit,
                storyBoard.Object,
                cloudNoteModel.Id,
                PullPushDirection.PushToServer,
                CommonMocksAndStubs.LanguageService(),
                CommonMocksAndStubs.FeedbackService(),
                settingsService.Object,
                CommonMocksAndStubs.CryptoRandomService(),
                repositoryStorageService.Object,
                CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object)); ;
            Assert.DoesNotThrowAsync(step.Run);

            // Cloud note object still exists and contains the content of the cloud plus a new modification date
            NoteModel newCloudNote = cloudRepositoryModel.Notes.FindById(localNoteModel.Id);
            Assert.AreSame(cloudNoteModel, newCloudNote);
            Assert.AreEqual("localContent", newCloudNote.HtmlContent);
            Assert.AreEqual(false, newCloudNote.InRecyclingBin);
            Assert.AreEqual(DateTime.UtcNow.Day, newCloudNote.ModifiedAt.Day);

            // Local note object still exists and has new modification date
            NoteModel newLocalNote = localRepositoryModel.Notes.FindById(cloudNoteModel.Id);
            Assert.AreEqual(DateTime.UtcNow.Day, newLocalNote.ModifiedAt.Day);

            // repository was stored to the local device and to the cloud
            repositoryStorageService.Verify(m => m.TrySaveRepository(It.IsAny<NoteRepositoryModel>()), Times.Once);
            cloudStorageClient.Verify(m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CloudStorageCredentials>()), Times.Once);
        }
    }
}
