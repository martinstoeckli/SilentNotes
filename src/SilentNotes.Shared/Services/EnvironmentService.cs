// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="IEnvironmentService"/> interface.
    /// </summary>
    public class EnvironmentService : IEnvironmentService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentService"/> class.
        /// </summary>
        /// <param name="os">Sets the <see cref="Os"/> property.</param>
        public EnvironmentService(OperatingSystem os)
        {
            Os = os;
        }

        /// <inheritdoc/>
        public OperatingSystem Os { get; }
    }
}
