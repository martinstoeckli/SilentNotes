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
        public void Navigate(Navigation navigateTo)
        {
            CleanupCurrentController();

            Navigation redirectedFrom = null;
            CurrentNavigation = navigateTo;
            CurrentController = Ioc.CreateWithKey<IController>(CurrentNavigation.ControllerId);

            // Check if a redirection is necessary
            if (CurrentController.NeedsNavigationRedirect(CurrentNavigation, out Navigation redirectTo))
            {
                redirectedFrom = CurrentNavigation;
                CurrentNavigation = redirectTo;
                CurrentController = Ioc.CreateWithKey<IController>(CurrentNavigation.ControllerId);
            }
            CurrentController.ShowInView(_htmlView, CurrentNavigation.Variables, redirectedFrom);
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
        public void RepeatNavigationIf(IEnumerable<string> ifAnyOfThisControllers)
        {
            if ((CurrentNavigation != null) && ifAnyOfThisControllers.Contains(CurrentNavigation.ControllerId))
            {
                Navigate(CurrentNavigation);
            }
        }

        /// <inheritdoc/>
        public Navigation CurrentNavigation { get; private set; }

        /// <inheritdoc/>
        public IController CurrentController { get; private set; }
    }
}
