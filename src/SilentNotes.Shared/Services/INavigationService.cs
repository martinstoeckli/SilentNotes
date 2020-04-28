// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using SilentNotes.Controllers;

namespace SilentNotes.Services
{
    /// <summary>
    /// The navigation service allows to navigate between different pages/views.
    /// In contrast to the Xamarin INavigation interface, it allows to navigate with routes instead
    /// of views. This allows to do the navigation inside the ViewModels instead of the Views.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Navigates to this controller.
        /// </summary>
        /// <param name="navigateTo">Navigates to this target.</param>
        /// <exception cref="RoutingServiceRouteNotFoundException">Is thrown if no such route exists.</exception>
        void Navigate(Navigation navigateTo);

        /// <summary>
        /// Reloads the current page by navigating to the currently open controller again, using
        /// the same parameters as where used before.
        /// </summary>
        /// <param name="ifAnyOfThisControllers">The navigation is done if the <see cref="CurrentController"/>
        /// is one of the listed controllers, otherwise no action is taken.</param>
        void RepeatNavigationIf(IEnumerable<string> ifAnyOfThisControllers);

        /// <summary>
        /// Gets the parameters of the currently active navigation.
        /// </summary>
        Navigation CurrentNavigation { get; }

        /// <summary>
        /// Gets the currently active controller.
        /// </summary>
        IController CurrentController { get; }
    }

    /// <summary>
    /// Describes all parameters necessary to do a navigation.
    /// </summary>
    public class Navigation
    {
        private KeyValueList<string, string> _variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="Navigation"/> class.
        /// </summary>
        public Navigation()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Navigation"/> class.
        /// </summary>
        /// <param name="controllerId">Sets the <see cref="ControllerId"/>.</param>
        public Navigation(string controllerId)
        {
            ControllerId = controllerId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Navigation"/> class.
        /// </summary>
        /// <param name="controllerId">Sets the <see cref="ControllerId"/>.</param>
        /// <param name="variableName">Add a variable with this name.</param>
        /// <param name="variableValue">Adds a variable with this value.</param>
        public Navigation(string controllerId, string variableName, string variableValue)
        {
            ControllerId = controllerId;
            Variables[variableName] = variableValue;
        }

        /// <summary>
        /// Gets or sets the id of the controller.
        /// </summary>
        public string ControllerId { get; set; }

        /// <summary>
        /// Gets or sets an optional collection of variables for the navigation.
        /// </summary>
        public KeyValueList<string, string> Variables
        {
            get { return _variables ?? (_variables = new KeyValueList<string, string>(StringComparer.InvariantCultureIgnoreCase)); }
            set { _variables = value; } 
        }
    }
}
