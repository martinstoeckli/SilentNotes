// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;

namespace SilentNotes.Services
{
    /// <summary>
    /// The browser history service can be used to controllable browser history list for the whole
    /// lifetime of the app.
    /// </summary>
    public interface IBrowserHistoryService
    {
        /// <summary>
        /// Gets the Uri of the current route.
        /// </summary>
        string CurrentLocation { get; }

        /// <summary>
        /// Gets the Uri of the last route.
        /// </summary>
        string LastLocation { get; }

        /// <summary>
        /// Should be called whenever the navigation service gets a navigation event.
        /// </summary>
        /// <param name="targetLocation">The new target location. The browser history service will
        /// decide itself whether this is a backwards or forwards navigation, or a refresh of the
        /// current page.</param>
        /// <param name="baseUri">The base Uri of the NavigationManager.</param>
        void UpdateHistoryOnNavigation(string targetLocation, string baseUri);

        /// <summary>
        /// Clears the whole history.
        /// </summary>
        void Clear();

        /// <summary>
        /// Removes/forgets the last entry of the history.
        /// </summary>
        void RemoveCurrent();
    }
}
