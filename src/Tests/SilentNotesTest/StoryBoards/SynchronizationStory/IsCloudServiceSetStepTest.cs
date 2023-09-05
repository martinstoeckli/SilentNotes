using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Stories;
using SilentNotes.Stories.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotesTest.StoryBoards.SynchronizationStory
{
    [TestFixture]
    public class IsCloudServiceSetStepTest
    {
        [Test]
        public async ValueTask RunStep_GoesToShowFirstTimeDialog_WithoutCloudStorageClientSet()
        {
            var settingsModel = new SettingsModel();

            var serviceCollection = new ServiceCollection();
            var settingsService = CommonMocksAndStubs.SettingsService(settingsModel);
            serviceCollection.AddSingleton<ISettingsService>(settingsService);

            var model = new SynchronizationStoryModel();
            var res = await new IsCloudServiceSetStep().RunStep(model, serviceCollection.BuildServiceProvider(), StoryMode.Silent);

            // Continue with correct next step to ask for missing credentials
            Assert.IsTrue(res.NextStepIs(typeof(ShowFirstTimeDialogStep)));
            Assert.IsNull(model.Credentials);
        }

        [Test]
        public async ValueTask RunStep_GoesToExistsCloudRepository_WithCloudStorageClientSet()
        {
            var credentials = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdWebdav };
            var settingsModel = new SettingsModel { Credentials = credentials };

            var serviceCollection = new ServiceCollection();
            var settingsService = CommonMocksAndStubs.SettingsService(settingsModel);
            serviceCollection.AddSingleton<ISettingsService>(settingsService);

            var model = new SynchronizationStoryModel();
            var res = await new IsCloudServiceSetStep().RunStep(model, serviceCollection.BuildServiceProvider(), StoryMode.Silent);

            // Continue with correct next step with credentials set
            Assert.IsTrue(res.NextStepIs(typeof(ExistsCloudRepositoryStep)));
            Assert.AreEqual(credentials, model.Credentials);
        }
    }
}
