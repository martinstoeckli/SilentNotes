// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<IFeedbackService>(),
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<ICloudStorageClientFactory>()));
            RegisterStep(new DownloadCloudRepositoryStep(
                PullPushStoryStepId.DownloadCloudRepository,
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<IFeedbackService>(),
                Ioc.GetOrCreate<ICloudStorageClientFactory>(),
                Ioc.GetOrCreate<ISettingsService>()));
            RegisterStep(new DecryptCloudRepositoryStep(
                PullPushStoryStepId.DecryptCloudRepository,
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<IFeedbackService>(),
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<INoteRepositoryUpdater>()));
            RegisterStep(new IsSameRepositoryStep(
                PullPushStoryStepId.IsSameRepository,
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<IFeedbackService>(),
                Ioc.GetOrCreate<IRepositoryStorageService>()));
            RegisterStep(new StoreMergedRepositoryAndQuitStep(
                PullPushStoryStepId.StoreMergedRepositoryAndQuit,
                this,
                noteId,
                direction,
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<IFeedbackService>(),
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<ICryptoRandomService>(),
                Ioc.GetOrCreate<IRepositoryStorageService>(),
                Ioc.GetOrCreate<ICloudStorageClientFactory>()));
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

    /// <summary>Extension methods for the enumeration.</summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Extension methods.")]
    public static class PullPushStorySessionKeyExtensions
    {
        /// <summary>Conversion from enum to int.</summary>
        /// <param name="step">The step.</param>
        /// <returns>Integer of the step.</returns>
        [DebuggerStepThrough]
        public static int ToInt(this PullPushStorySessionKey step)
        {
            return (int)step;
        }
    }
}
