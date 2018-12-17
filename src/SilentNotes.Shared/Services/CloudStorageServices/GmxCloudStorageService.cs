// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using SilentNotes.Services.CloudStorageServices;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="ICloudStorageService"/>,
    /// which can handle cloud storage with Gmx.
    /// </summary>
    public class GmxCloudStorageService : WebdavCloudStorageService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GmxCloudStorageService"/> class.
        /// </summary>
        public GmxCloudStorageService()
            : this(new CloudStorageAccount { CloudType = CloudStorageType.GMX, Url = "https://webdav.mc.gmx.net/" })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GmxCloudStorageService"/> class.
        /// </summary>
        /// <param name="account">The account with the credentials to use with this service.</param>
        public GmxCloudStorageService(CloudStorageAccount account)
            : base(account)
        {
            if (account.CloudType != CloudStorageType.GMX)
                throw new Exception("Invalid account information for GmxCloudStorageService, expected cloud storage type GMX");
            Account.IsUrlReadonly = true;
        }
    }
}
