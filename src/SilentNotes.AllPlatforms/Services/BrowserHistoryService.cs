// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SilentNotes.Services
{
    /// <summary>
    /// Implementation of the <see cref="IBrowserHistoryService"/> interface.
    /// </summary>
    [DebuggerDisplay("{HistoryAsString}")]
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
        public string PreviousLocation
        { 
            get
            {
                int secondToLastIndex = _history.Count - 2;
                if (secondToLastIndex >= 0)
                    return _history[secondToLastIndex];
                return null;
            }
        }

        /// <summary>
        /// Gets the number of items in the history. In contrast to the a real browser history,
        /// this is the number of steps from the first to the current one, it doesn't count later
        /// items which would exist to navigate forward.
        /// </summary>
        internal int Count
        {
            get { return _history.Count; }
        }

        /// <inheritdoc/>
        public NavigationDirection UpdateHistoryOnNavigation(string targetLocation, string baseUri)
        {
            NavigationDirection result = DetermineDirection(targetLocation, baseUri);

            switch (result)
            {
                case NavigationDirection.Next:
                    string relativeTargetLocation = GetRelativeUri(targetLocation, baseUri);
                    _history.Add(relativeTargetLocation);
                    break;
                case NavigationDirection.Back:
                    int lastIndex = _history.Count - 1;
                    _history.RemoveAt(lastIndex);
                    break;
            }
            return result;
        }

        /// <inheritdoc/>
        public void Clear(bool keepHomeLocation)
        {
            if (keepHomeLocation)
            {
                if (_history.Count > 1)
                    _history.RemoveRange(1, _history.Count - 1);
            }
            else
            {
                _history.Clear();
            }
        }

        /// <inheritdoc/>
        public void RemoveCurrent()
        {
            int lastIndex = _history.Count - 1;
            if (lastIndex >= 0)
                _history.RemoveAt(lastIndex);
        }

        /// <inheritdoc/>
        public NavigationDirection DetermineDirection(string targetLocation, string baseUri)
        {
            if (targetLocation == null)
                throw new ArgumentNullException(nameof(targetLocation));
            if (baseUri == null)
                throw new ArgumentNullException(nameof(baseUri));

            string relativeTargetLocation = GetRelativeUri(targetLocation, baseUri);

            int lastIndex = _history.Count - 1;
            int secondToLastIndex = lastIndex - 1;

            if ((lastIndex >= 0) && string.Equals(relativeTargetLocation, _history[lastIndex], StringComparison.InvariantCultureIgnoreCase))
            {
                // This is a refresh of the page
                return NavigationDirection.Reload;
            }
            else if ((secondToLastIndex >= 0) && string.Equals(relativeTargetLocation, _history[secondToLastIndex], StringComparison.InvariantCultureIgnoreCase))
            {
                // This is a back navigation
                return NavigationDirection.Back;
            }
            else
            {
                // This is a forward navigation to a new page
                return NavigationDirection.Next;
            }
        }

        internal string GetRelativeUri(string targetUri, string trimmedBaseUri)
        {
            string result = targetUri;
            trimmedBaseUri = trimmedBaseUri.TrimEnd('/');
            if (result.StartsWith(trimmedBaseUri))
                result = result.Substring(trimmedBaseUri.Length);
            return result;
        }

        private string HistoryAsString
        {
            get { return string.Join("  ", _history); }
        }
    }
}
