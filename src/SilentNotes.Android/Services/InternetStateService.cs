// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Android.Content;
using Android.Net;
using SilentNotes.Services;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// Implementation of the <see cref="IInternetStateService"/> interface for the Android platform.
    /// </summary>
    internal class InternetStateService : IInternetStateService
    {
        private readonly IAppContextService _appContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternetStateService"/> class.
        /// </summary>
        /// <param name="appContextService">A service which knows about the current main activity.</param>
        public InternetStateService(IAppContextService appContextService)
        {
            _appContext = appContextService;
        }

        /// <inheritdoc/>
        public bool IsInternetConnected()
        {
            ConnectivityManager connectivity = GetConnectivityManager();

            // With Build.VERSION.SdkInt < BuildVersionCodes.M we would have to use an alternative
            // way to check, but Android 6 is our min version.
            NetworkCapabilities capabilities = connectivity.GetNetworkCapabilities(connectivity.ActiveNetwork);
            return (capabilities != null) && capabilities.HasCapability(NetCapability.Internet);
        }

        /// <inheritdoc/>
        public bool IsInternetCostFree()
        {
            ConnectivityManager connectivity = GetConnectivityManager();
            return !connectivity.IsActiveNetworkMetered;
        }

        private ConnectivityManager GetConnectivityManager()
        {
            return (ConnectivityManager)_appContext.RootActivity.GetSystemService(Context.ConnectivityService);
        }
    }
}