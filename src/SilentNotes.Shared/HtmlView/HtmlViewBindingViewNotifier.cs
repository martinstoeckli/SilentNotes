// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.HtmlView
{
    /// <summary>
    /// Notifier which can handle click events in the (HTML) view.
    /// </summary>
    public class HtmlViewBindingViewNotifier
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlViewBindingViewNotifier"/> class.
        /// </summary>
        /// <param name="bindingName">The name of the binding. The name is declared as
        /// "data-binding" attribute ot the HTML element.</param>
        /// <param name="eventType">The type of the HTML event, the notifier is connected to.</param>
        public HtmlViewBindingViewNotifier(string bindingName, string eventType)
        {
            BindingName = bindingName;
            EventType = eventType;
        }

        /// <summary>
        /// Gets the name of the binding, the notifier is connected to.
        /// </summary>
        public string BindingName { get; }

        /// <summary>
        /// Gets the type of the HTML event, the notifier is connected to.
        /// </summary>
        public string EventType { get; }

        /// <summary>
        /// This event is triggered whenever the property of the view/viewmodel changes.
        /// </summary>
        public event Action<KeyValueList<string, string>> Notified;

        /// <summary>
        /// Triggers the <see cref="Notified"/> event.
        /// </summary>
        /// <param name="parameters">Argument passed from the view to the binding.</param>
        public void OnNotified(KeyValueList<string, string> parameters)
        {
            if (Notified != null)
                Notified(parameters);
        }

        /// <summary>
        /// Checks whether the given parameters match this notifier.
        /// </summary>
        /// <param name="bindingName">The name of the binding.</param>
        /// <param name="eventType">The HTML event type.</param>
        /// <returns>Returns true if the notifier matches, otherwise false.</returns>
        public bool Matches(string bindingName, string eventType)
        {
            return string.Equals(BindingName, bindingName, StringComparison.InvariantCultureIgnoreCase) &&
                string.Equals(EventType, eventType, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
