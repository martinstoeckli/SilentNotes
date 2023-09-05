// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using VanillaCloudStorageClient;

namespace SilentNotes.Stories.SynchronizationStory
{
    /// <summary>
    /// Model storing the collected information about the synchronization story.
    /// </summary>
    public class SynchronizationStoryModel
    {
        public SerializeableCloudStorageCredentials Credentials { get; set; }
    }
}
