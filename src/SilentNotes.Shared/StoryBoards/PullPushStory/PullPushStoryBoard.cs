// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using CommunityToolkit.Mvvm.DependencyInjection;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotes.StoryBoards.PullPushStory
{
    /// <summary>
    /// Story for manual synchronization of a single note with the cloud.
    /// This story can only be triggered by the user and has no Gui input.
    /// </summary>
    public class PullPushStoryBoard : StoryBoardBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullPushStoryBoard"/> class.
        /// </summary>
        /// <param name="noteId">Sets the <see cref="NoteId"/> property.</param>
        /// <param name="direction">Sets the <see cref="Direction"/> property.</param>
        public PullPushStoryBoard(Guid noteId, PullPushDirection direction)
            : base(StoryBoardMode.GuiAndToasts)
        {
            RegisterStep(new ExistsCloudRepositoryStep(
                PullPushStoryStepId.ExistsCloudRepository,
                this,
                Ioc.Default.GetService<ILanguageService>(),
                Ioc.Default.GetService<IFeedbackService>(),
                Ioc.Default.GetService<ISettingsService>(),
                Ioc.Default.GetService<ICloudStorageClientFactory>()));
            RegisterStep(new DownloadCloudRepositoryStep(
                PullPushStoryStepId.DownloadCloudRepository,
                this,
                Ioc.Default.GetService<ILanguageService>(),
                Ioc.Default.GetService<IFeedbackService>(),
                Ioc.Default.GetService<ICloudStorageClientFactory>(),
                Ioc.Default.GetService<ISettingsService>()));
            RegisterStep(new DecryptCloudRepositoryStep(
                PullPushStoryStepId.DecryptCloudRepository,
                this,
                Ioc.Default.GetService<ILanguageService>(),
                Ioc.Default.GetService<IFeedbackService>(),
                Ioc.Default.GetService<ISettingsService>(),
                Ioc.Default.GetService<INoteRepositoryUpdater>()));
            RegisterStep(new IsSameRepositoryStep(
                PullPushStoryStepId.IsSameRepository,
                this,
                Ioc.Default.GetService<ILanguageService>(),
                Ioc.Default.GetService<IFeedbackService>(),
                Ioc.Default.GetService<IRepositoryStorageService>()));
            RegisterStep(new StoreMergedRepositoryAndQuitStep(
                PullPushStoryStepId.StoreMergedRepositoryAndQuit,
                this,
                noteId,
                direction,
                Ioc.Default.GetService<ILanguageService>(),
                Ioc.Default.GetService<IFeedbackService>(),
                Ioc.Default.GetService<ISettingsService>(),
                Ioc.Default.GetService<ICryptoRandomService>(),
                Ioc.Default.GetService<IRepositoryStorageService>(),
                Ioc.Default.GetService<ICloudStorageClientFactory>()));
        }
    }

    /// <summary>
    /// Enumeration of all available step ids of the <see cref="PullPushStoryBoard"/>.
    /// </summary>
    public enum PullPushStoryStepId
    {
        ExistsCloudRepository,
        DownloadCloudRepository,
        DecryptCloudRepository,
        IsSameRepository,
        StoreMergedRepositoryAndQuit,
    }

    /// <summary>
    /// Enumeration of all available session keys of the <see cref="PullPushStoryBoard"/>.
    /// </summary>
    public enum PullPushStorySessionKey
    {
        BinaryCloudRepository,
        CloudRepository,
    }

    /// <summary>
    /// Enumeration of the direction of the synchronization, either from online storage to local
    /// or reverse.
    /// </summary>
    public enum PullPushDirection
    {
        /// <summary>Pull the note from the server and overwrite the local note</summary>
        PullFromServer,

        /// <summary>Use the local note and overwrite the note on the server</summary>
        PushToServer
    }
}
