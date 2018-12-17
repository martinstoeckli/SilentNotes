// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.HtmlView
{
    /// <summary>
    /// This interface describes a single binding between an (HTML) view and its viewmodel.
    /// </summary>
    public interface IHtmlViewBinding
    {
        /// <summary>
        /// Gets the notifier which informs about changes in the (HTML) view.
        /// </summary>
        HtmlViewBindingViewNotifier ViewNotifier { get; }

        /// <summary>
        /// Gets the notifier which informs about changes in the (HTML) view.
        /// </summary>
        HtmlViewBindingViewmodelNotifier ViewmodelNotifier { get; }

        /// <summary>
        /// Updates the view reading from the viewmodel.
        /// </summary>
        void ViewmodelToView();

        /// <summary>
        /// Gets the binding mode, which decides about the directions of the binding.
        /// </summary>
        HtmlViewBindingMode BindingMode { get; }
    }

    /// <summary>
    /// Generic implementation of the <see cref="IHtmlViewBinding"/> interface.
    /// It takes the "value" property of the HTML element.
    /// </summary>
    /// <typeparam name="T">The type of the data binding.</typeparam>
    public class HtmlViewBinding<T> : IHtmlViewBinding
    {
        private readonly Action<T> _viewSetter;
        private readonly Func<T> _viewmodelGetter;
        private readonly Action<T> _viewmodelSetter;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlViewBinding{T}"/> class.
        /// </summary>
        /// <param name="viewSetter">Can write the property to the (HTML) view.</param>
        /// <param name="viewNotifier">Informs about changes/clicks in the (HTML) view.</param>
        /// <param name="viewmodelGetter">Can read the property from the viewmodel.</param>
        /// <param name="viewmodelSetter">Can write the property to the viewmodel.</param>
        /// <param name="viewmodelNotifier">Informs about changes in the viewmodel.</param>
        /// <param name="bindingMode">The binding mode which defines the direction of the binding.</param>
        public HtmlViewBinding(
            Action<T> viewSetter,
            HtmlViewBindingViewNotifier viewNotifier,
            Func<T> viewmodelGetter,
            Action<T> viewmodelSetter,
            HtmlViewBindingViewmodelNotifier viewmodelNotifier,
            HtmlViewBindingMode bindingMode)
        {
            _viewSetter = viewSetter;
            ViewNotifier = viewNotifier;
            _viewmodelGetter = viewmodelGetter;
            _viewmodelSetter = viewmodelSetter;
            BindingMode = bindingMode;
            ViewmodelNotifier = viewmodelNotifier;

            if (ViewNotifier != null)
                ViewNotifier.Notified += ViewNotifiedEventHandler;
            if (ViewmodelNotifier != null)
                ViewmodelNotifier.Notified += ViewmodelNotifiedEventHandler;
        }

        /// <summary>
        /// Gets or sets a value which should be passed from the notify event of the view, to the
        /// viewmodel.
        /// </summary>
        protected object ValueFromViewEventHandler { get; set; }

        /// <inheritdoc/>
        public HtmlViewBindingViewNotifier ViewNotifier { get; protected set; }

        /// <inheritdoc/>
        public HtmlViewBindingViewmodelNotifier ViewmodelNotifier { get; protected set; }

        /// <inheritdoc/>
        public void ViewmodelToView()
        {
            if (BindingMode.In(new[] { HtmlViewBindingMode.OneWayToView, HtmlViewBindingMode.OneWayToViewPlusOneTimeToView, HtmlViewBindingMode.TwoWay, HtmlViewBindingMode.TwoWayPlusOneTimeToView })
                && (_viewSetter != null))
            {
                T value = (_viewmodelGetter != null) ? _viewmodelGetter() : default(T);
                _viewSetter(value);
            }
        }

        /// <summary>
        /// Gets the binding mode of this binding object.
        /// </summary>
        public HtmlViewBindingMode BindingMode { get; private set; }

        /// <summary>
        /// Updates the viewmodel reading from the view.
        /// </summary>
        protected void ViewToViewmodel()
        {
            if (BindingMode.In(new[] { HtmlViewBindingMode.Command, HtmlViewBindingMode.OneWayToViewmodel, HtmlViewBindingMode.TwoWay, HtmlViewBindingMode.TwoWayPlusOneTimeToView })
                && (_viewmodelSetter != null))
            {
                _viewmodelSetter((T)ValueFromViewEventHandler);
            }
        }

        /// <summary>
        /// Is called when the view triggered a notify event.
        /// </summary>
        /// <param name="parameters">Arguments passed from the view to the binding.</param>
        protected virtual void ViewNotifiedEventHandler(KeyValueList<string, string> parameters = null)
        {
            ValueFromViewEventHandler = parameters["value"];
            ViewToViewmodel();
        }

        /// <summary>
        /// Is called when the viewmodel triggered a notify event.
        /// </summary>
        /// <param name="value">Argument passed from the viewmodel to the binding.</param>
        protected virtual void ViewmodelNotifiedEventHandler(object value = null)
        {
            ViewmodelToView();
        }

        /// <summary>
        /// Checks whether all the getter, setter and notifier are set, which are required by the
        /// specified <see cref="BindingMode"/>.
        /// </summary>
        public void ValidateBindingModeOrThrow()
        {
            bool valid = true;
            switch (BindingMode)
            {
                case HtmlViewBindingMode.Command:
                    valid = (_viewSetter == null) && (ViewNotifier != null) && (_viewmodelGetter == null) && (_viewmodelSetter != null) && (ViewmodelNotifier == null);
                    break;
                case HtmlViewBindingMode.OneWayToView:
                case HtmlViewBindingMode.OneWayToViewPlusOneTimeToView:
                    valid = (_viewSetter != null) && (ViewNotifier == null) && (_viewmodelSetter == null) && (ViewmodelNotifier != null);
                    break;
                case HtmlViewBindingMode.OneWayToViewmodel:
                    valid = (_viewSetter == null) && (ViewNotifier != null) && (_viewmodelSetter != null) && (ViewmodelNotifier == null);
                    break;
                case HtmlViewBindingMode.TwoWay:
                case HtmlViewBindingMode.TwoWayPlusOneTimeToView:
                    valid = (_viewSetter != null) && (ViewNotifier != null) && (_viewmodelGetter != null) && (_viewmodelSetter != null) && (ViewmodelNotifier != null);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(string.Format("Unknown binding mode {0}.", BindingMode));
            }
            if (!valid)
                throw new Exception(string.Format("The defined getter, setter or notifier do not meet the requirements of the binding mode {0}.", BindingMode));
        }
    }
}
