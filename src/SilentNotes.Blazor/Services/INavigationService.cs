// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace SilentNotes.Services
{
    /// <summary>
    /// Abstraction interface for the <see cref="NavigationManager"/>.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Initializes app specific virtual routes, e.g. the "back" route.
        /// </summary>
        void InitializeVirtualRoutes();

        /// <summary>
        /// Navigates to the specified URI.
        /// </summary>
        /// <param name="uri">The destination URI. This can be absolute, or relative to the base URI.</param>
        /// <param name="forceLoad">This parameter is not used for the hybrid application, but we
        /// keep the parameters the same as with the NavigationManager.</param>
        /// <param name="replace">If true, replaces the current entry in the history stack. If false,
        /// appends the new entry to the history stack.</param>
        void NavigateTo(string uri, bool forceLoad = false, bool replace = false);

        /// <summary>
        /// Navigates one step back in the history. The browser history is adjusted accordingly.
        /// </summary>
        void NavigateBack();

        /// <summary>
        /// Reloads the current page by keeping the browser history intact.
        /// </summary>
        void Reload();
    }
}
