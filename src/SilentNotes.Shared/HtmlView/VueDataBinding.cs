// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Web;
using System.Windows.Input;
using SilentNotes.Workers;

namespace SilentNotes.HtmlView
{
    /// <summary>
    /// Acts as data binding between a C# viewmodel and a Vue model inside an HTML page.
    /// This allows to use an HTML page as view and still have a data binding. Bindable properties
    /// are automatically detected when marked with the <see cref="VueDataBindingAttribute"/>.
    /// </summary>
    public class VueDataBinding : IDisposable
    {
        private readonly object _dotnetViewModel;
        private readonly INotifyPropertyChanged _viewModelNotifier;
        private readonly IHtmlView _htmlView;
        private readonly VueBindingDescriptions _bindingDescriptions;
        private readonly List<VueBindingShortcut> _bindingShortcuts;
        private readonly List<KeyValuePair<string, string>> _additionalVueDatas;
        private readonly List<KeyValuePair<string, string>> _additionalVueMethods;

        /// <summary>
        /// Initializes a new instance of the <see cref="VueDataBinding"/> class.
        /// </summary>
        /// <param name="dotnetViewModel">A C# viewmodel, which must support the <see cref="INotifyPropertyChanged"/>
        /// interface.</param>
        /// <param name="htmlView">A html view interface.</param>
        /// <param name="shortcuts">Optional enumeration of keyboard shortcuts.</param>
        public VueDataBinding(object dotnetViewModel, IHtmlView htmlView, IEnumerable<VueBindingShortcut> shortcuts = null)
        {
            if (dotnetViewModel == null)
                throw new ArgumentNullException(nameof(dotnetViewModel));
            if (htmlView == null)
                throw new ArgumentNullException(nameof(htmlView));

            _dotnetViewModel = dotnetViewModel;
            _viewModelNotifier = dotnetViewModel as INotifyPropertyChanged;
            if (_viewModelNotifier == null)
                throw new ArgumentException("The parameter must support the interface INotifyPropertyChanged.", nameof(dotnetViewModel));
            _htmlView = htmlView;

            // Search for properties which require data binding
            _bindingDescriptions = new VueBindingDescriptions(DetectMarkedViewmodelAttributes(_dotnetViewModel));
            _bindingShortcuts = shortcuts != null ? new List<VueBindingShortcut>(shortcuts) : null;
            _additionalVueDatas = new List<KeyValuePair<string, string>>();
            _additionalVueMethods = new List<KeyValuePair<string, string>>();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            StopListening();
        }

        /// <summary>
        /// Creates the JavaScript which creates a Vue instance and adds the necessary properties
        /// and event handlers. The result can be inserted into a script tag of an HTML page, or it
        /// can be executed when the DOM content is loaded.
        /// </summary>
        /// <returns>JavaScript code which can be executed or placed inside an HTML view.</returns>
        public string BuildVueScript()
        {
            StringBuilder vueScript = new StringBuilder(@"
function vueReady(fn) {
    if (document.readyState != 'loading') {
        fn();
    } else {
        document.addEventListener('DOMContentLoaded', fn);
    }
}

function vuePropertyChanged(propertyName, value) {
    var encodedValue = encodeURIComponent(value);
    var url = 'vuePropertyChanged?name=' + propertyName + '&value=' + encodedValue;
    location.href = url;
}

function vueCommandExecuted(commandName) {
    var url = 'vueCommandExecuted?name=' + commandName;
    location.href = url;
}

function vueFindCommandByShortcut(e)
{
    if (event.isComposing)
        return null;
    [VUE_SHORTCUTS]
    return null;
}

var vm;
vueReady(function () {
    var _this = this;
    vm = new Vue({
        el: '#vueDataBinding',
        data: {
            [VUE_DATA_DECLARATIONS]
        },
        methods: {
            [VUE_METHOD_DECLARATIONS]
        },
        watch: {
            [VUE_WATCHES]
        },
        mounted: function() {
            this._shortcutListener = function(e) {
                var command = vueFindCommandByShortcut(e)
                if (command) {
                    e.preventDefault();
                    this[command]();
                }
            };
            document.addEventListener('keydown', this._shortcutListener.bind(this));
        },
        beforeDestroy: function() {
            document.removeEventListener('keydown', this._shortcutListener);
        },
    });
});
");

            List<string> vueDatas = new List<string>();
            foreach (VueBindingDescription binding in _bindingDescriptions)
            {
                if ((binding.BindingMode == VueBindingMode.TwoWay) || (binding.BindingMode == VueBindingMode.OneWayToView))
                {
                    TryGetFromViewmodel(binding, out object propertyValue);
                    TryFormatForView(binding, propertyValue, out string formattedValue);

                    // MyProperty: 42,
                    vueDatas.Add(string.Format(
                        "{0}: {1},",
                        binding.PropertyName,
                        formattedValue));
                }
            }
            foreach (var additionalVueData in _additionalVueDatas)
            {
                vueDatas.Add(string.Format("{0}: {1},", additionalVueData.Key, additionalVueData.Value));
            }

            List<string> vueMethods = new List<string>();
            foreach (VueBindingDescription binding in _bindingDescriptions)
            {
                if (binding.BindingMode == VueBindingMode.Command)
                {
                    // MyMethod: function() { vueCommandExecuted('MyMethod'); },
                    vueMethods.Add(string.Format(
                        "{0}: function() {{ vueCommandExecuted('{0}'); }},",
                        binding.PropertyName));
                }
            }
            foreach (var additionalVueMethod in _additionalVueMethods)
            {
                vueMethods.Add(string.Format("{0}: function() {{ {1} }},", additionalVueMethod.Key, additionalVueMethod.Value));
            }

            List<string> vueWatches = new List<string>();
            foreach (VueBindingDescription binding in _bindingDescriptions)
            {
                if (binding.BindingMode == VueBindingMode.TwoWay)
                {
                    // MyProperty: function(newVal, oldVal) { vuePropertyChanged('MyProperty', newVal); },
                    vueWatches.Add(string.Format(
                        "{0}: function(newVal) {{ vuePropertyChanged('{0}', newVal); }},",
                        binding.PropertyName));
                }
            }

            List<string> vueShortcuts = new List<string>();
            if (_bindingShortcuts != null)
            {
                foreach (VueBindingShortcut shortcut in _bindingShortcuts)
                {
                    // Return command if keydown event (e) matches a known shortcut
                    vueShortcuts.Add(string.Format(
                        "if (e.key === '{0}' && e.ctrlKey == {1} && e.shiftKey == {2} && e.altKey == {3}) return '{4}';",
                        shortcut.Key,
                        shortcut.Ctrl.ToString().ToLowerInvariant(),
                        shortcut.Shift.ToString().ToLowerInvariant(),
                        shortcut.Alt.ToString().ToLowerInvariant(),
                        shortcut.CommandName));
                }
            }

            vueScript.Replace("[VUE_DATA_DECLARATIONS]", string.Join("\n", vueDatas));
            vueScript.Replace("[VUE_METHOD_DECLARATIONS]", string.Join("\n", vueMethods));
            vueScript.Replace("[VUE_WATCHES]", string.Join("\n", vueWatches));
            vueScript.Replace("[VUE_SHORTCUTS]", string.Join("\n", vueShortcuts));
            string result = vueScript.ToString();
            return result;
        }

        /// <summary>
        /// Creates and executes the JavaScript which creates a Vue instance and adds the necessary
        /// properties and event handlers. Call this method not before the DOM content is loaded,
        /// alternatively the script can be inserted directly into the HTML, see <see cref="BuildVueScript"/>.
        /// </summary>
        public void InitializeVue()
        {
            string vueScript = BuildVueScript();
            _htmlView.ExecuteJavaScript(vueScript);
        }

        /// <summary>
        /// Gets a value indicating whether the vue data binding is currently listening to events.
        /// </summary>
        public bool IsListening { get; private set; }

        /// <summary>
        /// Starts listening to events from the view and from the viewmodel.
        /// </summary>
        public void StartListening()
        {
            if (!IsListening)
            {
                IsListening = true;
                _htmlView.Navigating += NavigatingEventHandler;
                _viewModelNotifier.PropertyChanged += ViewmodelPropertyChangedHandler;
            }
        }

        /// <summary>
        /// Stops listening to events from the view and from the viewmodel.
        /// </summary>
        public void StopListening()
        {
            if (IsListening)
            {
                _viewModelNotifier.PropertyChanged -= ViewmodelPropertyChangedHandler;
                _htmlView.Navigating -= NavigatingEventHandler;
                IsListening = false;
            }
        }

        /// <summary>
        /// Adds an additional entry to the Vue.data collection, which is not automatically created
        /// because it has an attribute in the viewmodel, but should be available in Vue instance
        /// anyway for the view itself.
        /// </summary>
        /// <param name="name">Name of the data entry.</param>
        /// <param name="value">Value of the data entry.</param>
        public void DeclareAdditionalVueData(string name, string value)
        {
            _additionalVueDatas.Add(new KeyValuePair<string, string>(name, value));
        }

        /// <summary>
        /// Adds an additional entry to the Vue.methods collection, which is not automatically created
        /// because it has an attribute in the viewmodel, but should be available in Vue instance
        /// anyway for the view itself (e.g. for shortcuts).
        /// </summary>
        /// <param name="name">Name of the data entry.</param>
        /// <param name="javascript">Java script command.</param>
        public void DeclareAdditionalVueMethod(string name, string javascript)
        {
            _additionalVueMethods.Add(new KeyValuePair<string, string>(name, javascript));
        }

        /// <summary>
        /// This event is triggered when the Html view notified about a user action, but there is
        /// no binding which handles the event. This event can be triggered deliberately in the
        /// HTML view by calling:
        /// <example><code>
        ///   vuePropertyChanged('UnknownPropertyName', newValue);
        /// </code></example>
        /// </summary>
        public event EventHandler<VueBindingUnhandledViewBindingEventArgs> UnhandledViewBindingEvent;

        internal bool TryFormatForView(VueBindingDescription binding, object value, out string formattedValue)
        {
            PropertyInfo propertyInfo = _dotnetViewModel.GetType().GetProperty(binding.PropertyName);
            if (propertyInfo != null)
            {
                Type propertyType = propertyInfo.PropertyType;
                if (value == null)
                {
                    formattedValue = "null";
                    return true;
                }
                else if (propertyType == typeof(string))
                {
                    formattedValue = "'" + WebviewUtils.EscapeJavaScriptString(value.ToString()) + "'";
                    return true;
                }
                else if (propertyType == typeof(bool))
                {
                    formattedValue = value.ToString().ToLowerInvariant();
                    return true;
                }
                else if (propertyType == typeof(int))
                {
                    formattedValue = value.ToString();
                    return true;
                }
            }
            formattedValue = null;
            return false;
        }

        internal bool TryParseFromView(VueBindingDescription binding, string formattedValue, out object value)
        {
            PropertyInfo propertyInfo = _dotnetViewModel.GetType().GetProperty(binding.PropertyName);
            if (propertyInfo != null)
            {
                try
                {
                    Type propertyType = propertyInfo.PropertyType;
                    if (string.Equals("null", formattedValue, StringComparison.InvariantCultureIgnoreCase))
                    {
                        value = null;
                        return true;
                    }
                    else if (propertyType == typeof(string))
                    {
                        value = formattedValue;
                        return true;
                    }
                    else if (propertyType == typeof(bool))
                    {
                        value = bool.Parse(formattedValue);
                        return true;
                    }
                    else if (propertyType == typeof(int))
                    {
                        value = int.Parse(formattedValue);
                        return true;
                    }
                }
                catch (Exception)
                {
                    // This will return false
                }
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Gets a value from a property of the ViewModel.
        /// </summary>
        /// <param name="binding">The description of the property binding.</param>
        /// <param name="propertyValue">Value of the property.</param>
        /// <returns>Returns true if the value could be retrieved successfully, otherwise false.</returns>
        private bool TryGetFromViewmodel(VueBindingDescription binding, out object propertyValue)
        {
            PropertyInfo propertyInfo = _dotnetViewModel.GetType().GetProperty(binding.PropertyName);
            if ((propertyInfo != null) && (propertyInfo.CanRead))
            {
                propertyValue = propertyInfo.GetValue(_dotnetViewModel, null);
                return true;
            }
            propertyValue = null;
            return false;
        }

        /// <summary>
        /// Sets a value to the Vue-model in the View.
        /// </summary>
        /// <param name="binding">The description of the property binding.</param>
        /// <param name="value">New value of the property.</param>
        /// <returns>Returns true if the value could be set successfully, otherwise false.</returns>
        private bool TrySetToView(VueBindingDescription binding, object value)
        {
            if (TryFormatForView(binding, value, out string formattedValue))
            {
                try
                {
                    string script = string.Format(
                        "var newValue = {2}; if ({0}.{1} != newValue) {0}.{1} = newValue;", "vm", binding.PropertyName, formattedValue);
                    _htmlView.ExecuteJavaScript(script);
                    return true;
                }
                catch (Exception)
                {
                    // This will return false
                }
            }
            return false;
        }

        private bool TrySetToViewmodel(VueBindingDescription binding, string formattedValue)
        {
            if (string.IsNullOrEmpty(binding?.PropertyName))
                return false;

            if (TryParseFromView(binding, formattedValue, out object value))
            {
                PropertyInfo propertyInfo = _dotnetViewModel.GetType().GetProperty(binding.PropertyName);
                if ((propertyInfo != null) && (propertyInfo.CanWrite))
                {
                    propertyInfo.SetValue(_dotnetViewModel, value);
                    return true;
                }
            }
            return false;
        }

        private bool TryExecuteCommand(VueBindingDescription binding)
        {
            PropertyInfo propertyInfo = _dotnetViewModel.GetType().GetProperty(binding.PropertyName);
            if ((propertyInfo != null) && (propertyInfo.PropertyType == typeof(ICommand)))
            {
                ICommand command = propertyInfo.GetValue(_dotnetViewModel, null) as ICommand;
                command.Execute(null);
                return true;
            }
            return false;
        }

        /// <summary>
        /// This method is called by the JavaScript, triggered by the Vue model.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="uri">The navigation uri, which should be filtered for binding commands.</param>
        private void NavigatingEventHandler(object sender, string uri)
        {
            if (!IsListening || !IsVueBindingUri(uri))
                return;

            string queryPart = GetUriQueryPart(uri);
            NameValueCollection queryArguments = HttpUtility.ParseQueryString(queryPart);
            string propertyName = queryArguments.Get("name");
            string value = queryArguments.Get("value");

            VueBindingDescription binding = _bindingDescriptions.FindByPropertyName(propertyName);
            if (binding != null)
            {
                switch (binding.BindingMode)
                {
                    case VueBindingMode.TwoWay:
                    case VueBindingMode.OneWayToViewmodel:
                        TrySetToViewmodel(binding, value);
                        break;
                    case VueBindingMode.Command:
                        TryExecuteCommand(binding);
                        break;
                    case VueBindingMode.OneWayToView:
                        break; // Should never happen
                    default:
                        throw new ArgumentOutOfRangeException(nameof(binding.BindingMode));
                }
            }
            else
            {
                UnhandledViewBindingEvent?.Invoke(this, new VueBindingUnhandledViewBindingEventArgs(propertyName, value));
            }
        }

        /// <summary>
        /// This method is called when the viewmodel modified a property and raised an
        /// INotifyPropertyChanged event.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private void ViewmodelPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (!IsListening)
                return;

            VueBindingDescription binding = _bindingDescriptions.FindByPropertyName(e.PropertyName);
            if (binding != null)
            {
                if (TryGetFromViewmodel(binding, out object propertyValue))
                    TrySetToView(binding, propertyValue);
            }
        }

        private bool IsVueBindingUri(string uri)
        {
            return !string.IsNullOrEmpty(uri)
                && (uri.Contains("vuePropertyChanged?") || uri.Contains("vueCommandExecuted?"))
                && !WebviewUtils.IsExternalUri(uri);
        }

        /// <summary>
        /// If the uri contains unicode characters, the Uri.Query sometimes throws an exception.
        /// </summary>
        /// <param name="uri">Uri string to get the query part from.</param>
        /// <returns>Query part of the uri.</returns>
        private static string GetUriQueryPart(string uri)
        {
            int position = uri.IndexOf('?');
            if (position >= 0)
            {
                return uri.Substring(position);
            }
            return string.Empty;
        }

        /// <summary>
        /// Searches the view model for properties marked with the <see cref="VueDataBindingAttribute"/>.
        /// </summary>
        /// <param name="viewModel">View model to search.</param>
        /// <returns>Enumeration of found propertie with their binding information.</returns>
        private IEnumerable<VueBindingDescription> DetectMarkedViewmodelAttributes(object viewModel)
        {
            Type type = viewModel.GetType();
            PropertyInfo[] props = type.GetProperties();
            foreach (PropertyInfo prop in props)
            {
                VueDataBindingAttribute bindingAttribute = prop.GetCustomAttribute<VueDataBindingAttribute>();
                if (bindingAttribute != null)
                {
                    VueBindingMode bindingMode = bindingAttribute.BindingMode;
                    if (prop.PropertyType == typeof(ICommand))
                        bindingMode = VueBindingMode.Command;

                    yield return new VueBindingDescription(prop.Name, bindingMode);
                }
            }
        }
    }
}
