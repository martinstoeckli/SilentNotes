// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Reflection;
using SilentNotes.Services;

namespace SilentNotes.UWP.Services
{
    /// <summary>
    /// Implementation of the <see cref="IVersionService"/> interface for the UWP platform.
    /// </summary>
    public class VersionService : IVersionService
    {
        /// <inheritdoc/>
        public string GetApplicationVersion(string format = "{0}.{1}.{2}")
        {
            Version version = typeof(VersionService).GetTypeInfo().Assembly.GetName().Version;
            try
            {
                return string.Format(format, version.Major, version.Minor, version.Build, version.Revision);
            }
            catch
            {
                return string.Format("{0}.{1}.{2}", version.Major, version.Minor, version.MajorRevision);
            }
        }
    }
}
