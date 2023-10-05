// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Models;
using SilentNotes.Services;

namespace SilentNotes.Stories.SynchronizationStory
{
    /// <summary>
    /// This step belongs to the <see cref="SynchronizationStory"/>. It checks whether the
    /// downloaded repository is the same repository as the one stored locally (has the same id).
    /// </summary>
    internal class IsSameRepositoryStep : SynchronizationStoryStepBase
    {
        /// <inheritdoc/>
        public override Task<StoryStepResult<SynchronizationStoryModel>> RunStep(SynchronizationStoryModel model, IServiceProvider serviceProvider, StoryMode uiMode)
        {
            var repositoryStorageService = serviceProvider.GetService<IRepositoryStorageService>();
            repositoryStorageService.LoadRepositoryOrDefault(out NoteRepositoryModel localRepository);

            if (localRepository.Id == model.CloudRepository.Id)
            {
                return ToTask(ToResult(new StoreMergedRepositoryAndQuitStep()));
            }
            else
            {
                return ToTask(ToResult(new ShowMergeChoiceStep()));
            }
        }
    }
}
