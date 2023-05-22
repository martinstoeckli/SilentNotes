// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="INavigationService"/> interface.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService"/> class.
        /// </summary>
        /// <param name="navigationManager">The applications navigation manager.</param>
        /// <param name="JSRuntime">The java script runtime.</param>
        public NavigationService(NavigationManager navigationManager, IJSRuntime JSRuntime)
        {
            _navigationManager = navigationManager;
            _jsRuntime = JSRuntime;
        }

        /// <inheritdoc/>
        public void NavigateTo(string uri)
        {
            _navigationManager.NavigateTo(uri);
        }

        /// <inheritdoc/>
        public async Task Back()
        {
            await _jsRuntime.InvokeVoidAsync("history.back");
        }
    }
}
