// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SilentNotes.Services
{
    /// <summary>
    /// Encapsulates and extends the <see cref="NavigationManager"/> of the app.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigates to the specified URI.
        /// </summary>
        /// <param name="uri">The destination URI. This can be absolute, or relative to the base URI
        /// (as returned by <see cref="BaseUri"/>).</param>
        void NavigateTo(string uri);

        /// <summary>
        /// Navigates one step backwards in the browser history.
        /// </summary>
        /// <returns>A task for async calls.</returns>
        Task Back();
    }
}