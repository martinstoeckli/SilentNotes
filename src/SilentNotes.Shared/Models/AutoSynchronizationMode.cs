// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Models
{
    /// <summary>
    /// Enumeration of the modes for auto synchronization withe the online storage.
    /// </summary>
    public enum AutoSynchronizationMode
    {
        /// <summary>
        /// No automatic synchronization, only manual synchronization
        /// </summary>
        Never,

        /// <summary>
        /// Auto sync when a cost free internet connection is available, otherwise no auto sync.
        /// </summary>
        CostFreeInternetOnly,

        /// <summary>
        /// Always auto synchronize if internet is available.
        /// </summary>
        Always
    }
}
