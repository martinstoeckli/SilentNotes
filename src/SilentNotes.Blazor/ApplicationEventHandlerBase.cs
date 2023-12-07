// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using CommunityToolkit.Mvvm.Messaging;
using SilentNotes.Services;

namespace SilentNotes
{
    /// <summary>
    /// Base class for platform specific application event handlers.
    /// </summary>
    internal class ApplicationEventHandlerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationEventHandlerBase"/> class.
        /// </summary>
        public ApplicationEventHandlerBase()
        {
            WeakReferenceMessenger.Default.Register<MainLayoutReadyMessage>(
                this, async (recipient, message) => await OnMainLayoutReady());
        }

        /// <summary>
        /// Is called when the first rendering of the MainLayout has finished.
        /// At this moment the services are available form IOC, even if they are scoped services.
        /// </summary>
        /// <returns>Task for async calls.</returns>
        protected virtual async Task OnMainLayoutReady()
        {
            INotificationService notificationService = Ioc.Instance.GetService<INotificationService>();
            await notificationService.ShowNextNotification();

            // Do not await the synchronization, so it runs in the background.
            var synchronizationService = Ioc.Instance.GetService<ISynchronizationService>();
            _ = synchronizationService.AutoSynchronizeAtStartup(Ioc.Instance);
        }
    }
}
