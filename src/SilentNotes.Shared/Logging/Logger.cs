// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SilentNotes.Logging
{
    /// <summary>
    /// Implementation of the <see cref="ILogger"/> interface.
    /// </summary>
    public class Logger : ILogger, ILogReader
    {
        private const string FormatErrorMsg = "Exception formatting the log message: '{0}'";
        private const string ExceptionErrorMsg = "Exception was thrown but could not be logged";
        private readonly TimeSpan _maxAge;
        private readonly List<LogEntry> _logEntries;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="maxAge">Maximum age a message can have, before it will be deleted.</param>
        public Logger(TimeSpan maxAge)
        {
            _maxAge = maxAge;
            _logEntries = new List<LogEntry>();
        }

        /// <inheritdoc/>
        public void Log(LogLevel level, string message)
        {
            DeleteExpiredLogEntries();
            _logEntries.Add(new LogEntry { Level = level, Message = message });
        }

        /// <inheritdoc/>
        public void LogFormat(LogLevel level, string messageFormat, params object[] args)
        {
            try
            {
                string message = string.Format(messageFormat, args);
                Log(level, message);
            }
            catch (Exception)
            {
                string message = string.Format(FormatErrorMsg, messageFormat);
                Log(level, message);
            }
        }

        /// <inheritdoc/>
        public void LogException(LogLevel level, Exception exception)
        {
            try
            {
                string message = exception.ToString();
                Log(level, message);
            }
            catch (Exception)
            {
                Log(level, ExceptionErrorMsg);
            }
        }

        /// <inheritdoc/>
        public IEnumerable<LogEntry> GetLastLogEntries(LogLevel minLevel)
        {
            DeleteExpiredLogEntries();
            return _logEntries.Where(logEntry => logEntry.Level >= minLevel);
        }

        private void DeleteExpiredLogEntries()
        {
            DateTime threshold = DateTime.UtcNow - _maxAge;
            _logEntries.RemoveAll(logEntry => logEntry.CreationTime < threshold);
        }
    }
}
