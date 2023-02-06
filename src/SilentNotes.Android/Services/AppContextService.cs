// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Android.App;
using Android.Content;
using SilentNotes.HtmlView;
using SilentNotes.Services;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// A service which knows the current main activity. The main activity can change in the
    /// lifetime of the app, thus we need a source from where other services can dynamically
    /// get the application context.
    /// </summary>
    /// <remarks>
    /// After quitting the application with the back key, the main activity can be recreated
    /// while the application is still the same. Thus the services must get the main window
    /// dynamically from the <see cref="IAppContextService"/> and should not store a reference
    /// to it.
    /// </remarks>
    internal interface IAppContextService
    {
        /// <summary>
        /// Initializes the service, this is usually done when the root activity is created.
        /// </summary>
        /// <param name="rootActivity">Sets the <see cref="RootActivity"/> property.</param>
        void Initialize(Activity rootActivity);

        /// <summary>
        /// Gets the root activity which started/restarted the app.
        /// </summary>
        Activity RootActivity { get; }
    }

    /// <summary>
    /// Implementation of the <see cref="IAppContextService"/> interface.
    /// </summary>
    internal class AppContextService : IAppContextService, IHtmlViewService
    {
        /// <inheritdoc/>
        public void Initialize(Activity rootActivity)
        {
            RootActivity = rootActivity;
        }

        /// <inheritdoc/>
        public Activity RootActivity { get; private set; }

        /// <inheritdoc/>
        public IHtmlView HtmlView 
        {
            get { return RootActivity as IHtmlView; }
        }
    }
}