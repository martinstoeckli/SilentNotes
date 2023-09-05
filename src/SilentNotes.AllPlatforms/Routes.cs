// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Text;

namespace SilentNotes
{
    /// <summary>
    /// List of routes to pages which can be used as navigation targets.
    /// </summary>
    public class Routes
    {
        public const string NoteRepository = "/";
        public const string ChangePassword = "/changepassword";
        public const string CloudStorageAccount = "/cloudstorageaccount";
        public const string CloudStorageChoice = "/cloudstoragechoice";
        public const string CloudStorageOauthWaiting = "/cloudstorageoauthwaiting";
        public const string Export = "/export";
        public const string FirstTimeSync = "/firsttimesync";
        public const string Info = "/info";
        public const string MergeChoice = "/mergechoice";
        public const string OpenSafe = "/opensafe";
        public const string RecycleBin = "/recyclebin";
        public const string Settings = "/settings";
        public const string TransferCode = "/transfercode";
        public const string TransferCodeHistory = "/transfercodehistory";
    }
}
