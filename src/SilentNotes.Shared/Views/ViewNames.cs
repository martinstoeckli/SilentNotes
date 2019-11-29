// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;

namespace SilentNotes.Views
{
    /// <summary>
    /// Collection of known razor view names.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Just a collection of constants.")]
    public static class ViewNames
    {
        public const string NoteRepository = "noterepositoryview";
        public const string NoteRepositoryContent = "noterepositorycontentview";
        public const string Note = "noteview";
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
    }
}
