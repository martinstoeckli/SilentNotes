// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Web;
using System.Windows.Input;

namespace SilentNotes.HtmlView
{
    /// <summary>
    /// This class manages a list of data binding objects, to link a WebView to a viewmodel.
    /// It doesn't make use of reflection, so the applications "Linking" options can be set to
    /// optimize the code.
    /// </summary>
    public class HtmlViewBindings : IDisposable
    {
        private const string JsNamespace = "HtmlViewBinding";
        private const string BindingAttribute = "data-binding";
        private const string EventTypeAttribute = "event-type";
        private readonly List<IHtmlViewBinding> _bindings;
        private IHtmlView _htmlView;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlViewBindings"/> class.
        /// </summary>
        /// <param name="htmlView">View which contains the WebView to bind to.</param>
        public HtmlViewBindings(IHtmlView htmlView)
        {
            _bindings = new List<IHtmlViewBinding>();
            _htmlView = htmlView;
            _htmlView.Navigating += NavigatingEventHandler;
            _htmlView.NavigationCompleted += NavigationCompletedEventHandler;
        }

        /// <summary>
        /// Is triggered when the user started navigating. This is the place we can intercept
        /// the navigation.
        /// </summary>
        /// <param name="sender">The sender of the event or null.</param>
        /// <param name="uri">The navigation Uri.</param>
        private void NavigatingEventHandler(object sender, string uri)
        {
            // Read parameters from requested url
            Uri requestUrl = new Uri(uri);
            string page = Path.GetFileName(requestUrl.LocalPath);
            string queryPart = GetUriQueryPart(uri);
            NameValueCollection queryArguments = HttpUtility.ParseQueryString(queryPart);
            var arguments = new KeyValueList<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (string item in queryArguments)
                arguments[item] = queryArguments[item];

            // We intercept all navigations triggered by our own namespace, because they indicate
            // user clicks instead of real navigation requests to other pages.
            if (string.IsNullOrEmpty(page))
            {
                // Ignore links with href="#", because they are meant to be handled by a binding.
            }
            else if (page == JsNamespace)
            {
                var eventArgs = new HtmlViewBindingNotifiedEventArgs(arguments[BindingAttribute], arguments[EventTypeAttribute], arguments);

                HtmlViewBindingViewNotifier notifier = FindViewNotifier(eventArgs);
                if (notifier != null)
                {
                    // Matching notifier found, trigger the notifier
                    notifier.OnNotified(eventArgs.Parameters);
                }
                else
                {
                    // Raise event to let the controller handle the request
                    if (UnhandledViewBindingEvent != null)
                        UnhandledViewBindingEvent(this, eventArgs);
                }
            }
        }

        /// <summary>
        /// If the uri contains unicode characters, the Uri.Query sometimes throws an exception.
        /// </summary>
        /// <param name="uri">Uri string to get the query part from.</param>
        /// <returns>Query part of the uri.</returns>
        private string GetUriQueryPart(string uri)
        {
            int position = uri.IndexOf('?');
            if (position >= 0)
            {
                return uri.Substring(position);
            }
            return string.Empty;
        }

        private void NavigationCompletedEventHandler(object sender, EventArgs e)
        {
            InjectJavaScriptBindingFunctions();

            // Activate all notifiers, so they start listening to events in the Html view.
            foreach (IHtmlViewBinding binding in _bindings)
            {
                if (binding.ViewmodelNotifier != null)
                {
                    // Do an initial binding from the viewmodel to the view, this is the first
                    // time we can do this, because now the HTML has finished loading.
                    if (binding.BindingMode.In(new[] { HtmlViewBindingMode.OneWayToViewPlusOneTimeToView, HtmlViewBindingMode.TwoWayPlusOneTimeToView }))
                        binding.ViewmodelToView();
                    binding.ViewmodelNotifier.SubscribeEvents();
                }
            }
        }

        /// <summary>
        /// Tries to find the binding notifier matching the event.
        /// </summary>
        /// <param name="eventArgs">The event arguments with the binding name to search for.</param>
        /// <returns>Found notifier, or null if no such element could be found.</returns>
        private HtmlViewBindingViewNotifier FindViewNotifier(HtmlViewBindingNotifiedEventArgs eventArgs)
        {
            foreach (IHtmlViewBinding binding in _bindings)
            {
                HtmlViewBindingViewNotifier viewNotifier = binding.ViewNotifier;
                if ((viewNotifier != null) && viewNotifier.Matches(eventArgs.BindingName, eventArgs.EventType))
                    return viewNotifier;
            }
            return null;
        }

        /// <summary>
        /// This event is triggered when the Html view notified about a user action, but there is
        /// no binding which handle the event.
        /// </summary>
        public event NotifiedEventHandler UnhandledViewBindingEvent;

        /// <summary>
        /// Describes how the handler of the <see cref="UnhandledViewBindingEvent"/> must look like.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        public delegate void NotifiedEventHandler(object sender, HtmlViewBindingNotifiedEventArgs e);

        /// <summary>
        /// Creates a generic binding between an HTML element and a viewmodel. This method can
        /// achieve all behaviours the other bind methods can, but is more complex to use.
        /// All parameters are optional and null can be passed.
        /// </summary>
        /// <typeparam name="T">Type of the binding, use "object" if it is unimportant.</typeparam>
        /// <param name="viewSetter">Can write the property to the (HTML) view.</param>
        /// <param name="viewNotifier">Informs about changes/clicks in the (HTML) view.</param>
        /// <param name="viewmodelGetter">Can read the property from the viewmodel.</param>
        /// <param name="viewmodelSetter">Can write the property to the viewmodel.</param>
        /// <param name="viewmodelNotifier">Informs about changes in the viewmodel.</param>
        /// <param name="bindingMode">The binding mode which defines the direction of the binding.</param>
        public void BindGeneric<T>(
            Action<T> viewSetter,
            HtmlViewBindingViewNotifier viewNotifier,
            Func<T> viewmodelGetter,
            Action<T> viewmodelSetter,
            HtmlViewBindingViewmodelNotifier viewmodelNotifier,
            HtmlViewBindingMode bindingMode)
        {
            var binding = new HtmlViewBinding<T>(
                viewSetter, viewNotifier, viewmodelGetter, viewmodelSetter, viewmodelNotifier, bindingMode);
            binding.ValidateBindingModeOrThrow();
            _bindings.Add(binding);
        }

        /// <summary>
        /// Binds the click event of an HTML element to a command of the viewmodel.
        /// The HTML element needs this properties: onclick="bind(event)" data-binding="MyBindingName".
        /// </summary>
        /// <param name="bindingName">The name of the binding. The name is declared as
        /// "data-binding" attribute of the HTML element.</param>
        /// <param name="viewModelCommand">Command to execute when the control is clicked.</param>
        public void BindCommand(string bindingName, ICommand viewModelCommand)
        {
            BindGeneric<object>(
                null,
                new HtmlViewBindingViewNotifier(bindingName, "click"),
                null,
                (parameter) => viewModelCommand.Execute(parameter),
                null,
                HtmlViewBindingMode.Command);
        }

        /// <summary>
        /// Binds a text property of the viewmodel to an element in the (HTML) view.
        /// The HTML element needs this properties: oninput="bind(event)" data-binding="MyBindingName".
        /// </summary>
        /// <param name="bindingName">The name of the binding. The name is declared as
        /// "data-binding" attribute of the HTML element.</param>
        /// <param name="viewmodelGetter">Can read the property from the viewmodel.</param>
        /// <param name="viewmodelSetter">Can write the property to the viewmodel.</param>
        /// <param name="viewmodelNotifier">Usually the viewmodel itself, supporting the
        /// INotifyPropertyChanged interface.</param>
        /// <param name="viewmodelPropertyName">Name of the property in the viewmodel, whose
        /// changes should be listened for.</param>
        /// <param name="bindingMode">The binding mode which defines the direction of the binding.</param>
        public void BindText(string bindingName, Func<string> viewmodelGetter, Action<string> viewmodelSetter, INotifyPropertyChanged viewmodelNotifier, string viewmodelPropertyName, HtmlViewBindingMode bindingMode)
        {
            BindGeneric<string>(
                ViewSetterOrNullIfNotRequired<string>((value) => ViewValueSetter(bindingName, value), bindingMode),
                CreateViewNotifierOrNullIfNotRequired(bindingName, "input", bindingMode),
                viewmodelGetter,
                viewmodelSetter,
                CreateViewmodelNotifierOrNull(viewmodelNotifier, viewmodelPropertyName),
                bindingMode);
        }

        /// <summary>
        /// Binds a selected-element property of the viewmodel to a dropdown list in the (HTML) view.
        /// The HTML element needs this properties: onchange="bind(event)" data-binding="MyBindingName".
        /// The binding works with a string key, which can be converted by the <paramref name="viewmodelGetter"/>
        /// and the <paramref name="viewmodelSetter"/>.
        /// </summary>
        /// <param name="bindingName">The name of the binding. The name is declared as
        /// "data-binding" attribute of the HTML element.</param>
        /// <param name="viewmodelGetter">Can read the property from the viewmodel.</param>
        /// <param name="viewmodelSetter">Can write the property to the viewmodel.</param>
        /// <param name="viewmodelNotifier">Usually the viewmodel itself, supporting the
        /// INotifyPropertyChanged interface.</param>
        /// <param name="viewmodelPropertyName">Name of the property in the viewmodel, whose
        /// changes should be listened for.</param>
        /// <param name="bindingMode">The binding mode which defines the direction of the binding.</param>
        public void BindDropdown(string bindingName, Func<string> viewmodelGetter, Action<string> viewmodelSetter, INotifyPropertyChanged viewmodelNotifier, string viewmodelPropertyName, HtmlViewBindingMode bindingMode)
        {
            BindGeneric<string>(
                ViewSetterOrNullIfNotRequired<string>((value) => ViewValueSetter(bindingName, value), bindingMode),
                CreateViewNotifierOrNullIfNotRequired(bindingName, "change", bindingMode),
                viewmodelGetter,
                viewmodelSetter,
                CreateViewmodelNotifierOrNull(viewmodelNotifier, viewmodelPropertyName),
                bindingMode);
        }

        /// <summary>
        /// Binds a boolean property of the viewmodel to a checkbox in the (HTML) view.
        /// The HTML element needs this properties: onclick="bind(event)" data-binding="MyBindingName".
        /// </summary>
        /// <param name="bindingName">The name of the binding. The name is declared as
        /// "data-binding" attribute of the HTML element.</param>
        /// <param name="viewmodelGetter">Can read the property from the viewmodel.</param>
        /// <param name="viewmodelSetter">Can write the property to the viewmodel.</param>
        /// <param name="viewmodelNotifier">Usually the viewmodel itself, supporting the
        /// INotifyPropertyChanged interface.</param>
        /// <param name="viewmodelPropertyName">Name of the property in the viewmodel, whose
        /// changes should be listened for.</param>
        /// <param name="bindingMode">The binding mode which defines the direction of the binding.</param>
        public void BindCheckbox(string bindingName, Func<bool> viewmodelGetter, Action<bool> viewmodelSetter, INotifyPropertyChanged viewmodelNotifier, string viewmodelPropertyName, HtmlViewBindingMode bindingMode)
        {
            if (!bindingMode.In(new[] { HtmlViewBindingMode.OneWayToViewmodel }))
                throw new NotImplementedException("BindCheckbox expects the bindingMode to be OneWayToViewmodel, other modes are not supported.");

            var binding = new CheckboxHtmlViewBinding(
                null,
                CreateViewNotifierOrNullIfNotRequired(bindingName, "click", bindingMode),
                viewmodelGetter,
                viewmodelSetter,
                CreateViewmodelNotifierOrNull(viewmodelNotifier, viewmodelPropertyName),
                bindingMode);
            _bindings.Add(binding);
        }

        /// <summary>
        /// Binds a boolean property of the viewmodel to the visibility state of a control in the
        /// (HTML) view. This is a one way binding from the viewmodel to the view.
        /// </summary>
        /// <param name="bindingName">The name of the binding. The name is declared as
        /// "data-binding" attribute of the HTML element.</param>
        /// <param name="viewmodelGetter">Can read the property from the viewmodel.</param>
        /// <param name="viewmodelNotifier">Usually the viewmodel itself, supporting the
        /// INotifyPropertyChanged interface.</param>
        /// <param name="viewmodelPropertyName">Name of the property in the viewmodel, whose
        /// changes should be listened for.</param>
        /// <param name="bindingMode">The binding mode which defines the direction of the binding.</param>
        public void BindVisibility(string bindingName, Func<bool> viewmodelGetter, INotifyPropertyChanged viewmodelNotifier, string viewmodelPropertyName, HtmlViewBindingMode bindingMode)
        {
            if (!bindingMode.In(new[] { HtmlViewBindingMode.OneWayToView, HtmlViewBindingMode.OneWayToViewPlusOneTimeToView }))
                throw new Exception("BindVisibility expects the bindingMode to be either OneWayToView or OneWayToViewPlusOneTimeToView.");

            BindGeneric<bool>(
                ViewSetterOrNullIfNotRequired<bool>((value) => ViewVisibilitySetter(bindingName, value), bindingMode),
                null,
                viewmodelGetter,
                null,
                CreateViewmodelNotifierOrNull(viewmodelNotifier, viewmodelPropertyName),
                bindingMode);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _htmlView.Navigating -= NavigatingEventHandler;
            _bindings.Clear();
        }

        private Action<T> ViewSetterOrNullIfNotRequired<T>(Action<T> viewSetter, HtmlViewBindingMode bindingMode)
        {
            if (bindingMode.In(new[] { HtmlViewBindingMode.Command, HtmlViewBindingMode.OneWayToViewmodel }))
                return null;
            else
                return viewSetter;
        }

        private HtmlViewBindingViewNotifier CreateViewNotifierOrNullIfNotRequired(string bindingName, string jsEvent, HtmlViewBindingMode bindingMode)
        {
            if (bindingMode.In(new[] { HtmlViewBindingMode.Command, HtmlViewBindingMode.OneWayToView }))
                return null;
            else
                return new HtmlViewBindingViewNotifier(bindingName, jsEvent);
        }

        private HtmlViewBindingViewmodelNotifier CreateViewmodelNotifierOrNull(INotifyPropertyChanged viewmodelNotifier, string viewmodelPropertyName)
        {
            // Either both or none should be set
            bool notifierIsNull = (viewmodelNotifier == null);
            bool propertyNameIsNull = string.IsNullOrEmpty(viewmodelPropertyName);
            if (notifierIsNull != propertyNameIsNull)
                throw new Exception("Viewmodel notifier and viewmodel property name must be defined together.");

            if (!notifierIsNull && !propertyNameIsNull)
                return new HtmlViewBindingViewmodelNotifier(viewmodelNotifier, viewmodelPropertyName);
            return null;
        }

        private void ViewValueSetter(string bindingName, object value)
        {
            string valueText = value == null ? string.Empty : value.ToString();
            string script = string.Format(
                "htmlViewBindingsSetValue('{0}', '{1}');", bindingName, valueText);
            _htmlView.ExecuteJavaScript(script);
        }

        private void ViewVisibilitySetter(string bindingName, bool visible)
        {
            string script = string.Format(
                "htmlViewBindingsSetVisibility('{0}', {1});", bindingName, visible.ToString().ToLowerInvariant());
            _htmlView.ExecuteJavaScript(script);
        }

        /// <summary>
        /// Adds a JavaScript function to the html view, which can be called from HTML events to
        /// trigger a binding. The function can be called from an HTML element like this:
        /// <example><code>
        /// onclick='bind(event);'
        /// </code></example>
        /// The JavaScript function collects the id of the HTML element, its value and all
        /// data-* attributes and passes them to the binding. The data-binding attribute has a
        /// special meaning and should be set to identify the binding.
        /// </summary>
        private void InjectJavaScriptBindingFunctions()
        {
            const string JsBindFunction = @"
function bind(event)
{
    event = event || window.event;
    //event.preventDefault();

    var params = [];
    params['{2}'] = event.type;
    params['id'] = event.currentTarget.id;
    params['value'] = event.currentTarget.value;
    params['checked'] = event.currentTarget.checked;
    [].forEach.call(event.currentTarget.attributes, function(attr) {
        if (/^data-/.test(attr.name))
            params[attr.name] = attr.value;
    });

    var parts = [];
    for (var key in params) {
        var value = params[key];
        if (value)
            parts.push(key + '=' + encodeURIComponent(value));
    }

    var url = '{0}?' + parts.join('&');
    location.href = url;
}

function htmlViewBindingsSetValue(bindingName, value)
{
    var selector = '[{1}=""' + bindingName + '""]';
    $(selector).each(function()
    {
        var oldValue = $(this).val();
        if (value != oldValue)
            $(this).val(value);
    });
}

function htmlViewBindingsSetVisibility(bindingName, visible)
{
    var selector = '[{1}=""' + bindingName + '""]';
    if (visible)
        $(selector).show();
    else
        $(selector).hide();
}

function htmlViewBindingsSetCss(bindingName, cssAttributeName, cssAttributeValue)
{
    var selector = '[{1}=""' + bindingName + '""]';
    $(selector).css(cssAttributeName, cssAttributeValue);
}
";
            string script = JsBindFunction
                .Replace("{0}", JsNamespace)
                .Replace("{1}", BindingAttribute)
                .Replace("{2}", EventTypeAttribute);
            _htmlView.ExecuteJavaScript(script);
        }
    }
}
