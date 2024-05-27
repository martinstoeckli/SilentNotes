// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using Microsoft.Extensions.DependencyInjection;

namespace SilentNotes
{
    /// <summary>
    /// Global Ioc service provider.
    /// <example><code>
    /// var myService = Ioc.Instance.GetService&lt;IMyService&gt;();
    /// </code></example>
    /// </summary>
    internal class Ioc : IServiceProvider
    {
        private IServiceProvider _serviceProvider;

        /// <summary>
        /// Gets a static instance of the Ioc service provider.
        /// </summary>
        public static Ioc Instance { get; } = new Ioc();

        /// <summary>
        /// The application can register its service provider to make it globally available.
        /// </summary>
        /// <param name="serviceProvider">The apps service provider.</param>
        public void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public object GetService(Type serviceType)
        {
            if (_serviceProvider == null)
                throw new Exception("Ioc is not initialized.");
            return _serviceProvider.GetService(serviceType);
        }

        /// <summary>
        /// Generic version of <see cref="GetService(Type)"/>.
        /// </summary>
        /// <typeparam name="T">The requested service type.</typeparam>
        /// <returns>A service object of type serviceType.</returns>
        public T GetService<T>() where T : class
        {
            return (T)GetService(typeof(T));
        }
    }
}
