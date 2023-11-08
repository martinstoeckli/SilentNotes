// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.Workers
{
    /// <summary>
    /// Can be used to detect modifications between two states of the data, by calculating a
    /// fingerprint of its content.
    /// </summary>
    internal class ModificationDetector
    {
        private readonly Func<long?> _fingerPrintProvider;
        private long? _memorizedFingerPrint;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModificationDetector"/> class.
        /// </summary>
        /// <param name="fingerPrintProvider">A delegate which can calculate a finger print (hash)
        /// value, which identifies the current content of the data.</param>
        /// <param name="memorizeCurrentState">If true, <see cref="MemorizeCurrentState"/> is
        /// called immediately by the constructor.</param>
        /// <exception cref="ArgumentNullException">Is thrown if a required parameter is null.</exception>
        public ModificationDetector(Func<long?> fingerPrintProvider, bool memorizeCurrentState = true)
        {
            if (fingerPrintProvider == null)
                throw new ArgumentNullException(nameof(fingerPrintProvider));

            _fingerPrintProvider = fingerPrintProvider;
            if (memorizeCurrentState)
                MemorizeCurrentState();
        }

        /// <summary>
        /// Calculates the finger print (hash) of the current content of the data.
        /// </summary>
        public void MemorizeCurrentState()
        {
            _memorizedFingerPrint = _fingerPrintProvider();
        }

        /// <summary>
        /// Checks whether the content of the data has changed since the last call to
        /// <see cref="MemorizeCurrentState"/>, by calculating and comparing a new finger print.
        /// </summary>
        /// <returns>Returns true if the content of the data has changed, otherwise false.</returns>
        public bool IsModified()
        {
            long? currentFingerprint = _fingerPrintProvider();
            return _memorizedFingerPrint != currentFingerprint;
        }
    }
}
