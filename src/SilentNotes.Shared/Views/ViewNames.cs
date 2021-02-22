// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Views
{
    /// <summary>
    /// Collection of known razor view names.
    /// </summary>
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class ViewNames
    {
        public const string NoteRepository = "noterepositoryview";
        public const string NoteRepositoryContent = "noterepositorycontentview";
        public const string Note = "noteview";
        public const string Checklist = "checklistview";
        public const string Info = "infoview";
        public const string RecycleBin = "recyclebinview";
        public const string RecycleBinContent = "recyclebincontentview";
        public const string Settings = "settingsview";
        public const string TransferCode = "transfercodeview";
        public const string TransferCodeHistory = "transfercodehistoryview";
        public const string Stop = "stopview";
        public const string FirstTimeSync = "firsttimesyncview";
        public const string CloudStorageChoice = "cloudstoragechoiceview";
        public const string CloudStorageAccount = "cloudstorageaccountview";
        public const string CloudStorageOauthWaiting = "cloudstorageoauthwaitingview";
        public const string MergeChoice = "mergechoiceview";
        public const string OpenSafe = "opensafeview";
        public const string ChangePassword = "changepasswordview";
        public const string Export = "exportview";
    }
}
