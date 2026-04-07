using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using AvaloniaCrossTest.Services;
using MudBlazor;
using SilentNotes.Services;

namespace SilentNotesAvalonia.Desktop.Services
{
    internal class FeedbackService : IFeedbackService
    {
        private readonly IMainWindowProvider _mainWindowProvider;
        private WindowNotificationManager _notificationManager;

        public FeedbackService(IMainWindowProvider mainWindowProvider)
        {
            _mainWindowProvider = mainWindowProvider;
        }

        public Task<MessageBoxResult> ShowMessageAsync(string message, string title, MessageBoxButtons buttons, bool conservativeDefault)
        {
            return Task.FromResult(MessageBoxResult.Ok);
        }

        public void ShowToast(string message, Severity severity = Severity.Normal)
        {
            var notificationManager = GetOrCreateNotificationManager();
            _notificationManager.Show(new Notification(
                null,
                message,
                NotificationType.Success,
                TimeSpan.FromSeconds(5)));
        }

        private WindowNotificationManager GetOrCreateNotificationManager()
        {
            if (_notificationManager == null)
            {
                _notificationManager = new WindowNotificationManager(_mainWindowProvider.MainWindow)
                {
                    Position = NotificationPosition.BottomCenter,
                    MaxItems = 3,
                };
            }
            return _notificationManager;
        }
    }
}
