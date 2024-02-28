// Copyright © 2024 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Text;
using Microsoft.Extensions.Logging;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the ILogger interface.
    /// This logger should not be used in published versions of SilentNotes, its purpose is to
    /// detect exceptions in test versions (flight packages), thus it logs immediately to a file.
    /// </summary>
    public class LoggerService : ILogger
    {
        private StringBuilder _lines;
        private string _logFilePath;

        public LoggerService(string logFilePath)
        {
            _lines = new StringBuilder();
            _logFilePath = logFilePath;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return default!;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return (logLevel == LogLevel.Error) || (logLevel == LogLevel.Critical);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            _lines.Clear();
            _lines.AppendLine();
            _lines.Append(DateTime.Now.ToString("o"));
            _lines.Append(" ");
            _lines.AppendLine(logLevel.ToString());

            if (exception != null)
            {
                _lines.AppendLine(exception.Message);
                _lines.AppendLine(exception.StackTrace);
            }
            File.AppendAllText(_logFilePath, _lines.ToString());
        }
    }
}
