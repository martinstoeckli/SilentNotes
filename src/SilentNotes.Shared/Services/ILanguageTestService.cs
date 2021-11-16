// Copyright © 2021 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;

namespace SilentNotes.Services
{
    /// <summary>
    /// A language service can support this interface, so the app can show a preview of a user
    /// written language resource files. Since Android and UWP do not allow to access the executable
    /// directory, a user cannot place his own file there, so we provide another way to test the file.
    /// </summary>
    public interface ILanguageTestService
    {
        /// <summary>
        /// Overrides the language resources of the original language with resources from a user
        /// written file. If the file contains resources which do not exist in the original language
        /// they are ignored. If the file is missing resources of the original language, the original
        /// resources will be displayed instead.
        /// </summary>
        /// <param name="customResourceFile">The content of a user written language resource file.</param>
        void OverrideWithTestResourceFile(byte[] customResourceFile);
    }
}
