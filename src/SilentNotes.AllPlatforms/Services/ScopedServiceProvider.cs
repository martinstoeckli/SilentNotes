using System;
using System.Collections.Generic;
using System.Text;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementaition of the <see cref="IScopedServiceProvider{T}"/> interface.
    /// </summary>
    /// <typeparam name="T">Type of the scoped object.</typeparam>
    public class ScopedServiceProvider<T> : IScopedServiceProvider<T> where T : class
    {
        private List<OwnerServicePair> _services;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopedServiceProvider{T}"/> instance.
        /// </summary>
        public ScopedServiceProvider()
        {
            _services = new List<OwnerServicePair>();
        }

        /// <summary>
        /// This event is called immediately before a new scoped object is registered.
        /// </summary>
        public event EventHandler<T> BeforeRegister;

        /// <inheritdoc/>
        public void Register(Guid owner, T scopedService)
        {
            OnBeforeRegister(scopedService);
            var item = new OwnerServicePair(owner, scopedService);
            _services.Add(item);
        }

        /// <inheritdoc/>
        public void Unregister(Guid owner)
        {
            _services.RemoveAll(item => item.Owner == owner);
        }

        /// <inheritdoc/>
        public T Get()
        {
            var item = _services.LastOrDefault();
            return (item != null) ? item.Service : null;
        }

        protected void OnBeforeRegister(T ownerServicePair)
        {
            BeforeRegister?.Invoke(this, ownerServicePair);
        }

        /// <summary>
        /// Key value pair containing the owner and the scoped service.
        /// </summary>
        private class OwnerServicePair
        {
            public OwnerServicePair(Guid owner, T service)
            {
                Owner = owner;
                Service = service;
            }

            public Guid Owner { get; }

            public T Service { get; }
        }
    }
}
