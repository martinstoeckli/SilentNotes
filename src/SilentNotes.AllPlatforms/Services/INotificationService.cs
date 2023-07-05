// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using SilentNotes.Models;

namespace SilentNotes.Services
{
    /// <summary>
    /// The notification service can show messages at startup time of the app.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Shows the next notification in the queue. Maximum one notification per startup is shown.
        /// </summary>
        /// <returns>Task for async calls.</returns>
        Task ShowNextNotification();
    }
}
