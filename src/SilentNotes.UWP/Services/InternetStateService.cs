// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Windows.Networking.Connectivity;
using SilentNotes.Services;

namespace SilentNotes.UWP.Services
{
    /// <summary>
    /// Implementation of the <see cref="IInternetStateService"/> interface for the UWP platform.
    /// </summary>
    internal class InternetStateService : IInternetStateService
    {
        /// <inheritdoc/>
        public bool IsInternetConnected()
        {
            NetworkConnectivityLevel? connectivity = NetworkInformation.GetInternetConnectionProfile()?.GetNetworkConnectivityLevel();
            return (connectivity != null) && (connectivity == NetworkConnectivityLevel.InternetAccess);
        }

        /// <inheritdoc/>
        public bool IsInternetCostFree()
        {
            ConnectionCost cost = NetworkInformation.GetInternetConnectionProfile()?.GetConnectionCost();
            return (cost != null) && (cost.NetworkCostType == NetworkCostType.Unrestricted);
        }
    }
}
