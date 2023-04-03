// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Android.Content;
using SilentNotes.Services;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// Implementation of the <see cref="IVersionService"/> interface for the Android platform.
    /// </summary>
    internal class VersionService : IVersionService
    {
        private readonly IAppContextService _appContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="VersionService"/> class.
        /// </summary>
        /// <param name="appContextService">A service which knows about the current main activity.</param>
        public VersionService(IAppContextService appContextService)
        {
            _appContext = appContextService;
        }

        /// <inheritdoc/>
        public string GetApplicationVersion(string format = "{0}.{1}.{2}")
        {
            try
            {
                // Android does not support 4 digit versions, instead one can read the version string.
                Context context = _appContext.Context;
                return context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionName;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}