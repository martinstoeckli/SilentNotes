// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Net;

namespace SilentNotes.Services.CloudStorageServices
{
    /// <summary>
    /// Extends a WebClient with a user defined timeout.
    /// </summary>
    public class WebClientWithTimeout : WebClient
    {
        private readonly TimeSpan _timeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebClientWithTimeout"/> class.
        /// </summary>
        /// <param name="timeout">Timespan which defines the maximum waiting time.
        /// e.g. TimeSpan.FromSeconds(60)</param>
        public WebClientWithTimeout(TimeSpan timeout)
        {
            _timeout = timeout;
        }

        /// <summary>
        /// Overrides creation of the web request to set a timeout.
        /// </summary>
        /// <param name="uri">The uri to get a webrequest from.</param>
        /// <returns>The web request object.</returns>
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest request = base.GetWebRequest(uri);
            request.Timeout = (int)_timeout.TotalMilliseconds;
            return request;
        }
    }
}
