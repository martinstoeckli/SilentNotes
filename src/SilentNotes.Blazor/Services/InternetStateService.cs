// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Microsoft.Maui.Networking;
using SilentNotes.Services;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="IInternetStateService"/> interface for the Android platform.
    /// </summary>
    internal class InternetStateService : IInternetStateService
    {
        /// <inheritdoc/>
        public bool IsInternetConnected()
        {
            return Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        }

        /// <inheritdoc/>
        public bool IsInternetCostFree()
        {
            var profiles = Connectivity.Current.ConnectionProfiles;
            if (profiles.Contains(ConnectionProfile.Cellular))
                return profiles.Contains(ConnectionProfile.WiFi) || profiles.Contains(ConnectionProfile.Ethernet);
            return true;
        }
    }
}