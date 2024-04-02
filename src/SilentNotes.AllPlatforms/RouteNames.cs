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
    public static class RouteNames
    {
        public const string NoteRepository = "/noterepository";
        public const string Note = "/note";
        public const string Checklist = "/checklist";
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
        public const string TransferCodePrompt = "/transfercodeprompt";
        public const string TransferCodeHistory = "/transfercodehistory";

        /// <summary>
        /// Combines a route name with parameters.
        /// </summary>
        /// <param name="routeName">The name of the route.</param>
        /// <param name="parts">0-n parts which are encoded and added to the resulting route path.</param>
        /// <returns>Route path.</returns>
        public static string Combine(string routeName, params object[] parts)
        {
            char delimiter = '/';
            StringBuilder result = new StringBuilder();
            result.Append(routeName);
            foreach (var part in parts)
            {
                if (part != null)
                {
                    string encodedPart = System.Web.HttpUtility.UrlEncode(part.ToString());
                    if (result[result.Length - 1] != delimiter)
                        result.Append(delimiter);
                    result.Append(encodedPart);
                }
            }
            return result.ToString();
        }
    }
}
