// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using AndroidX.Lifecycle;

namespace SilentNotes.Android
{
    /// <summary>
    /// Helper class to implement the Android <see cref="IObserver"/> interface.
    /// An instance of this class can be passed whenever Android requires an IObserver interface,
    /// it can redirect the <see cref="IObserver.OnChanged(Java.Lang.Object?)"/> event to a
    /// delegate. This way the owner doesn't have to inherit from Java.Lang.Object.
    /// </summary>
    internal class ObserverHelper : Java.Lang.Object, IObserver
    {
        private readonly Action<Java.Lang.Object> _action;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObserverHelper"/> class.
        /// </summary>
        /// <param name="action">Redirect to this delegate if the <see cref="IObserver.OnChanged(Java.Lang.Object?)"/>
        /// event was triggered.</param>
        public ObserverHelper(Action<Java.Lang.Object> action)
        {
            _action = action;
        }

        /// <summary>
        /// Handler for the event triggered by the observed object.
        /// </summary>
        /// <param name="p0">The event argument.</param>
        public void OnChanged(Java.Lang.Object p0)
        {
            _action(p0);
        }
    }
}