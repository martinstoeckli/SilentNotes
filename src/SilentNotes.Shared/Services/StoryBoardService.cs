// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.StoryBoards;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="IStoryBoardService"/> interface.
    /// </summary>
    public class StoryBoardService : IStoryBoardService
    {
        /// <summary>
        /// We store the <see cref="ActiveStory"/> property in this static member, so it remains
        /// accessible even if the Android activity changes and the Ioc is rebuilt with new services.
        /// </summary>
        private static IStoryBoard _activeStory;

        /// <inheritdoc/>
        public IStoryBoard ActiveStory
        {
            get { return _activeStory; }
            set { _activeStory = value; }
        }
    }
}
