using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// Most services solve a platform dependend problem. Their interface and sometimes their
    /// base implementation is placed in this namespace, while their implementation can be found
    /// in the platform specific projects.
    /// The services will be registered in the IOC and make the application platform independend
    /// and mockable.
    /// </summary>
    internal class NamespaceDoc
    {
    }
}
