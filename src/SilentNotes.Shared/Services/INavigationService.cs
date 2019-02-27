// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

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
        /// <param name="controllerId">The id of the controller to navigate to.</param>
        /// <param name="variables">Optional collection of variables.</param>
        /// <exception cref="RoutingServiceRouteNotFoundException">Is thrown if no such route exists.</exception>
        void Navigate(string controllerId, KeyValueList<string, string> variables = null);

        /// <summary>
        /// Navigates to this controller. This is jus a convenience function for <see cref="Navigate(string, NameValueCollection)"/>,
        /// for controllers with a single variable.
        /// </summary>
        /// <param name="controllerId">The id of the controller to navigate to.</param>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="variableValue">Value of the variable.</param>
        /// <exception cref="RoutingServiceRouteNotFoundException">Is thrown if no such route exists.</exception>
        void Navigate(string controllerId, string variableName, string variableValue);

        /// <summary>
        /// Reloads the current page by navigating to the currently open controller again, using
        /// the same parameters as where used before.
        /// </summary>
        /// <param name="ifAnyOfThisControllers">The navigation is done if the <see cref="CurrentController"/>
        /// is one of the listed controllers, otherwise no action is taken.</param>
        void RepeatNavigationIf(IEnumerable<string> ifAnyOfThisControllers);

        /// <summary>
        /// Gets the currently active controller.
        /// </summary>
        IController CurrentController { get; }
    }
}
