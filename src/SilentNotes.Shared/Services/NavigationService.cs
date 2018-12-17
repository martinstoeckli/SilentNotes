// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using SilentNotes.Controllers;
using SilentNotes.HtmlView;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="INavigationService"/> interface.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private IHtmlView _htmlView;

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationService"/> class.
        /// </summary>
        /// <param name="htmlView">WebView control of the main page.</param>
        public NavigationService(IHtmlView htmlView)
        {
            _htmlView = htmlView;
        }

        /// <inheritdoc/>
        public virtual void Navigate(string controllerId, KeyValueList<string, string> variables = null)
        {
            IController oldController = CurrentController;
            CurrentController = null;

            // Clean up old controller
            if (oldController != null)
            {
                oldController.StoreUnsavedData();
                oldController.Dispose();
            }

            IController newController = Ioc.CreateWithKey<IController>(controllerId);
            CurrentController = newController;

            // Setup new controller
            newController.ShowInView(_htmlView, variables);
        }

        /// <inheritdoc/>
        public void Navigate(string controllerId, string variableName, string variableValue)
        {
            var variables = new KeyValueList<string, string>(StringComparer.InvariantCultureIgnoreCase);
            variables[variableName] = variableValue;
            Navigate(controllerId, variables);
        }

        /// <inheritdoc/>
        public IController CurrentController { get; private set; }
    }
}
