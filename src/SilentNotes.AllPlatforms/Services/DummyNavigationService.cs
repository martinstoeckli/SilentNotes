// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace SilentNotes.Services
{
    /// <summary>
    /// Dummy implementation of the <see cref="INavigationService"/> interface. This implementation
    /// provides no functionallity and can be used when all navigation should be ignored.
    /// </summary>
    public class DummyNavigationService : INavigationService
    {
        /// <inheritdoc/>
        public void InitializeVirtualRoutes()
        {
        }

        /// <inheritdoc/>
        public void NavigateTo(string uri, bool removeCurrentFromHistory = false)
        {
        }

        /// <inheritdoc/>
        public bool CanNavigateBack { get; }

        /// <inheritdoc/>
        public void NavigateBack()
        {
        }

        /// <inheritdoc/>
        public void NavigateReload()
        {
        }

        /// <inheritdoc/>
        public void NavigateHome()
        {
        }
    }
}
