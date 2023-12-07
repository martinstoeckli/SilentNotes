// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SilentNotes.Models;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="INotificationService"/> interface.
    /// </summary>
    public class NotificationService : INotificationService
    {
        public static readonly Guid TransferCodeNotificationId = new Guid("cf497f24-61a0-4ddd-8af8-76faa59c6eff");

        private readonly IFeedbackService _feedbackService;
        private readonly ILanguageService _languageService;
        private readonly ISettingsService _settingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService"/> class.
        /// </summary>
        /// <param name="feedbackService">A feedback service from IOC.</param>
        /// <param name="languageService">A language service from IOC.</param>
        /// <param name="settingsService">A settings service from IOC.</param>
        public NotificationService(IFeedbackService feedbackService, ILanguageService languageService, ISettingsService settingsService)
        {
            _feedbackService = feedbackService;
            _languageService = languageService;
            _settingsService = settingsService;

            // Build list of known notifications
            Notifications = new List<Notification>
            {
                new Notification
                {
                    Id = TransferCodeNotificationId,
                    Message = _languageService.LoadTextFmt("transfer_code_notification", _languageService.LoadText("show_transfer_code")),
                    QueueTime = TimeSpan.FromDays(5),
                }
            };
        }

        /// <summary>
        /// Gets a list of known notifications.
        /// </summary>
        private List<Notification> Notifications { get; }

        /// <inheritdoc/>
        public async Task ShowNextNotification()
        {
            SettingsModel settings = _settingsService.LoadSettingsOrDefault();
            DateTime now = DateTime.UtcNow;

            AutoAddNotifications(settings);

            foreach (var notification in Notifications)
            {
                NotificationTriggerModel trigger = settings.NotificationTriggers.Find(item => item.Id == notification.Id);
                if ((trigger != null) && trigger.IsDue(now, notification.QueueTime))
                {
                    trigger.ShownAt = now;
                    _settingsService.TrySaveSettingsToLocalDevice(settings);
                    await _feedbackService.ShowMessageAsync(notification.Message, string.Empty, MessageBoxButtons.Ok, true);
                    break; // Show only one notification per startup
                }
            }
        }

        /// <summary>
        /// Automatically adds notification triggers to the settings. This allows to add the
        /// trigger for the <see cref="TransferCodeNotificationId"/>, even if the transfercode was
        /// created before notifications where available.
        /// </summary>
        /// <param name="settings">The currently loaded settings.</param>
        private void AutoAddNotifications(SettingsModel settings)
        {
            NotificationTriggerModel foundTrigger = settings.NotificationTriggers.Find(item => item.Id == TransferCodeNotificationId);
            if ((foundTrigger == null) && (settings.HasTransferCode))
            {
                NotificationTriggerModel trigger = new NotificationTriggerModel { Id = TransferCodeNotificationId };
                settings.NotificationTriggers.Add(trigger);
                _settingsService.TrySaveSettingsToLocalDevice(settings);
            }
        }

        /// <summary>
        /// Describes a single known notification.
        /// </summary>
        private class Notification
        {
            /// <summary>Gets or sets the id of the notification.</summary>
            public Guid Id { get; set; }

            /// <summary>Gets or sets the already translated message which of the notification.</summary>
            public string Message { get; set; }

            /// <summary>
            /// Gets or sets the timespan to wait until the notification is shown to the
            /// user. Example: A remember transfercode notification could be shown 5 days after the
            /// first synchronization.
            /// </summary>
            public TimeSpan QueueTime { get; set; }
        }
    }
}
