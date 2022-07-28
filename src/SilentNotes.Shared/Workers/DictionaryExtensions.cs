// Copyright © 2022 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Extension methods for dictionaries.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets the key associated with the specified value. This is the opposite of <see cref="Dictionary{TKey, TValue}.TryGetValue(TKey, out TValue)"/>
        /// and therefore cannot provide the performance of the latter.
        /// If multiple entries exists with the same value, it is random which key is returned.
        /// </summary>
        /// <typeparam name="TKey">The type of the dictionary keys.</typeparam>
        /// <typeparam name="TValue">The type of the dictionary values.</typeparam>
        /// <param name="dictionary">The dictionary to search.</param>
        /// <param name="value">The value whose key we are looking for.</param>
        /// <param name="key">Retrieves the key if a matching value was found, otherwise the default value.</param>
        /// <param name="comparer">Optional value comparer, if null is passed the types default
        /// comparer will be used.</param>
        /// <returns>Returns true if the value was found, otherwise true.</returns>
        public static bool TryGetKey<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TValue value, out TKey key, IEqualityComparer<TValue> comparer = null)
        {
            if (dictionary == null)
                throw new ArgumentNullException(nameof(dictionary));

            IEqualityComparer<TValue> equalityComparer = comparer ?? EqualityComparer<TValue>.Default;

            foreach (var item in dictionary)
            {
                if (equalityComparer.Equals(value, item.Value))
                {
                    key = item.Key;
                    return true;
                }
            }
            key = default(TKey);
            return false;
        }
    }
}
