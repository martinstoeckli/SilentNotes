// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.ComponentModel;

namespace SilentNotes.HtmlView
{
    /// <summary>
    /// Notifier which can handle changes in the viewmodel.
    /// </summary>
    public class HtmlViewBindingViewmodelNotifier
    {
        private readonly INotifyPropertyChanged _notifier;
        private readonly string _propertyName;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlViewBindingViewmodelNotifier"/> class.
        /// </summary>
        /// <param name="notifier">Usually the viewmodel itself, supporting the
        /// INotifyPropertyChanged interface.</param>
        /// <param name="propertyName">The name of the property to listen to.</param>
        public HtmlViewBindingViewmodelNotifier(INotifyPropertyChanged notifier, string propertyName)
            : base()
        {
            _notifier = notifier;
            _propertyName = propertyName;
        }

        /// <summary>
        /// Subscribe to property change events in the viewmodel.
        /// </summary>
        public void SubscribeEvents()
        {
            if ((_notifier != null) && !string.IsNullOrWhiteSpace(_propertyName))
                _notifier.PropertyChanged += ViewmodelChangedEventHandler;
        }

        /// <summary>
        /// Release subscriptions to property change events in the viewmodel.
        /// </summary>
        public void UnsubscribeEvents()
        {
            if ((_notifier != null) && !string.IsNullOrWhiteSpace(_propertyName))
                _notifier.PropertyChanged -= ViewmodelChangedEventHandler;
        }

        /// <summary>
        /// This event is triggered whenever the property of the viewmodel changes.
        /// </summary>
        public event Action<object> Notified;

        /// <summary>
        /// Triggers the <see cref="Notified"/> event.
        /// </summary>
        /// <param name="value">Argument passed from the viewmodel to the binding.</param>
        public void OnNotified(object value)
        {
            if (Notified != null)
                Notified(value);
        }

        private void ViewmodelChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            string changedPropertyName = e.PropertyName;
            if (changedPropertyName == _propertyName)
                OnNotified(null);
        }
    }
}
