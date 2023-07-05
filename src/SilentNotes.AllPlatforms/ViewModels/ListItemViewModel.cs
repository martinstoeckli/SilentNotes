// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// A ViewModel for an item of a MudList.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="Value"/> property.</typeparam>
    public class ListItemViewModel<T>
    {
        /// <summary>
        /// Gets or sets the value of the list item.
        /// </summary>
        public T Value { get; set; }

        /// <summary>
        /// Gets or sets the readable localized text of the list item.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the name of the icon. This should be a member of the <see cref="IconName"/>
        /// class, or null if no icon should be displayed.
        /// </summary>
        public string IconName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this list item is a divider/separator. If so,
        /// all other properties are ignored.
        /// </summary>
        public bool IsDivider { get; set; }
    }
}
