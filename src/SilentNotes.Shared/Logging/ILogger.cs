// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Logging
{
    /// <summary>
    /// The log service can be used to collect log messages, which 
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Writes a log message.
        /// </summary>
        /// <param name="level">The severity level of the message.</param>
        /// <param name="message">The log message.</param>
        void Log(LogLevel level, string message);

        /// <summary>
        /// Writes a log message.
        /// </summary>
        /// <remarks>
        /// This method does not throw formatting exceptions, because logging should be no source
        /// of errors in the application.
        /// </remarks>
        /// <param name="level">The severity level of the message.</param>
        /// <param name="messageFormat">The log message as format string.</param>
        /// <param name="args">The arguments which can be used to format the message.</param>
        void LogFormat(LogLevel level, string messageFormat, params object[] args);

        /// <summary>
        /// Writes a log message.
        /// </summary>
        /// <param name="level">The severity level of the message.</param>
        /// <param name="exception">The exception to log.</param>
        void LogException(LogLevel level, Exception exception);
    }
}
