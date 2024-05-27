using System;
using System.Collections.Generic;
using System.Text;

namespace SilentNotes.Services
{
    /// <summary>
    /// Some services like the IJSRuntime or the Android app-context are scoped, they are not
    /// always available in the lifecycle of the app and can change. This provider allows to
    /// declare depending services as singletons in the IOC, but to exchange certain scoped objects
    /// at runtime.
    /// <example>
    /// The IJSRuntime is not available before the page OnAfterRender(), there the page can
    /// register the IJSRuntime and other services can get the current runtime from this provider.
    /// </example>
    /// </summary>
    /// <remarks>
    /// This provider can handle the situations, where no scoped object is currently registered,
    /// or when a new scoped object is registered before the old one is unregistered. Both
    /// situations are common for the lifetime of blazor pages.
    /// </remarks>
    /// <typeparam name="T">Type of the scoped object.</typeparam>
    public interface IScopedServiceProvider<T> where T : class
    {
        /// <summary>
        /// Registers the scoped object. This should be called when a new scoped object is ready to
        /// be used, afterwards the registered object is used by depending services.
        /// </summary>
        /// <param name="owner">The id of the owner who registered the service.</param>
        /// <param name="scopedService">The scoped object to register.</param>
        void Register(Guid owner, T scopedService);

        /// <summary>
        /// Removes all registered scoped objects of this owner. This should be called when scoped
        /// objects shouldn't be used anymore.
        /// </summary>
        /// <param name="owner">All registered scoped objects formerly registered by this owner
        /// will be removed.</param>
        void Unregister(Guid owner);

        /// <summary>
        /// Removes all registered scoped objects. This can be called when the application is
        /// restarted (Android OnDestroy/OnCreate).
        /// </summary>
        void UnregisterAll();

        /// <summary>
        /// Gets the most current registered scoped object.
        /// </summary>
        /// <returns>Currently registered scoped object, or null if no such object is reistered at
        /// this moment.</returns>
        T GetScopedService();

        /// <summary>
        /// This event is called immediately before a new scoped object is registered. If an object
        /// is already registered, it can be gotten by <see cref="GetScopedService"/>.
        /// </summary>
        event EventHandler<T> BeforeRegister;
    }
}
