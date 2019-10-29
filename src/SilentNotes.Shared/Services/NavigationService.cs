// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using SilentNotes.Controllers;
using SilentNotes.HtmlView;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="INavigationService"/> interface.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly IHtmlView _htmlView;

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
            CleanupCurrentController();
            CurrentNavigation = new Navigation { ControllerId = controllerId, Variables = variables };

            // Setup new controller
            CurrentController = Ioc.CreateWithKey<IController>(CurrentNavigation.ControllerId);
            CurrentController.ShowInView(_htmlView, CurrentNavigation.Variables);
        }

        private void CleanupCurrentController()
        {
            IController oldController = CurrentController;
            CurrentController = null;
            CurrentNavigation = null;

            // Clean up old controller
            if (oldController != null)
            {
                oldController.StoreUnsavedData();
                oldController.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Navigate(string controllerId, string variableName, string variableValue)
        {
            var variables = new KeyValueList<string, string>(StringComparer.InvariantCultureIgnoreCase);
            variables[variableName] = variableValue;
            Navigate(controllerId, variables);
        }

        /// <inheritdoc/>
        public void RepeatNavigationIf(IEnumerable<string> ifAnyOfThisControllers)
        {
            if ((CurrentNavigation != null) && ifAnyOfThisControllers.Contains(CurrentNavigation.ControllerId))
            {
                Navigate(CurrentNavigation.ControllerId, CurrentNavigation.Variables);
            }
        }

        /// <inheritdoc/>
        public Navigation CurrentNavigation { get; private set; }

        /// <inheritdoc/>
        public IController CurrentController { get; private set; }
    }
}
