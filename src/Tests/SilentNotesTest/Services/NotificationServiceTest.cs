using System;
using System.Threading.Tasks;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SilentNotes.Models;
using SilentNotes.Services;

namespace SilentNotesTest.Services
{
    [TestClass]
    public class NotificationServiceTest
    {
        [TestMethod]
        public async Task ShowNextNotification_ShowsMessageWhenDue()
        {
            // Prepare settings
            var settingsModel = new SettingsModel
            {
                TransferCode = "123456789",
                NotificationTriggers =
                {
                    new NotificationTriggerModel
                    {
                        Id = NotificationService.TransferCodeNotificationId,
                        CreatedAt = DateTime.UtcNow.AddDays(-20), // first sync was 20 days ago
                    }
                }
            };

            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();
            INotificationService notificationService = new NotificationService(
                feedbackService.Object,
                CommonMocksAndStubs.LanguageService(),
                CommonMocksAndStubs.SettingsService(settingsModel));

            await notificationService.ShowNextNotification();

            Assert.IsNotNull(settingsModel.NotificationTriggers[0].ShownAt); // Marked as shown
            feedbackService.Verify(m => m.ShowMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButtons>(), It.IsAny<bool>()), Times.Once);
        }

        [TestMethod]
        public async Task ShowNextNotification_IgnoresInvalidTriggers()
        {
            // Prepare settings
            var settingsModel = new SettingsModel
            {
                TransferCode = "123456789",
                NotificationTriggers =
                {
                    new NotificationTriggerModel
                    {
                        Id = new Guid("fadd63ff-d305-491f-89bb-a65a8e6c4bfe"),
                        CreatedAt = DateTime.UtcNow.AddDays(-20), // first sync was 20 days ago
                    }
                }
            };

            Mock<IFeedbackService> feedbackService = new Mock<IFeedbackService>();
            INotificationService notificationService = new NotificationService(
                feedbackService.Object,
                CommonMocksAndStubs.LanguageService(),
                CommonMocksAndStubs.SettingsService(settingsModel));

            await notificationService.ShowNextNotification();

            Assert.IsNull(settingsModel.NotificationTriggers[0].ShownAt);
            feedbackService.Verify(m => m.ShowMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButtons>(), It.IsAny<bool>()), Times.Never);
        }
    }
}
