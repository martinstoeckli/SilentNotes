// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SilentNotes.StoryBoards
{
    /// <summary>
    /// Base class for all classes implementing the <see cref="IStoryBoard"/> interface.
    /// </summary>
    public class StoryBoardBase : IStoryBoard
    {
        private readonly List<IStoryBoardStep> _steps;
        protected Dictionary<int, object> _session;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoryBoardBase"/> class.
        /// Derrived classes should register their steps in the constructor with
        /// method <see cref="RegisterStep(IStoryBoardStep)"/>.
        /// </summary>
        /// <param name="mode">Sets the property <see cref="Mode"/>.</param>
        public StoryBoardBase(StoryBoardMode mode = StoryBoardMode.GuiAndToasts)
        {
            Mode = mode;
            _session = new Dictionary<int, object>();
            _steps = new List<IStoryBoardStep>();
        }

        /// <inheritdoc/>
        public StoryBoardMode Mode { get; private set; }

        /// <inheritdoc/>
        public void RegisterStep(IStoryBoardStep step)
        {
            _steps.Add(step);
        }

        /// <inheritdoc/>
        public async Task Start()
        {
            if (_steps.Count > 0)
                await _steps[0].Run();
        }

        /// <inheritdoc/>
        public async Task ContinueWith(Enum stepId)
        {
            IStoryBoardStep step = FindRegisteredStep(stepId);
            if (step != null)
                await step.Run();
        }

        /// <inheritdoc/>
        public void StoreToSession(int key, object value)
        {
            _session[key] = value;
        }

        /// <inheritdoc/>
        public void RemoveFromSession(int key)
        {
            _session.Remove(key);
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        public bool TryLoadFromSession<T>(int key, out T value)
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
        public T LoadFromSession<T>(int key)
        {
            bool successful = TryLoadFromSession<T>(key, out T result);
            if (successful)
                return result;
            else
                throw new ArgumentOutOfRangeException(nameof(key));
        }

        /// <inheritdoc/>
        public void ClearSession()
        {
            _session.Clear();
        }

        /// <summary>
        /// Searches for a registered step in the story board.
        /// </summary>
        /// <param name="stepId">Id of the step to search for.</param>
        /// <returns>The found step, or null if no such step could be found.</returns>
        protected IStoryBoardStep FindRegisteredStep(Enum stepId)
        {
            return _steps.Find(step => step.Id.Equals(stepId));
        }
    }
}
