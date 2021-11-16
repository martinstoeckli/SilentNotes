// Copyright © 2021 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SilentNotes.Services
{
    /// <summary>
    /// Interface of a service which can load/safe files from/to the locale device.
    /// The file must be picked manually by the user, this grants the necessary privileges,
    /// which must otherwise be declared in the application manifest.
    /// </summary>
    public interface IFilePickerService
    {
        /// <summary>
        /// Lets the user pick a file for reading/writing with a system dialog.
        /// </summary>
        /// <returns>Returns true if the user picked a file, false if the pick dialog was
        /// canceled.</returns>
        Task<bool> PickFile();

        /// <summary>
        /// Tries to read a formerly picked file, call this method only after <see cref="PickFile"/>
        /// returned true.
        /// </summary>
        /// <returns>Returns the content of the read file.</returns>
        Task<byte[]> ReadPickedFile();
    }
}
