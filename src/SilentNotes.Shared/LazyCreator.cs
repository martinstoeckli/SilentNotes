// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the license was not distributed with this
// file, You can obtain one at https://opensource.org/licenses/MIT.

using System;

namespace SilentNotes
{
    /// <summary>
    /// Can create member variables/properties on demand. This is especially useful for model
    /// classes, so they can guarantee existing property instances, but do not create unnecessary
    /// instances before the model is deserialized from a config.
    /// </summary>
    public static class LazyCreator
    {
        /// <summary>
        /// When <paramref name="memberVariable"/> is null, an instance will be created, before it
        /// is returned as result. This allows for lazy creation of properties, e.g. if the
        /// property is a list.
        /// <example>
        /// Example declaration of a list property.
        /// <code>
        ///   public List Items
        ///   {
        ///     get { return LazyCreator.GetOrCreate(ref this.items); }
        ///     set { this.items = value; }
        ///   }
        /// </code></example>
        /// </summary>
        /// <typeparam name="T">Type of the member variable/property.</typeparam>
        /// <param name="memberVariable">Member of the class, which is associated with the property.</param>
        /// <returns>The already existing or new created property member.</returns>
        public static T GetOrCreate<T>(ref T memberVariable) where T : new()
        {
            if (memberVariable == null)
                memberVariable = new T();
            return memberVariable;
        }

        /// <summary>
        /// When <paramref name="memberVariable"/> is null, an instance will be created, before it
        /// is returned as result. This allows for lazy creation of properties, e.g. if the
        /// property is a list.
        /// <example>
        /// Example declaration of a case insensitive dictionary property.
        /// <code>
        ///   public List&lt;string$gt; Items
        ///   {
        ///     get { LazyCreator.GetOrCreate(ref memberVariable, () =&gt; new Dictionary&lt;string, string&gt;(StringComparer.InvariantCultureIgnoreCase)); }
        ///   }
        /// </code></example>
        /// </summary>
        /// <typeparam name="T">Type of the member variable/property.</typeparam>
        /// <param name="memberVariable">Member of the class, which is associated with the property.</param>
        /// <param name="creator">Delegate which can create a new instance of the member variable.</param>
        /// <returns>The already existing or new created property member.</returns>
        public static T GetOrCreate<T>(ref T memberVariable, Func<T> creator)
        {
            if (memberVariable == null)
                memberVariable = creator();
            return memberVariable;
        }
    }
}
