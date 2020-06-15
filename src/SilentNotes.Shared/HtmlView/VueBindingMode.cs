// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.HtmlView
{
    /// <summary>
    /// Defines the directions the data binding should work.
    /// </summary>
    public enum VueBindingMode
    {
        /// <summary>
        /// Changes in the viewmodel are updating the view and changes in the view are updating
        /// the viewmodel.
        /// </summary>
        TwoWay,

        /// <summary>
        /// Changes in the viewmodel are updating the view, but not the other way. This is useful
        /// for displaying calculated results or control states.
        /// </summary>
        OneWayToView,

        /// <summary>
        /// Executes an <see cref="ICommand"/>.
        /// </summary>
        Command,
    }
}
