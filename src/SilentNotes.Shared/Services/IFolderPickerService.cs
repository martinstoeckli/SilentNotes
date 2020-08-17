// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Threading.Tasks;

namespace SilentNotes.Services
{
    /// <summary>
    /// Interface of a service which can load/safe files from/to a folder of the locale device.
    /// The folder must be picked manually by the user, this grants the necessary privileges,
    /// which must otherwise be declared in the application manifest.
    /// </summary>
    public interface IFolderPickerService
    {
        /// <summary>
        /// Lets the user pick a folder for reading/writing with a system dialog.
        /// </summary>
        /// <returns>Returns true if the user picked a folder, false if the pick dialog was
        /// canceled.</returns>
        Task<bool> PickFolder();

        /// <summary>
        /// Tries to safe a file to the formerly picked folder, see also <see cref="PickFolder"/>.
        /// </summary>
        /// <param name="relativeFilePath">The file path relative to the picked folder.</param>
        /// <param name="content">The file content to store.</param>
        /// <returns>Returns true if the file could be stored, otherwise false.</returns>
        Task<bool> TrySaveFileToFolder(string relativeFilePath, byte[] content);
    }
}
