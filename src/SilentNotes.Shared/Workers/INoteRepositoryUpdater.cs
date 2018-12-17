// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Xml.Linq;

namespace SilentNotes.Workers
{
    /// <summary>
    /// This repository updater can update older versions of a note repository, to the current
    /// supported version.
    /// </summary>
    public interface INoteRepositoryUpdater
    {
        /// <summary>
        /// Gets the newest repository version which is supported by this installation of the app.
        /// The version will be increased, whenever the repository becomes incompatible, and
        /// therefore older apps would damage the repository.
        /// </summary>
        int NewestSupportedRevision { get; }

        /// <summary>
        /// Checks whether a repository was stored with a newer incompatible version of the app.
        /// In this case, we should refuse to work with the repository to avoid damaging future
        /// features.
        /// </summary>
        /// <param name="repository">The repository to check.</param>
        /// <returns>Returns true if the repository is too new for this app, otherwise false.</returns>
        bool IsTooNewForThisApp(XDocument repository);

        /// <summary>
        /// Updates the repository to the most current version supported by the application.
        /// This can span 0-n update steps.
        /// </summary>
        /// <param name="repository">Repository to update</param>
        /// <returns>Returns true if the repository was updated, false if there was no need to update.</returns>
        bool Update(XDocument repository);
    }
}
