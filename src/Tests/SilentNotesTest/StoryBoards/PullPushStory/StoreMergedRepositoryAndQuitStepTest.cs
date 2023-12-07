using System;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Stories;
using SilentNotes.Stories.PullPushStory;
using SilentNotes.Stories.SynchronizationStory;
using VanillaCloudStorageClient;
//using VanillaCloudStorageClient;

//using SynchronizationStorySessionKey = SilentNotes.StoryBoards.SynchronizationStory.SynchronizationStorySessionKey;

namespace SilentNotesTest.StoryBoards.PullPushStory
{
    [TestFixture]
    public class StoreMergedRepositoryAndQuitStepTest
    {
        [Test]
        public async Task RejectWhenCloudNoteDoesNotExist()
        {
            NoteRepositoryModel cloudRepositoryModel = new NoteRepositoryModel();

            NoteModel localNoteModel = new NoteModel();
            NoteRepositoryModel localRepositoryModel = new NoteRepositoryModel();
            localRepositoryModel.Notes.Add(localNoteModel);

            var model = new PullPushStoryModel(localNoteModel.Id, PullPushDirection.PullFromServer)
            {
                CloudRepository = cloudRepositoryModel,
                StoryMode = StoryMode.Toasts,
            };

            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out localRepositoryModel));
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(CommonMocksAndStubs.SettingsService())
                .AddSingleton<IRepositoryStorageService>(repositoryStorageService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService("pushpull_error_no_cloud_note"))
                .AddSingleton<IFeedbackService>(CommonMocksAndStubs.FeedbackService())
                .AddSingleton<ICryptoRandomService>(CommonMocksAndStubs.CryptoRandomService())
                .AddSingleton<ICloudStorageClientFactory>(CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));

            // Run step
            var step = new SilentNotes.Stories.PullPushStory.StoreMergedRepositoryAndQuitStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // repository is not stored to the local device, nor to the cloud
            repositoryStorageService.Verify(m => m.TrySaveRepository(It.IsAny<NoteRepositoryModel>()), Times.Never);
            cloudStorageClient.Verify(m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CloudStorageCredentials>()), Times.Never);
            Assert.IsNull(result.NextStep);
            Assert.AreEqual("pushpull_error_no_cloud_note", result.Toast);
        }

        [Test]
        public async Task RejectWhenNotesAreEqual()
        {
            NoteModel noteModel = new NoteModel();
            NoteRepositoryModel repositoryModel = new NoteRepositoryModel();
            repositoryModel.Notes.Add(noteModel);

            var model = new PullPushStoryModel(noteModel.Id, PullPushDirection.PullFromServer)
            {
                CloudRepository = repositoryModel, // same as from repositoryStorageService
                StoryMode = StoryMode.Toasts,
            };

            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out repositoryModel));
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(CommonMocksAndStubs.SettingsService())
                .AddSingleton<IRepositoryStorageService>(repositoryStorageService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService("pushpull_success"))
                .AddSingleton<IFeedbackService>(CommonMocksAndStubs.FeedbackService())
                .AddSingleton<ICryptoRandomService>(CommonMocksAndStubs.CryptoRandomService())
                .AddSingleton<ICloudStorageClientFactory>(CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));

            // Run step
            var step = new SilentNotes.Stories.PullPushStory.StoreMergedRepositoryAndQuitStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // repository is not stored to the local device, nor to the cloud
            repositoryStorageService.Verify(m => m.TrySaveRepository(It.IsAny<NoteRepositoryModel>()), Times.Never);
            cloudStorageClient.Verify(m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CloudStorageCredentials>()), Times.Never);
            Assert.AreEqual("pushpull_success", result.Toast);
        }

        [Test]
        public async Task PullStoresCorrectData()
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
                HtmlContent = "localContent",
                InRecyclingBin = false,
                ModifiedAt = new DateTime(2000, 10, 22)
            };
            NoteRepositoryModel localRepositoryModel = new NoteRepositoryModel();
            localRepositoryModel.Notes.Add(localNoteModel);

            var model = new PullPushStoryModel(cloudNoteModel.Id, PullPushDirection.PullFromServer)
            {
                CloudRepository = cloudRepositoryModel,
                StoryMode = StoryMode.Toasts,
            };

            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out localRepositoryModel));
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(CommonMocksAndStubs.SettingsService())
                .AddSingleton<IRepositoryStorageService>(repositoryStorageService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService("pushpull_success"))
                .AddSingleton<IFeedbackService>(CommonMocksAndStubs.FeedbackService())
                .AddSingleton<ICryptoRandomService>(CommonMocksAndStubs.CryptoRandomService())
                .AddSingleton<ICloudStorageClientFactory>(CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));

            // Run step
            var step = new SilentNotes.Stories.PullPushStory.StoreMergedRepositoryAndQuitStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // repository was stored to the local device, but not to the cloud
            repositoryStorageService.Verify(m => m.TrySaveRepository(It.IsAny<NoteRepositoryModel>()), Times.Once);
            cloudStorageClient.Verify(m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CloudStorageCredentials>()), Times.Never);
            Assert.AreEqual("pushpull_success", result.Toast);
        }

        [Test]
        public async Task PushStoresCorrectData()
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

            var model = new PullPushStoryModel(cloudNoteModel.Id, PullPushDirection.PushToServer)
            {
                CloudRepository = cloudRepositoryModel,
                StoryMode = StoryMode.Toasts,
            };

            Mock<IRepositoryStorageService> repositoryStorageService = new Mock<IRepositoryStorageService>();
            repositoryStorageService.
                Setup(m => m.LoadRepositoryOrDefault(out localRepositoryModel));
            Mock<ICloudStorageClient> cloudStorageClient = new Mock<ICloudStorageClient>();

            var serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<ISettingsService>(CommonMocksAndStubs.SettingsService(new SettingsModel { TransferCode = "AAAAAAAAAAAA", Credentials = new SerializeableCloudStorageCredentials() }))
                .AddSingleton<IRepositoryStorageService>(repositoryStorageService.Object)
                .AddSingleton<ILanguageService>(CommonMocksAndStubs.LanguageService("pushpull_success"))
                .AddSingleton<IFeedbackService>(CommonMocksAndStubs.FeedbackService())
                .AddSingleton<ICryptoRandomService>(CommonMocksAndStubs.CryptoRandomService())
                .AddSingleton<ICloudStorageClientFactory>(CommonMocksAndStubs.CloudStorageClientFactory(cloudStorageClient.Object));

            // Run step
            var step = new SilentNotes.Stories.PullPushStory.StoreMergedRepositoryAndQuitStep();
            var result = await step.RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            Assert.AreEqual("pushpull_success", result.Toast);

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
