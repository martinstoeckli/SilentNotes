// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SilentNotes.Services;

namespace SilentNotes.Android.Services
{
    /// <summary>
    /// Implementation of the <see cref="IFolderPickerService"/> interface for the Android platform.
    /// </summary>
    public class FolderPickerService : IFolderPickerService
    {
        /// <inheritdoc/>
        public async Task<bool> PickFolder()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<bool> TrySaveFileToFolder(string relativeFilePath, byte[] content)
        {
            throw new NotImplementedException();
        }
    }
}