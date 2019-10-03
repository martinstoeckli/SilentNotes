// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Threading.Tasks;
using System.Xml.Linq;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotes.StoryBoards.PullPushStory
{
    /// <summary>
    /// This step belongs to the <see cref="PullPushStoryBoard"/>. It tries to decrypt an
    /// already downloaded cloud repository with the known transfer codes.
    /// </summary>
    public class DecryptCloudRepositoryStep : SynchronizationStory.DecryptCloudRepositoryStep
    {
        /// <inheritdoc/>
        public DecryptCloudRepositoryStep(
            int stepId,
            IStoryBoard storyBoard,
            ILanguageService languageService,
            IFeedbackService feedbackService,
            ISettingsService settingsService,
            INoteRepositoryUpdater noteRepositoryUpdater)
            : base(stepId, storyBoard, languageService, feedbackService, settingsService, noteRepositoryUpdater)
        {
        }

        /// <inheritdoc/>
        public override async Task Run()
        {
            try
            {
                SettingsModel settings = _settingsService.LoadSettingsOrDefault();
                byte[] binaryCloudRepository = StoryBoard.LoadFromSession<byte[]>(PullPushStorySessionKey.BinaryCloudRepository.ToInt());

                // Try to decode with all possible transfer codes
                bool successfullyDecryptedRepository = TryDecryptWithAllTransferCodes(
                    settings, binaryCloudRepository, out byte[] decryptedRepository);

                if (successfullyDecryptedRepository)
                {
                    // Deserialize and update repository
                    XDocument cloudRepositoryXml = XmlUtils.LoadFromXmlBytes(decryptedRepository);
                    if (_noteRepositoryUpdater.IsTooNewForThisApp(cloudRepositoryXml))
                        throw new SynchronizationStory.SynchronizationStoryBoard.UnsuportedRepositoryRevisionException();

                    _noteRepositoryUpdater.Update(cloudRepositoryXml);
                    NoteRepositoryModel cloudRepository = XmlUtils.DeserializeFromXmlDocument<NoteRepositoryModel>(cloudRepositoryXml);

                    // Continue with next step
                    StoryBoard.StoreToSession(PullPushStorySessionKey.CloudRepository.ToInt(), cloudRepository);
                    await StoryBoard.ContinueWith(PullPushStoryStepId.IsSameRepository.ToInt());
                }
                else
                {
                    _feedbackService.ShowToast(_languageService["pushpull_error_need_sync_first"]);
                }
            }
            catch (Exception ex)
            {
                // Keep the current page open and show the error message
                ShowExceptionMessage(ex, _feedbackService, _languageService);
            }
        }
    }
}
