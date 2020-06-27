// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace SilentNotes.HtmlView
{
    /// <summary>
    /// Event arguments for the <see cref="VueDataBinding.UnhandledViewBindingEvent"/>.
    /// </summary>
    public class VueBindingUnhandledViewBindingEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VueBindingUnhandledViewBindingEventArgs"/> class.
        /// </summary>
        /// <param name="propertyName">Sets the <see cref="PropertyName"/>.</param>
        /// <param name="value">Sets the <see cref="Value"/>.</param>
        public VueBindingUnhandledViewBindingEventArgs(string propertyName, string value)
        {
            PropertyName = propertyName;
            Value = value;
        }

        /// <summary>Gets the name of the unknown property.</summary>
        public string PropertyName { get; }

        /// <summary>Gets the value which was passed from the Html view.</summary>
        public string Value { get; }
    }
}
