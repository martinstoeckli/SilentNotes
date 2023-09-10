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
        /// <param name="historyModification">Specifies how the browser history changes by this
        /// navigation.</param>
        void NavigateTo(string uri, HistoryModification historyModification = HistoryModification.Add);

        /// <summary>
        /// Gets a value indicating whether there exists a previous location in the browser history.
        /// </summary>
        bool CanNavigateBack {  get; }

        /// <summary>
        /// Navigates one step back in the history. The browser history is adjusted accordingly.
        /// </summary>
        /// <returns>Task for async calling.</returns>
        void NavigateBack();

        /// <summary>
        /// Navigates back in the history to the first step. The browser history is cleared.
        /// </summary>
        /// <returns>Task for async calling.</returns>
        void NavigateHome();

        /// <summary>
        /// Reloads the current page by keeping the browser history intact.
        /// </summary>
        void Reload();
    }

    /// <summary>
    /// Enumeration of the possible browser history modifications.
    /// </summary>
    public enum HistoryModification
    {
        /// <summary>The new page is added to the browser history.</summary>
        Add,

        /// <summary>The last entry of the browser history is removed and replaced with the new page,
        /// so that a following backwards navigation won't show the current page again.</summary>
        ReplaceCurrent,
    }
}
