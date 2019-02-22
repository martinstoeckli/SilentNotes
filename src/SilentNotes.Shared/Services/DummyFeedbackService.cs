// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;

namespace SilentNotes.Services
{
    /// <summary>
    /// Dummy implementation of the <see cref="IFeedbackService"/> interface. This implementation
    /// provides no functionallity and can be used when all feedback should be ignored.
    /// </summary>
    public class DummyFeedbackService : IFeedbackService
    {
        private static readonly Task _completedTask = Task.FromResult(0);

        /// <inheritdoc/>
        public IDisposable ShowBusyIndicator()
        {
            return new BusyIndicatorHolder();
        }

        /// <inheritdoc/>
        public Task ShowMessageAsync(string message, string title)
        {
            return _completedTask;
        }

        /// <inheritdoc/>
        public void ShowToast(string message)
        {
        }

        /// <summary>
        /// Helper class which acts as result of the <see cref="ShowBusyIndicator"/> method.
        /// </summary>
        private class BusyIndicatorHolder : IDisposable
        {
            /// <inheritdoc/>
            public void Dispose()
            {
            }
        }
    }
}
