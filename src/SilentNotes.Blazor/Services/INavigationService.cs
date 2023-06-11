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

        /// <inheritdoc/>
        void NavigateTo(string uri, bool forceLoad = false, bool replace = false);

        /// <summary>
        /// Reloads the current page by keeping the browser history intact.
        /// </summary>
        void Reload();
    }
}
