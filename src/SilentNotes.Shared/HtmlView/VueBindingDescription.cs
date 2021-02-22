// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;

namespace SilentNotes.HtmlView
{
    /// <summary>
    /// All necessary information to do a binding to a property of a viewmodel.
    /// </summary>
    public class VueBindingDescription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VueBindingDescription"/> class.
        /// </summary>
        /// <param name="propertyName">Sets the <see cref="PropertyName"/>.</param>
        /// <param name="bindingMode">Sets the <see cref="BindingMode"/>.</param>
        public VueBindingDescription(string propertyName, VueBindingMode bindingMode)
        {
            PropertyName = propertyName;
            BindingMode = bindingMode;
        }

        /// <summary>Gets the name of the property, the binding belongs to.</summary>
        public string PropertyName { get; private set; }

        /// <summary>Gets the mode ot the binding.</summary>
        public VueBindingMode BindingMode { get; private set; }
    }

    /// <summary>
    /// List of <see cref="VueBindingDescription"/> objects.
    /// </summary>
    public class VueBindingDescriptions : List<VueBindingDescription>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VueBindingDescriptions"/> class.
        /// </summary>
        public VueBindingDescriptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VueBindingDescriptions"/> class.
        /// </summary>
        /// <param name="descriptions">Enumeration of descriptions which are applied to the list.</param>
        public VueBindingDescriptions(IEnumerable<VueBindingDescription> descriptions)
            : base(descriptions)
        {
        }

        /// <summary>
        /// Searches the list for an element with this property name and returns the first found
        /// element.
        /// </summary>
        /// <param name="propertyName">Name of the property to search for.</param>
        /// <returns>First found element or null if no such element was found.</returns>
        public VueBindingDescription FindByPropertyName(string propertyName)
        {
            return Find(item => string.Equals(propertyName, item.PropertyName, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
