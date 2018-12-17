// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.HtmlView
{
    /// <summary>
    /// Event arguments for the <see cref="HtmlViewBindings.NotifiedEventHandler"/> event.
    /// </summary>
    public class HtmlViewBindingNotifiedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlViewBindingNotifiedEventArgs"/> class.
        /// </summary>
        /// <param name="bindingName">The name/id of the binding.</param>
        /// <param name="eventType">The type of the HTML event.</param>
        /// <param name="parameters">The query parameters.</param>
        public HtmlViewBindingNotifiedEventArgs(string bindingName, string eventType, KeyValueList<string, string> parameters)
        {
            BindingName = bindingName;
            EventType = eventType;
            Parameters = parameters;
        }

        /// <summary>
        /// Gets the name of the binding, declared as "data-binding" attribute of the HTML element.
        /// </summary>
        public string BindingName { get; private set; }

        /// <summary>
        /// Gets the type of the HTML event.
        /// </summary>
        public string EventType { get; private set; }

        /// <summary>
        /// Gets the binding parameters, containing all "data-*" attributes.
        /// </summary>
        public KeyValueList<string, string> Parameters { get; private set; }
    }
}
