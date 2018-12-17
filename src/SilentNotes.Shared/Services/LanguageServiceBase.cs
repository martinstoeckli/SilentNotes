// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SilentNotes.Services
{
    /// <summary>
    /// Base class for implementations of the <see cref="ILanguageService"/> interface.
    /// </summary>
    public abstract class LanguageServiceBase : ILanguageService
    {
        private readonly string _languageCode;
        private Dictionary<string, string> _textResources;

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageServiceBase"/> class.
        /// </summary>
        /// <param name="languageCode">Two digit language code.</param>
        public LanguageServiceBase(string languageCode)
        {
            _languageCode = languageCode;
        }

        /// <inheritdoc/>
        public string this[string id]
        {
            get { return LoadText(id); }
        }

        /// <inheritdoc/>
        public string LoadText(string id)
        {
            LoadTextResourcesIfNecessary();

            string text;
            if (_textResources.TryGetValue(id, out text))
            {
                return text;
            }
            else
            {
                if (Debugger.IsAttached)
                    throw new Exception(string.Format("Could not find text resource {0}", id));
                else
                    return string.Empty;
            }
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

        /// <summary>
        /// Derrived classes should fill the dictionary with text resources of a given language.
        /// The id should always be lower case only.
        /// </summary>
        /// <param name="textResources">Dictionary to fill.</param>
        /// <param name="languageCode">Two digit lanugage code of the required language.</param>
        protected abstract void LoadTextResources(Dictionary<string, string> textResources, string languageCode);

        private void LoadTextResourcesIfNecessary()
        {
            if (_textResources == null)
            {
                _textResources = new Dictionary<string, string>();
                LoadTextResources(_textResources, _languageCode);
            }
        }
    }
}
