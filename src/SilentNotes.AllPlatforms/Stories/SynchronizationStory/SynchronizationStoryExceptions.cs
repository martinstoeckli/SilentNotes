// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Stories.SynchronizationStory
{
    /// <summary>
    /// Exceptions specific to the synchronization story.
    /// </summary>
    internal class SynchronizationStoryExceptions
    {
        /// <summary>
        /// This exception can be thrown, when a repository has a revision, which is supported only
        /// by more recent applications.
        /// </summary>
        internal class UnsuportedRepositoryRevisionException : Exception
        {
        }
    }
}
