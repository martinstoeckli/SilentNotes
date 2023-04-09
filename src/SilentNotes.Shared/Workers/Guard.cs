// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Workers
{
    /// <summary>
    /// A guard can be places in a "using" statement, to guarantee that a given action is executed,
    /// as soon as the guard is disposed at the and of the "using" scope. This also includes cases
    /// when an exception is thrown or if the code is exited with a return.
    /// <example><code>
    /// Cursor.Current = Cursors.WaitCursor;
    /// using (var cursorGuard = new Guard(() => Cursor.Current = Cursors.Default))
    /// {
    /// }
    /// </code></example>
    /// </summary>
    public class Guard : IDisposable
    {
        private readonly Action _endAction;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="Guard"/> class.
        /// </summary>
        /// <param name="endAction"></param>
        public Guard(Action endAction)
        {
            _endAction = endAction;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Guard"/> class.
        /// </summary>
        ~Guard()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Calls the end action once when the guard is disposed.
        /// </summary>
        /// <param name="disposing">Value indicating whether the method was called from
        /// IDisposable.Dispose method (true) or from the finalizer (false).</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
                _endAction();
            }
        }
    }
}
