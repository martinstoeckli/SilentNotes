// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SilentNotes.StoryBoards
{
    /// <summary>
    /// Implementation of the <see cref="IStoryBoardSession"/> interface.
    /// </summary>
    public class StoryBoardSession : IStoryBoardSession
    {
        private Dictionary<Enum, object> _session;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoryBoardSession"/> class.
        /// </summary>
        public StoryBoardSession()
        {
            _session = new Dictionary<Enum, object>();
        }

        /// <inheritdoc/>
        public void Store(Enum key, object value)
        {
            _session[key] = value;
        }

        /// <inheritdoc/>
        public void Remove(Enum key)
        {
            _session.Remove(key);
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        public bool TryLoad<T>(Enum key, out T value)
        {
            if (_session.TryGetValue(key, out var dictionaryValue) && (dictionaryValue is T typedValue))
            {
                value = typedValue;
                return true;
            }
            value = default(T);
            return false;
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        public T Load<T>(Enum key)
        {
            bool successful = TryLoad<T>(key, out T result);
            if (successful)
                return result;
            else
                throw new ArgumentOutOfRangeException(nameof(key));
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _session.Clear();
        }
    }
}
