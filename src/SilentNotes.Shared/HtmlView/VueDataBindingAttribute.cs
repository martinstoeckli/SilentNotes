// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.HtmlView
{
    /// <summary>
    /// Indicates that the property should be used for data binding with the HTML view.
    /// The property name must match the Vue tag.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class VueDataBindingAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VueDataBindingAttribute"/> class.
        /// </summary>
        /// <param name="bindingMode">Sets the <see cref="BindingMode"/>.</param>
        public VueDataBindingAttribute(VueBindingMode bindingMode)
        {
            BindingMode = bindingMode;
        }

        /// <summary>
        /// Gets or sets the mode of the binding, which determines the direction of the data binding.
        /// </summary>
        public VueBindingMode BindingMode { get; set; }
    }
}
