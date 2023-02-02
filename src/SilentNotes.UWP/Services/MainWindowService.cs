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
    internal interface IMainWindowService
    {
        /// <summary>
        /// Initializes the service, this is usually done when the main page is created.
        /// </summary>
        /// <param name="mainPage">Sets the <see cref="MainPage"/> property.</param>
        void Initialize(MainPage mainPage);

        /// <summary>
        /// Gets the main page.
        /// This property can be set not only at construction time, but also later. This way the
        /// service/IOC can be built at startup and populated later when the main page is known.
        /// </summary>
        MainPage MainPage { get; }
    }

    /// <summary>
    /// Implementation of the <see cref="IMainWindowService"/> interface.
    /// </summary>
    internal class MainWindowService: IMainWindowService
    {
        /// <inheritdoc/>
        public void Initialize(MainPage mainPage)
        {
            MainPage = mainPage;
        }

        /// <inheritdoc/>
        public MainPage MainPage { get; private set; }
    }
}
