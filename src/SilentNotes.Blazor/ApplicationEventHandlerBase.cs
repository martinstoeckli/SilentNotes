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
            WeakReferenceMessenger.Default.Register<ApplicationReady>(
                this, async (recipient, message) => await OnApplicationReady());
        }

        /// <summary>
        /// Is called when the first rendering of the MainLayout has finished.
        /// </summary>
        /// <returns>Task for async calls.</returns>
        protected virtual async Task OnApplicationReady()
        {
            INotificationService notificationService = Ioc.Instance.GetService<INotificationService>();
            await notificationService.ShowNextNotification();

            var synchronizationService = Ioc.Instance.GetService<ISynchronizationService>();
            synchronizationService.SynchronizeAtStartup(Ioc.Instance);
        }
    }
}
