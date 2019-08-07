// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using SilentNotes.Controllers;

namespace SilentNotes.Services
{
    /// <summary>
    /// Dummy implementation of the <see cref="INavigationService"/> interface. This implementation
    /// provides no functionallity and can be used when all navigation should be ignored.
    /// </summary>
    public class DummyNavigationService : INavigationService
    {
        /// <inheritdoc/>
        public void Navigate(string controllerId, string variableName, string variableValue)
        {
        }

        /// <inheritdoc/>
        public void Navigate(string controllerId, KeyValueList<string, string> variables = null)
        {
        }

        /// <inheritdoc/>
        public void RepeatNavigationIf(IEnumerable<string> ifAnyOfThisControllers)
        {
        }

        /// <inheritdoc/>
        public IController CurrentController { get; private set; }
    }
}
