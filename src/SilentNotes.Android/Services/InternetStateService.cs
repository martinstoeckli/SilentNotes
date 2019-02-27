// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Android.Content;
using Android.Net;
using Android.OS;
using SilentNotes.Services;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// Implementation of the <see cref="IInternetStateService"/> interface for the Android platform.
    /// </summary>
    public class InternetStateService : IInternetStateService
    {
        private readonly Context _applicationContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternetStateService"/> class.
        /// </summary>
        /// <param name="applicationContext">The Android application context.</param>
        public InternetStateService(Context applicationContext)
        {
            _applicationContext = applicationContext;
        }

        /// <inheritdoc/>
        public bool IsInternetConnected()
        {
            ConnectivityManager connectivity = GetConnectivityManager();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                NetworkCapabilities capabilities = connectivity.GetNetworkCapabilities(connectivity.ActiveNetwork);
                return (capabilities != null) && capabilities.HasCapability(NetCapability.Internet);
            }

            NetworkInfo info = connectivity.ActiveNetworkInfo;
            return (info != null) && (info.IsConnected);
        }

        /// <inheritdoc/>
        public bool IsInternetCostFree()
        {
            ConnectivityManager connectivity = GetConnectivityManager();
            return !connectivity.IsActiveNetworkMetered;
        }

        private ConnectivityManager GetConnectivityManager()
        {
            return (ConnectivityManager)_applicationContext.GetSystemService(Context.ConnectivityService);
        }
    }
}