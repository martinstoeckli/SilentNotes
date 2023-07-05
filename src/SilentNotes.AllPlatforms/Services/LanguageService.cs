// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="ILanguageService"/> interface.
    /// The platform specific part of reading the resource files is outsourced to an instance of
    /// <see cref="ILanguageServiceResourceReader"/>.
    /// </summary>
    public class LanguageService : ILanguageService, ILanguageTestService
    {
        private readonly ILanguageServiceResourceReader _resourceReader;
        private readonly string _domain;
        private readonly string _languageCode;
        private readonly string _fallbackLanguageCode;
        private Dictionary<string, string> _textResources;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageService"/> class.
        /// </summary>
        /// <param name="resourceReader">A reader which knows about how to load language resources.</param>
        /// <param name="domain">The domain name defines the pattern of the language resource files
        /// which can be found by the lanugage service. The format is a preceding "Lng.", followed
        /// by the <paramref name="domain"/> and terminated by the <paramref name="languageCode"/>.
        /// Example: Lng.MyApp.en</param>
        /// <param name="languageCode">Two digit language code.</param>
        /// <param name="fallbackLanguageCode">The language code, which should be used if there
        /// doesn't exist resources for the requested language code. The default is set to "en"
        /// for English.</param>
        public LanguageService(
            ILanguageServiceResourceReader resourceReader,
            string domain,
            string languageCode,
            string fallbackLanguageCode = "en")
        {
            _resourceReader = resourceReader;
#if (LANG_EN && DEBUG)
            languageCode = "en";
#endif
            _domain = domain;
            _languageCode = languageCode;
            _fallbackLanguageCode = fallbackLanguageCode;
        }

        /// <inheritdoc/>
        public string this[string id]
        {
            get { return LoadText(id); }
        }

        /// <inheritdoc/>
        public string LoadText(string id)
        {
            if (LazyLoadTextResources() && _textResources.TryGetValue(id, out string text))
                return text;
            else if (Debugger.IsAttached)
                throw new Exception(string.Format("Could not find text resource {0}", id));
            else
                return "Translated text not found";
        }

        /// <inheritdoc/>
        public string LoadTextFmt(string id, params object[] args)
        {
            string text = LoadText(id);
            try
            {
                return string.Format(text, args);
            }
            catch
            {
                return text;
            }
        }

        /// <inheritdoc/>
        public string FormatDateTime(DateTime dateTime, string format)
        {
            CultureInfo culture;
            try
            {
                culture = CultureInfo.GetCultureInfo(_languageCode);
            }
            catch (Exception)
            {
                culture = CultureInfo.InvariantCulture;
            }
            return dateTime.ToString(format, culture);
        }

        private bool LazyLoadTextResources()
        {
            // Try to load requested language resources.
            if (_textResources == null)
            {
                try
                {
                    _textResources = LoadTextResources(_domain, _languageCode);
                }
                catch (Exception)
                {
                    _textResources = null;
                }
            }

            // Load fallback language in case that the file could not be found, or was invalid.
            // The fallback language is installed by the app, so it must exist, otherwise throw.
            if (_textResources == null)
                _textResources = LoadTextResources(_domain, _fallbackLanguageCode);
            return _textResources != null;
        }

        /// <inheritdoc/>
        private Dictionary<string, string> LoadTextResources(string domain, string languageCode)
        {
            Dictionary<string, string> result = null;

            Stream resourceStream = Task.Run(async () => await _resourceReader.TryOpenResourceStream(domain, languageCode)).Result;
            if (resourceStream != null)
            {
                using (resourceStream)
                using (StreamReader resourceReader = new StreamReader(resourceStream))
                {
                    result = ReadFromStream(resourceReader);
                }
            }
            return result;
        }

        internal Dictionary<string, string> ReadFromStream(StreamReader languageResourceStream)
        {
            var result = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

            // process all lines
            string line;
            while ((line = languageResourceStream.ReadLine()) != null)
            {
                if (!IsComment(line) &&
                    TrySplitLine(line, out string resKey, out string resText))
                {
                    resText = ReplaceSpecialTags(resText);
                    result[resKey] = resText;
                }
            }
            return result;
        }

        /// <summary>
        /// Checks whether a given line contains a comment starting with //, and therefore can be
        /// skipped.
        /// </summary>
        /// <param name="line">Line to test.</param>
        /// <returns>Returns true if the line contains a comment, otherwise false.</returns>
        private static bool IsComment(string line)
        {
            return line.TrimStart().StartsWith(@"//");
        }

        /// <summary>
        /// Splits a line from a file in two parts, id and text. The id is expected at the begin of
        /// the line, separated by a '='. It's allowed to have none or multiple whitespace
        /// characters around the separator.
        /// </summary>
        /// <param name="line">Line form resource file.</param>
        /// <param name="key">Retreives the found id.</param>
        /// <param name="text">Retreives the found text.</param>
        /// <returns>Returns true if line was valid, otherwise false.</returns>
        private static bool TrySplitLine(string line, out string key, out string text)
        {
            key = null;
            text = null;
            if (line == null)
                return false;

            line = line.Trim();

            int delimiterPos = line.IndexOf('=');
            if (delimiterPos < 1)
                return false;

            key = line.Substring(0, delimiterPos).TrimEnd();
            text = line.Substring(delimiterPos + 1).TrimStart();
            return true;
        }

        /// <summary>
        /// Replaces tags with a special meaning (e.g. "\n" or "\r\n" becomes crlf).
        /// </summary>
        /// <param name="resText">Text to search.</param>
        /// <returns>Text with replaces tags.</returns>
        protected static string ReplaceSpecialTags(string resText)
        {
            string result = resText;
            if (result.Contains(@"\n"))
            {
                result = result.Replace(@"\r\n", "\r\n");
                result = result.Replace(@"\n", "\r\n");
            }
            return result;
        }

        /// <inheritdoc/>
        public void OverrideWithTestResourceFile(byte[] customResourceFile)
        {
            const int MaxResourceItemLength = 1000; // We handle unsafe user input after all

            // Reset to the original language
            _textResources = null;
            LazyLoadTextResources();

            // Read test file and overwrite existing items.
            using (MemoryStream stream = new MemoryStream(customResourceFile))
            using (StreamReader languageResourceStream = new StreamReader(stream))
            {
                // process all lines
                string line;
                while ((line = languageResourceStream.ReadLine()) != null)
                {
                    if (!IsComment(line) &&
                        TrySplitLine(line, out string resKey, out string resText))
                    {
                        if (_textResources.ContainsKey(resKey))
                        {
                            resText = ReplaceSpecialTags(resText);
                            if (resText.Length > MaxResourceItemLength)
                                resText.Substring(0, MaxResourceItemLength);
                            _textResources[resKey] = resText;
                        }
                    }
                }
            }
        }
    }
}
