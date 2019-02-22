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
