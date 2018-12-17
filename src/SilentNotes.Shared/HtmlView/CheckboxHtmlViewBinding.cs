// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;

namespace SilentNotes.HtmlView
{
    /// <summary>
    /// Variation of the <see cref="HtmlViewBinding{T}"/> to handle checkboxes.
    /// </summary>
    public class CheckboxHtmlViewBinding : HtmlViewBinding<bool>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckboxHtmlViewBinding"/> class.
        /// </summary>
        /// <param name="viewSetter">Can write the property to the (HTML) view.</param>
        /// <param name="viewNotifier">Informs about changes/clicks in the (HTML) view.</param>
        /// <param name="viewmodelGetter">Can read the property from the viewmodel.</param>
        /// <param name="viewmodelSetter">Can write the property to the viewmodel.</param>
        /// <param name="viewmodelNotifier">Informs about changes in the viewmodel.</param>
        /// <param name="bindingMode">The binding mode which defines the direction of the binding.</param>
        public CheckboxHtmlViewBinding(Action<bool> viewSetter, HtmlViewBindingViewNotifier viewNotifier, Func<bool> viewmodelGetter, Action<bool> viewmodelSetter, HtmlViewBindingViewmodelNotifier viewmodelNotifier, HtmlViewBindingMode bindingMode)
            : base(viewSetter, viewNotifier, viewmodelGetter, viewmodelSetter, viewmodelNotifier, bindingMode)
        {
        }

        /// <inheritdoc/>
        protected override void ViewNotifiedEventHandler(KeyValueList<string, string> parameters = null)
        {
            bool boolValue = parameters["checked"] != null;
            ValueFromViewEventHandler = boolValue;
            ViewToViewmodel();
        }
    }
}
