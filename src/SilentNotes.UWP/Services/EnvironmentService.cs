// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.Services;
using Windows.UI.Xaml;

namespace SilentNotes.UWP.Services
{
    /// <summary>
    /// Implementation of the <see cref="IEnvironmentService"/> for the UWP platform.
    /// </summary>
    public class EnvironmentService : IEnvironmentService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentService"/> class.
        /// </summary>
        /// <param name="os">Sets the <see cref="Os"/> property.</param>
        public EnvironmentService(OperatingSystem os)
        {
            Os = os;

            // Detect dark mode already in the constructor, because the RequestedTheme can be
            // overwritten at startup.
            InDarkMode = Application.Current.RequestedTheme == ApplicationTheme.Dark;
        }

        /// <inheritdoc/>
        public OperatingSystem Os { get; }

        /// <inheritdoc/>
        public bool InDarkMode { get; }
    }
}
