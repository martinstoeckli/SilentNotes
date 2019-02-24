// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// Provides information about the current internet connection.
    /// </summary>
    public interface IInternetStateService
    {
        /// <summary>
        /// Checks whether an internet connection is currently accessible by the device.
        /// </summary>
        /// <returns>Returns true if an internet connection is available, otherwise false.</returns>
        bool IsInternetConnected();

        /// <summary>
        /// Checks whether the internet connection can be used without any costs.
        /// </summary>
        /// <returns>Returns true if the internet connection is for free, false if this is a
        /// rated connection.</returns>
        bool IsInternetCostFree();
    }
}
