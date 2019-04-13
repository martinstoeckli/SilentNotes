// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Globalization;
using System.Threading;

namespace SilentNotesTest
{
    /// <summary>
    /// Switches between different cultures for testing purposes.
    /// Use this class inside a using() statement.
    /// <example><code>
    /// using (var cultureSwitcher = new CultureSwitcher4UnitTest("de-De"))
    /// {
    ///     // work with other culture
    /// }
    /// </code></example>
    /// </summary>
    internal class CultureSwitcher4UnitTest : IDisposable
    {
        private readonly CultureInfo _oldCultureInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="CultureSwitcher4UnitTest"/> class.
        /// </summary>
        /// <param name="cultureName">Pass a culture name like "de-De" or omit this parameter
        /// to get the invariant culture.</param>
        public CultureSwitcher4UnitTest(string cultureName = "InvariantCulture")
        {
            // Remember current culture
            _oldCultureInfo = Thread.CurrentThread.CurrentCulture;

            if ("InvariantCulture".Equals(cultureName, StringComparison.InvariantCultureIgnoreCase))
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            else
                Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
        }

        /// <summary>
        /// Switches back to the previous culture.
        /// </summary>
        public void Dispose()
        {
            Thread.CurrentThread.CurrentCulture = _oldCultureInfo;
        }
    }
}
