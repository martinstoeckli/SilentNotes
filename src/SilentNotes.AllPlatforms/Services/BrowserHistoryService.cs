// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="IBrowserHistoryService"/> interface.
    /// </summary>
    internal class BrowserHistoryService : IBrowserHistoryService
    {
        private readonly List<string> _history;

        /// <summary>
        /// Initializes a new instance of the <see cref="BrowserHistoryService"/> class.
        /// </summary>
        public BrowserHistoryService()
        {
            _history = new List<string>();
        }

        /// <inheritdoc/>
        public string CurrentLocation
        {
            get { return _history.LastOrDefault(); }
        }

        /// <inheritdoc/>
        public string LastLocation
        { 
            get
            {
                int secondToLastIndex = _history.Count - 2;
                if (secondToLastIndex >= 0)
                    return _history[secondToLastIndex];
                return null;
            }
        }

        /// <inheritdoc/>
        public void UpdateHistoryOnNavigation(string targetLocation, string baseUri)
        {
            if (targetLocation == null)
                throw new ArgumentNullException(nameof(targetLocation));
            if (baseUri == null)
                throw new ArgumentNullException(nameof(baseUri));

            string relativeTargetLocation = GetRelativeUri(targetLocation, baseUri);

            int lastIndex = _history.Count - 1;
            int secondToLastIndex = lastIndex - 1;

            if ((lastIndex >= 0) && string.Equals(relativeTargetLocation, _history[lastIndex]))
            {
                // This is a refresh of the page, don't alter the history
            }
            else if ((secondToLastIndex >= 0) && string.Equals(relativeTargetLocation, _history[secondToLastIndex]))
            {
                // This is a back navigation
                _history.RemoveAt(lastIndex);
            }
            else
            {
                // This is a forward navigation to a new page
                _history.Add(relativeTargetLocation);
            }
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _history.Clear();
        }

        /// <inheritdoc/>
        public void RemoveCurrent()
        {
            int lastIndex = _history.Count - 1;
            if (lastIndex >= 0)
                _history.RemoveAt(lastIndex);
        }

        private string GetRelativeUri(string targetUri, string trimmedBaseUri)
        {
            string result = targetUri;
            trimmedBaseUri = trimmedBaseUri.TrimEnd('/');
            if (result.StartsWith(trimmedBaseUri))
                result = result.Substring(trimmedBaseUri.Length);
            return result;
        }
    }
}
