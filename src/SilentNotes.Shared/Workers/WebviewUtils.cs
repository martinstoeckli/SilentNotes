// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Web;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Helper class for working with web related stuff.
    /// </summary>
    public static class WebviewUtils
    {
        /// <summary>
        /// Escapes a string so it can be inserted into a script as string. Characters like single
        /// quotation marks and double quotation marks are encoded, null string are converted to an
        /// empty string.
        /// </summary>
        /// <param name="text">String to encode.</param>
        /// <returns>Encoded string, which can safely be placed into a script.</returns>
        public static string EscapeJavaScriptString(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            else
                return HttpUtility.JavaScriptStringEncode(text, false);
        }

        /// <summary>
        /// Determines whether a given url is a link to an external resource (e.g. http:// or mailto://).
        /// Usually those url's are used to open an external webpage. This is the opposite of an url
        /// with a local protocol which is used to communicate between a webview and the app.
        /// </summary>
        /// <param name="uri">Url to examine.</param>
        /// <returns>Returns true if the url is an external link, otherwise false.</returns>
        public static bool IsExternalUri(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                return false;

            // The URI class fails with some protocols like "tel://" which are not valid urls, but
            // can be used in an html page nevertheless, thus we do the parsing on our own.
            string[] acceptedSchemes = new string[] { "http:", "https:", "ftp:", "ftps:", "mailto:", "news:", "tel:" };
            foreach (string acceptedProtocol in acceptedSchemes)
            {
                if (uri.StartsWith(acceptedProtocol, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
