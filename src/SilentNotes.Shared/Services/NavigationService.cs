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
        private string _currentControllerId;
        private KeyValueList<string, string> _currentVariables;

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

            // Setup new controller
            IController newController = Ioc.CreateWithKey<IController>(controllerId);
            CurrentController = newController;
            _currentControllerId = controllerId;
            _currentVariables = variables;
            newController.ShowInView(_htmlView, variables);
        }

        private void CleanupCurrentController()
        {
            IController oldController = CurrentController;
            CurrentController = null;
            _currentControllerId = null;
            _currentVariables = null;

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
            if ((CurrentController != null) && ifAnyOfThisControllers.Contains(_currentControllerId))
            {
                Navigate(_currentControllerId, _currentVariables);
            }
        }

        /// <inheritdoc/>
        public IController CurrentController { get; private set; }
    }
}
