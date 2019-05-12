// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace SilentNotes.Logging
{
    /// <summary>
    /// Reads the messages logged with the <see cref="ILogger"/>.
    /// </summary>
    public interface ILogReader
    {
        /// <summary>
        /// Gets a list of the most recent log entries.
        /// </summary>
        /// <param name="minLevel">Only log entries of this level or higher are returned.</param>
        /// <returns>List of log entries.</returns>
        IEnumerable<LogEntry> GetLastLogEntries(LogLevel minLevel);
    }
}
