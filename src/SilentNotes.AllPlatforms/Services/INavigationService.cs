// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using Microsoft.AspNetCore.Components;

namespace SilentNotes.Services
{
    /// <summary>
    /// Abstraction interface for the <see cref="NavigationManager"/>. It adds tighter control
    /// about the browser history which is required for native apps.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigates to the specified URI.
        /// </summary>
        /// <param name="uri">The destination URI. This can be absolute, or relative to the base URI.</param>
        /// <param name="reload">If true, the page will surely be refreshed, even if it is the same
        /// as the current page.</param>
        void NavigateTo(string uri, bool reload = false);

        /// <summary>
        /// Reloads the current page. A reload doesn't trigger the OnStoreUnsavedData(), it
        /// forcefully refreshes the current page, loosing the current modifications.
        /// </summary>
        void NavigateReload();
    }
}
