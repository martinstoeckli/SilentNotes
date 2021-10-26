// Copyright © 2021 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Services
{
    /// <summary>
    /// The IHtmlRecourceService provides version independend paths to given recources like
    /// *.js or *.css files. It can also be used to exchange minified versions to debuggable
    /// versions.
    /// </summary>
    public interface IHtmlRecourceService
    {
        /// <summary>
        /// Indexer property which returns the version independend path to the resource.
        /// </summary>
        /// <param name="id">Id of the recource.</param>
        /// <returns>A path to the recource.</returns>
        string this[string id] { get; }
    }
}
