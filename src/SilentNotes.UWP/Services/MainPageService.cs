// Copyright © 2022 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Windows.UI.Xaml.Controls;

namespace SilentNotes.UWP.Services
{
    /// <summary>
    /// This service provides the main page of the application.
    /// </summary>
    public class MainPageService
    {
        /// <summary>
        /// Gets or sets the main page.
        /// This property can be set not only at construction time, but also later. This way the
        /// service/IOC can be built at startup and populated later when the main page is known.
        /// </summary>
        public MainPage MainPage { get; set; }
    }
}
