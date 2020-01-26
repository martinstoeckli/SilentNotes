// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Android.Content;
using Android.Content.Res;
using SilentNotes.Services;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// Implementation of the <see cref="IEnvironmentService"/> for the Android platform.
    /// </summary>
    public class EnvironmentService : IEnvironmentService
    {
        private readonly Context _applicationContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentService"/> class.
        /// </summary>
        /// <param name="os">Sets the <see cref="Os"/> property.</param>
        /// <param name="applicationContext">The Android application context.</param>
        public EnvironmentService(OperatingSystem os, Context applicationContext)
        {
            Os = os;
            _applicationContext = applicationContext;
        }

        /// <inheritdoc/>
        public OperatingSystem Os { get; private set; }

        /// <inheritdoc/>
        public bool InDarkMode
        {
            get
            {
                UiMode nightModeFlags = _applicationContext.Resources.Configuration.UiMode & UiMode.NightMask;
                return nightModeFlags == UiMode.NightYes;
            }
        }
    }
}