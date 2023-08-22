// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.Services;

namespace SilentNotes.Platforms.Services
{
    /// <summary>
    /// Implementation of the <see cref="IEnvironmentService"/> for the UWP platform.
    /// </summary>
    internal class EnvironmentService : IEnvironmentService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentService"/> class.
        /// </summary>
        public EnvironmentService()
        {
            Os = SilentNotes.Services.OperatingSystem.Windows;

            // Detect dark mode already in the constructor, because the RequestedTheme can be
            // overwritten at startup.
            InDarkMode = Application.Current.RequestedTheme == AppTheme.Dark;
        }

        /// <inheritdoc/>
        public SilentNotes.Services.OperatingSystem Os { get; }

        /// <inheritdoc/>
        public bool InDarkMode { get; }

        /// <inheritdoc/>
        public IKeepScreenOn KeepScreenOn
        {
            get { return null; }
        }

        /// <inheritdoc/>
        public IScreenshots Screenshots
        {
            get { return null; }
        }
    }
}
