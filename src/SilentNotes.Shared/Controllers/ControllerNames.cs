// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Diagnostics.CodeAnalysis;

namespace SilentNotes.Controllers
{
    /// <summary>
    /// Collection of known controller names.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Just a collection of constants.")]
    public static class ControllerNames
    {
        public const string NoteRepository = "noterepository";
        public const string Note = "note";
        public const string Info = "info";
        public const string RecycleBin = "recyclebin";
        public const string Settings = "settings";
        public const string TransferCode = "transfercode";
        public const string TransferCodeHistory = "transfercodehistory";
        public const string FirstTimeSync = "firsttimesync";
        public const string CloudStorageChoice = "cloudstoragechoice";
        public const string CloudStorageAccount = "cloudstorageaccount";
        public const string CloudStorageOauthWaiting = "cloudstorageoauthwaiting";
        public const string MergeChoice = "mergechoice";
        public const string OpenSafe = "opensafe";
        public const string ChangePassword = "changepassword";
    }
}
