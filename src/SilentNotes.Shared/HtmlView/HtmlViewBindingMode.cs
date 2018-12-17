// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Linq;

namespace SilentNotes.HtmlView
{
    /// <summary>
    /// Defines the directions the data binding should work.
    /// </summary>
    public enum HtmlViewBindingMode
    {
        /// <summary>
        /// Calls an action in the viewmodel.
        /// </summary>
        Command,

        /// <summary>
        /// Changes in the viewmodel are updating the view, but not the other way. This is useful
        /// for displaying calculated results or control states.
        /// </summary>
        OneWayToView,

        /// <summary>
        /// Changes in the viewmodel are updating the view, but not the other way. This is useful
        /// for displaying calculated results or control states. Additionally the view is updated
        /// once before it is shown.
        /// </summary>
        OneWayToViewPlusOneTimeToView,

        /// <summary>
        /// Changes in the view are updating the viewmodel, but not the other way. This can be
        /// used, when the value is inserted directly into the razor template.
        /// </summary>
        OneWayToViewmodel,

        /// <summary>
        /// Changes in the viewmodel are updating the view and changes in the view are updating
        /// the viewmodel.
        /// </summary>
        TwoWay,

        /// <summary>
        /// Changes in the viewmodel are updating the view and changes in the view are updating
        /// the viewmodel. Additionally the view is updated once before it is shown.
        /// </summary>
        TwoWayPlusOneTimeToView
    }

    /// <summary>
    /// Extension methods for the <see cref="HtmlViewBindingMode"/> enumeration.
    /// </summary>
    public static class HtmlViewBindingModeExtensions
    {
        /// <summary>
        /// Checks whether an enum value matches one of the values in the <paramref name="range"/>.
        /// </summary>
        /// <param name="value">Value to test.</param>
        /// <param name="range">Range of valid values.</param>
        /// <returns>Returns true if the value is part of the range, otherwise false.</returns>
        public static bool In(this HtmlViewBindingMode value, IEnumerable<HtmlViewBindingMode> range)
        {
            if (range == null)
                return false;
            return range.Contains(value);
        }
    }
}
