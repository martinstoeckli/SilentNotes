using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Stories;
using SilentNotes.Stories.SynchronizationStory;
using VanillaCloudStorageClient;

namespace SilentNotesTest.Stories.SynchronizationStory
{
    [TestClass]
    public class IsCloudServiceSetStepTest
    {
        [TestMethod]
        public async ValueTask RunStep_GoesToShowFirstTimeDialog_WithoutCloudStorageClientSet()
        {
            var settingsModel = new SettingsModel();

            var settingsService = CommonMocksAndStubs.SettingsService(settingsModel);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<ISettingsService>(settingsService);

            var model = new SynchronizationStoryModel { StoryMode = StoryMode.Silent };
            var res = await new IsCloudServiceSetStep().RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // Continue with correct next step to ask for missing credentials
            Assert.IsInstanceOfType<ShowFirstTimeDialogStep>(res.NextStep);
            Assert.IsNull(model.Credentials);
        }

        [TestMethod]
        public async ValueTask RunStep_GoesToExistsCloudRepository_WithCloudStorageClientSet()
        {
            var credentials = new SerializeableCloudStorageCredentials { CloudStorageId = CloudStorageClientFactory.CloudStorageIdWebdav };
            var settingsModel = new SettingsModel { Credentials = credentials };

            var serviceCollection = new ServiceCollection();
            var settingsService = CommonMocksAndStubs.SettingsService(settingsModel);
            serviceCollection.AddSingleton<ISettingsService>(settingsService);

            var model = new SynchronizationStoryModel { StoryMode = StoryMode.Silent };
            var res = await new IsCloudServiceSetStep().RunStep(model, serviceCollection.BuildServiceProvider(), model.StoryMode);

            // Continue with correct next step with credentials set
            Assert.IsInstanceOfType<ExistsCloudRepositoryStep>(res.NextStep);
            Assert.AreEqual(credentials, model.Credentials);
        }
    }
}
