// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using GalaSoft.MvvmLight.Ioc;

namespace SilentNotes
{
    /// <summary>
    /// This static class is a wrapper around an external IOC framework. Its usage allows to
    /// exchange the framework, and offers only the subset of functions which is really used.
    /// </summary>
    public static class Ioc
    {
        /// <summary>
        /// Gets or creates an instance of a given type.
        /// If no instance had been instantiated before, a new instance will be created.
        /// If an instance had already been created, that same instance will be returned.
        /// </summary>
        /// <typeparam name="T">The class of the instance we are interested in.</typeparam>
        /// <returns>An instance of the given type.</returns>
        public static T GetOrCreate<T>()
        {
            return SimpleIoc.Default.GetInstance<T>();
        }

        /// <summary>
        /// Gets or creates an instance of a given type, registered with a given key.
        /// If no instance had been instantiated before, a new instance will be created.
        /// If an instance had already been created, that same instance will be returned.
        /// </summary>
        /// <typeparam name="T">The class of the instance we are interested in.</typeparam>
        /// <param name="key">The key uniquely identifying this instance.</param>
        /// <returns>An instance of the given type.</returns>
        public static T GetOrCreateWithKey<T>(string key)
        {
            return SimpleIoc.Default.GetInstance<T>(key);
        }

        /// <summary>
        /// Creates an new instance of a given type, registered with a given key.
        /// Even if an instance had already been created, a new instance will be returned.
        /// </summary>
        /// <typeparam name="T">The class of the instance we are interested in.</typeparam>
        /// <param name="key">The key uniquely identifying this instance.</param>
        /// <returns>An instance of the given type.</returns>
        public static T CreateWithKey<T>(string key)
        {
            return SimpleIoc.Default.GetInstanceWithoutCaching<T>(key);
        }

        /// <summary>
        /// Registers a given instance for a given type.
        /// </summary>
        /// <typeparam name="T">The interface/type for which instances will be resolved.</typeparam>
        /// <param name="factory">The factory method able to create the instance that
        /// must be returned when the given type is resolved.</param>
        public static void RegisterFactory<T>(Func<T> factory) where T : class
        {
            SimpleIoc.Default.Register<T>(factory);
        }

        /// <summary>
        /// Registers a given instance for a given type, registered with a given key.
        /// </summary>
        /// <typeparam name="T">The type for which instances will be resolved.</typeparam>
        /// <param name="key">The key uniquely identifying this instance.</param>
        /// <param name="factory">The factory method able to create the instance that
        /// must be returned when the given type is resolved.</param>
        public static void RegisterFactoryWithKey<T>(string key, Func<T> factory) where T : class
        {
            SimpleIoc.Default.Register<T>(factory, key);
        }

        /// <summary>
        /// Checks whether a given type is already registered or not.
        /// </summary>
        /// <typeparam name="T">The type to search for.</typeparam>
        /// <returns>Returns true if the type is already registered, otherwiese false.</returns>
        public static bool IsRegistered<T>() where T : class
        {
            return SimpleIoc.Default.IsRegistered<T>();
        }

        /// <summary>
        /// Resets all instances and removes all registrations.
        /// </summary>
        public static void Reset()
        {
            SimpleIoc.Default.Reset();
        }
    }
}
