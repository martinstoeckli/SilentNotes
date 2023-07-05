// Copyright © 2022 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace SilentNotes.Services
{
    /// <summary>
    /// Allows to register more than one implementation of the same interface in the IOC framework.
    /// </summary>
    /// <typeparam name="TServiceInterface">Common interface of the registered services.</typeparam>
    /// <typeparam name="TKey">A key to distinguish the services.</typeparam>
    public class ServiceFactory<TKey, TServiceInterface> where TServiceInterface : class
    {
        private Dictionary<TKey, Func<TServiceInterface>> _factoryFunctions;
        private Dictionary<TKey, TServiceInterface> _cachedSingletons;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceFactory{TKey, TServiceInterface}"/> class.
        /// </summary>
        /// <param name="createAsSingletons">Sets the <see cref="CreateAsSingletons"/> property.</param>
        public ServiceFactory(bool createAsSingletons)
        {
            CreateAsSingletons = createAsSingletons;

            IEqualityComparer<TKey> comparer = (typeof(TKey) == typeof(string))
                ? comparer = (IEqualityComparer<TKey>)StringComparer.InvariantCultureIgnoreCase
                : EqualityComparer<TKey>.Default;

            _factoryFunctions = new Dictionary<TKey, Func<TServiceInterface>>(comparer);
            _cachedSingletons = CreateAsSingletons ? new Dictionary<TKey, TServiceInterface>() : null;
        }

        /// <summary>
        /// Gets a value indicating whether services should be created as singletons and be kept in
        /// a cache.
        /// </summary>
        public bool CreateAsSingletons { get; }

        /// <summary>
        /// Registers a given instance for a given key.
        /// </summary>
        /// <param name="key">The key which identifies the instance.</param>
        /// <param name="factoryFunction">The factory function is able to create the instance that
        /// must be returned when the given type is resolved.</param>
        public void Add(TKey key, Func<TServiceInterface> factoryFunction)
        {
            _factoryFunctions.Add(key, factoryFunction);
        }

        /// <summary>
        /// Gets or creates an instance of a given type, registered with a given key.
        /// If no instance had been instantiated before, a new instance will be created.
        /// If an instance had already been created, that same instance will be returned.
        /// </summary>
        /// <param name="key">The key uniquely identifying this instance.</param>
        /// <returns>An instance of the type of the factory.</returns>
        public TServiceInterface GetByKey(TKey key)
        {
            TServiceInterface result = null;
            if (CreateAsSingletons && _cachedSingletons.TryGetValue(key, out result))
                return result;

            try
            {
                Func<TServiceInterface> factoryFunction = _factoryFunctions[key];
                result = factoryFunction();
                if (CreateAsSingletons)
                    _cachedSingletons.Add(key, result);
                return result;
            }
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException(string.Format("An instance of the interface [{0}] for key [{1}] could not be created.", typeof(TServiceInterface).Name, key.ToString()), ex);
            }
        }
    }
}
