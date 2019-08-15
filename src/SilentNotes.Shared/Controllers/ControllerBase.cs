// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.HtmlView;
using SilentNotes.Services;
using SilentNotes.ViewModels;

namespace SilentNotes.Controllers
{
    /// <summary>
    /// Abstract base class for all controllers.
    /// </summary>
    public abstract class ControllerBase : IController
    {
        private bool _disposed = false;

        /// <summary>Gets the injected view service.</summary>
        protected readonly IRazorViewService _viewService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ControllerBase"/> class.
        /// </summary>
        /// <param name="viewService">Razor view service which can generate the HTML view.</param>
        protected ControllerBase(IRazorViewService viewService)
        {
            _viewService = viewService;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ControllerBase"/> class.
        /// </summary>
        ~ControllerBase()
        {
            if (!_disposed)
                OverrideableDispose();
            _disposed = true;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
                OverrideableDispose();
            _disposed = true;
        }

        /// <summary>
        /// Release unused resources and unsubscribe from events. This method must work correctly,
        /// even if called multiple times.
        /// </summary>
        protected virtual void OverrideableDispose()
        {
            // This unsubscribes events form the view.
            Bindings?.Dispose();
            Bindings = null;

            GetViewModel()?.OnClosing();
        }

        /// <summary>
        /// Gets the html view or null if not yet set.
        /// </summary>
        protected IHtmlView View { get; private set; }

        /// <summary>
        /// Gets the viewmodel interface.
        /// </summary>
        /// <returns>The view model.</returns>
        protected abstract IViewModel GetViewModel();

        /// <summary>
        /// Gets a list of bindings or null if not yet set.
        /// </summary>
        protected HtmlViewBindings Bindings { get; private set; }

        /// <inheritdoc/>
        public virtual void ShowInView(IHtmlView htmlView, KeyValueList<string, string> variables)
        {
            View = htmlView;
            Bindings?.Dispose();
            Bindings = new HtmlViewBindings(htmlView);
        }

        /// <inheritdoc/>
        public virtual void StoreUnsavedData()
        {
            GetViewModel()?.OnStoringUnsavedData();
        }

        /// <inheritdoc/>
        public void OnGoBackPressed(out bool handled)
        {
            handled = true;
            GetViewModel()?.OnGoBackPressed(out handled);
        }
    }
}
