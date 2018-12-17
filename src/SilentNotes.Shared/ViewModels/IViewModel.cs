// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// The viewmodel interacts with a model and provides all necessary methods and properties used
    /// by the view.
    /// </summary>
    public interface IViewModel
    {
        /// <summary>
        /// This method is called when the controller of the viewmodel is disposed.
        /// </summary>
        void OnClosing();

        /// <summary>
        /// This method can be used to make changes persistent. It is usually called when the view
        /// is closed, but can be called in other situations as well.
        /// </summary>
        void OnStoringUnsavedData();

        /// <summary>
        /// This function is called when the user pressed the go-back key. Not every platform has
        /// such a go-back system key, mobile devices usually have. All but the main viewmodel
        /// shold handle this request and return true, returning false will result in closing the
        /// application.
        /// </summary>
        /// <param name="handled">The viewmodel can set this parameter to true to signal that
        /// the event is handled by the viewmodel, instead of doing the system default action
        /// which closes the application.</param>
        void OnGoBackPressed(out bool handled);
    }
}
