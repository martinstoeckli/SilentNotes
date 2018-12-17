// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.ViewModels;

namespace SilentNotes.Services
{
    /// <summary>
    /// Allows to work with razor views, which must be hosted in the platform specific part of the
    /// application. With this interface, the razor views can be loaded with Ioc.
    /// </summary>
    public interface IRazorViewService
    {
        /// <summary>
        /// Generates the HTML page so it can be shown in the WebView of the application view.
        /// </summary>
        /// <param name="viewmodel">The viewmodel which should be bound to the razor view.</param>
        /// <returns>HTML code.</returns>
        string GenerateHtml(ViewModelBase viewmodel);
    }
}
