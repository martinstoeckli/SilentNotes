// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SilentNotes.Models
{
    /// <summary>
    /// List of safes.
    /// </summary>
    public class SafeListModel : List<SafeModel>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SafeListModel"/> class.
        /// </summary>
        public SafeListModel()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SafeListModel"/> class.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        public SafeListModel(IEnumerable<SafeModel> collection)
            : base(collection)
        {
        }

        /// <summary>
        /// Searches for a safe with a given id in the list and returns the found safe.
        /// </summary>
        /// <param name="id">Search for the safe with this id.</param>
        /// <returns>Found safe, or null if no such safe exists.</returns>
        public SafeModel FindById(Guid? id)
        {
            if (id == null)
                return null;
            return Find(item => item.Id == id);
        }

        /// <summary>
        /// Searches for the oldest open safe.
        /// </summary>
        /// <returns>Returns the found safe, or null if no safes are open.</returns>
        public SafeModel FindOldestOpenSafe()
        {
            SafeModel result = null;
            foreach (SafeModel openSafe in this.Where(item => item.IsOpen))
            {
                if (result == null)
                    result = openSafe;
                else if (openSafe.CreatedAt < result.CreatedAt)
                    result = openSafe;
            }
            return result;
        }
    }
}
