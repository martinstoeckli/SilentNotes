// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;
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
        /// <param name="removeCurrentFromHistory">If true, the navigation will remove the current
        /// entry in the history before doing the navigation.</param>
        void NavigateTo(string uri, bool removeCurrentFromHistory = false);

        /// <summary>
        /// Gets a value indicating whether there exists a previous location in the browser history.
        /// </summary>
        bool CanNavigateBack {  get; }

        /// <summary>
        /// Navigates one step back in the history. The browser history is adjusted accordingly.
        /// </summary>
        void NavigateBack();

        /// <summary>
        /// Reloads the current page. A reload doesn't trigger the OnStoreUnsavedData(), it
        /// forcefully refreshes the current page, loosing the current modifications.
        /// </summary>
        void NavigateReload();

        /// <summary>
        /// Navigates back in the history to the first step. The browser history is cleared.
        /// </summary>
        void NavigateHome();
    }
}
