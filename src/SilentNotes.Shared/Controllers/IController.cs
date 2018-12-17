// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using SilentNotes.HtmlView;

namespace SilentNotes.Controllers
{
    /// <summary>
    /// A controller is a mediator between an (Html)view and its viewmodel. It takes care of
    /// data binding and other user interactions.
    /// </summary>
    public interface IController : IDisposable
    {
        /// <summary>
        /// Shows the content of a viewmodel in the (Html)view.
        /// </summary>
        /// <param name="htmlView">Interface to a WebView control which will dislay the Html.</param>
        /// <param name="variables">Collection of routing variables.</param>
        void ShowInView(IHtmlView htmlView, KeyValueList<string, string> variables);

        /// <summary>
        /// This method can be used to make changes persistent. It is usually called when the view
        /// is closed.
        /// </summary>
        void StoreUnsavedData();

        /// <summary>
        /// This function is called when the user pressed the go-back key. Not every platform has
        /// such a go-back system key, mobile devices usually have. All but the main viewmodel
        /// shold handle this request and return true, returning false will result in closing the
        /// application.
        /// </summary>
        /// <param name="handled">The controller can set this parameter to true to signal that
        /// the event is handled by the controller, instead of doing the system default action
        /// which closes the application.</param>
        void OnGoBackPressed(out bool handled);
    }
}
