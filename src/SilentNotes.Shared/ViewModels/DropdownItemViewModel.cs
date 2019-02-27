// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// Describes a single entry of a dropdown combobox
    /// </summary>
    public class DropdownItemViewModel
    {
        /// <summary>
        /// Gets or sets the value of the item.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the description of the item.
        /// </summary>
        public string Description { get; set; }
    }
}
