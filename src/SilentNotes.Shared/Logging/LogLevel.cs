// Copyright © 2019 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Logging
{
    /// <summary>
    /// Defines logging severity levels.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Log message that track the general flow of the application.
        /// </summary>
        Information = 2,

        /// <summary>
        /// Log message for an abnormal or unexpected event in the application flow, without
        /// stopping the application.
        /// </summary>
        Warning = 3,

        /// <summary>
        /// Log messages for errors when the execution cannot finish its job.
        /// </summary>
        Error = 4,
    }
}
