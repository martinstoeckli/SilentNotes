// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the license was not distributed with this
// file, You can obtain one at https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;

namespace SilentNotes.Workers
{
    /// <summary>
    /// This list contains key-value pairs and offers handling like a NameValueCollection. In
    /// contrast to a NameValueCollection/dictionary, this list is serializeable, has a well
    /// defined order of elements and can be enumerated, though it is not optimized for speed.
    /// Possible usages are reading/writing of config files, list properties inside serializeable
    /// model classes, or data-binding to keyed items of the list.
    /// </summary>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    public class KeyValueList<TKey, TValue> : List<KeyValueList<TKey, TValue>.Pair>
    {
        private readonly IEqualityComparer<TKey> _keyComparer;
        private readonly IEqualityComparer<TValue> _valueComparer;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueList{TKey, TValue}"/> class.
        /// </summary>
        public KeyValueList()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueList{TKey, TValue}"/> class.
        /// To create a case insensitive list, we could write code like this:
        /// <example><code>
        /// var list = new KeyValueList&lt;string, string&gt;(StringComparer.InvariantCultureIgnoreCase);
        /// </code></example>
        /// </summary>
        /// <param name="keyComparer">User defined comparer, or null to use the default
        /// comparer of this datatype. The comparer is used for searching by the left element.
        /// This allows e.g. for case insensitive searching.</param>
        public KeyValueList(IEqualityComparer<TKey> keyComparer)
            : this(keyComparer, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyValueList{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="keyComparer">User defined comparer, or null to use the default
        /// comparer of this datatype. The comparer is used for searching by the left element.
        /// This allows e.g. for case insensitive searching.</param>
        /// <param name="valueComparer">User defined comparer, or null to use the default
        /// comparer of this datatype. The comparer is used for searching by the right element.
        /// This allows e.g. for case insensitive searching.</param>
        public KeyValueList(IEqualityComparer<TKey> keyComparer, IEqualityComparer<TValue> valueComparer)
        {
            _keyComparer = keyComparer ?? EqualityComparer<TKey>.Default;
            _valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;
        }

        /// <summary>
        /// A serializable key-value pair for the <see cref="KeyValueList{TKey,TValue}"/>.
        /// </summary>
        [DebuggerDisplay("{Key} = {Value}")]
        public class Pair
        {
            /// <summary>
            /// Gets or sets the key of the pair, which can be searched for.
            /// </summary>
            [XmlElement(ElementName = "key")]
            public TKey Key { get; set; }

            /// <summary>
            /// Gets or sets the value belonging to the key.
            /// </summary>
            [XmlElement(ElementName = "value")]
            public TValue Value { get; set; }
        }

        /// <summary>
        /// Gets or sets the value for a given key. This property can be used for data-binding (WPF).
        /// If the key cannot be found in the list, the null indicator of the value is returned when reading.
        /// If the key already exists in the list, the value of the pair will be overwritten when writing.
        /// </summary>
        /// <param name="key">The key which is used to search the list.</param>
        /// <returns>Found value or its null representation</returns>
        public TValue this[TKey key]
        {
            get { return GetValueOrDefault(key); }
            set { AddOrReplace(key, value); }
        }

        /// <summary>
        /// Gets the key-value pair with a given index. This is a replacement of the indexed
        /// property to get the pair at a given index, e.g. for enumerations.
        /// </summary>
        /// <param name="index">Zero based index of the list.</param>
        /// <returns>Key-value pair at the given index.</returns>
        public Pair GetByIndex(int index)
        {
            return this[index];
        }

        /// <summary>
        /// Adds a new key-value pair, or if the list already contains a pair with this key,
        /// replaces its value.
        /// </summary>
        /// <param name="key">Key to search for.</param>
        /// <param name="value">New value to set.</param>
        public void AddOrReplace(TKey key, TValue value)
        {
            Pair item = FindByKey(key);
            if (item != null)
                item.Value = value;
            else
                Add(new Pair { Key = key, Value = value });
        }

        /// <summary>
        /// Searches for the first key-value pair in the list with a given key, and returns its value.
        /// </summary>
        /// <param name="key">Key to search for.</param>
        /// <returns>Value of the found key-value pair or its null representation, if no such key
        /// was found.</returns>
        public TValue GetValueOrDefault(TKey key)
        {
            TryGetValue(key, out TValue value);
            return value;
        }

        /// <summary>
        /// Tries to find the first key-value pair in the list with a given key, and returns its value.
        /// </summary>
        /// <param name="key">Key to search for.</param>
        /// <param name="value">Retrieves the value of the found key-value pair or its null
        /// representation, if no such key was found.</param>
        /// <returns>Returns true if the key was found, otherwise false.</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            Pair item = FindByKey(key);
            if (item != null)
            {
                value = item.Value;
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        /// <summary>
        /// Tries to find the first key-value pair in the list with a given value, and returns its key.
        /// </summary>
        /// <param name="value">Value to search for.</param>
        /// <param name="key">Retrieves the key of the found key-value pair or its null
        /// representation, if no such key was found.</param>
        /// <returns>Returns true if the value was found, otherwise false.</returns>
        public bool TryGetKey(TValue value, out TKey key)
        {
            Pair item = FindByValue(value);
            if (item != null)
            {
                key = item.Key;
                return true;
            }
            else
            {
                key = default(TKey);
                return false;
            }
        }

        /// <summary>
        /// Checks whether the list contains a pair with a given key.
        /// </summary>
        /// <param name="key">Key to search for.</param>
        /// <returns>Returns true if the key exists, otherwise false.</returns>
        public bool ContainsKey(TKey key)
        {
            return FindByKey(key) != null;
        }

        /// <summary>
        /// Removes the item with a given <paramref name="key"/> from the list. If several items
        /// have the same key, all of those items are removed.
        /// </summary>
        /// <param name="key">Key to search for.</param>
        public void RemoveByKey(TKey key)
        {
            RemoveAll((item) => _keyComparer.Equals(key, item.Key));
        }

        /// <summary>
        /// Searches for the first key-value pair in the list, with a given key.
        /// </summary>
        /// <param name="key">Key to search for.</param>
        /// <returns>Found key-value pair or null if no such key was found.</returns>
        protected Pair FindByKey(TKey key)
        {
            return this.FirstOrDefault(item => _keyComparer.Equals(key, item.Key));
        }

        /// <summary>
        /// Searches for the first key-value pair in the list, with a given value.
        /// </summary>
        /// <param name="key">Key to search for.</param>
        /// <returns>Found key-value pair or null if no such key was found.</returns>
        protected Pair FindByValue(TValue value)
        {
            return this.FirstOrDefault(item => _valueComparer.Equals(value, item.Value));
        }
    }
}
