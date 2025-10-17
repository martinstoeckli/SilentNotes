// Copyright © 2025 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Text;
using SilentNotes.Services;

namespace SilentNotes.Platforms.Services
{
    /// <summary>
    /// Implementation of the <see cref="ISharingService"/> interface for the Windows platform.
    /// </summary>
    internal class SharingService : ISharingService
    {
        /// <inheritdoc/>
        public async Task ShareHtmlText(string htmlText, string plainText, string subject)
        {
            subject = Uri.EscapeDataString(subject);

            // Some email clients will strip away leading spaces, thus removing the indentation.
            plainText = ReplaceLeadingSpaces(plainText);
            string body = Uri.EscapeDataString(plainText);

            string url = string.Format("mailto:{0}?subject={1}&body={2}",
                string.Empty, subject, body);
            await Windows.System.Launcher.LaunchUriAsync(new Uri(url));
        }

        /// <summary>
        /// Replaces leading space characters in each line with a &nbsp character.
        /// </summary>
        /// <param name="text">Text to search for leading spaces.</param>
        /// <returns>Text with replaced leading spaces.</returns>
        private static string ReplaceLeadingSpaces(string text)
        {
            const char space = ' ';
            const char nbsp = '\u00A0';

            var result = new StringBuilder(text);
            bool insideLeadingSpaces = true;
            for (int pos = 0; pos < text.Length; pos++)
            {
                switch (result[pos])
                {
                    case space:
                        if (insideLeadingSpaces)
                            result[pos] = nbsp;
                        break;
                    case '\n':
                    case '\r':
                        insideLeadingSpaces = true;
                        break;
                    default:
                        insideLeadingSpaces = false;
                        break;
                }
            }
            return result.ToString();
        }
    }
}
