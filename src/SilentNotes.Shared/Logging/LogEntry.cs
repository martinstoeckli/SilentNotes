// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Logging
{
    /// <summary>
    /// A single log entry.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Initializes a new instance of the LogEntry class.
        /// </summary>
        public LogEntry()
        {
            CreationTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Gets or sets the creation time (UTC) of the log entry.
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Gets or sets the severity level of the log entry.
        /// </summary>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Gets or sets the message of the log entry.
        /// </summary>
        public string Message { get; set; }
    }
}
