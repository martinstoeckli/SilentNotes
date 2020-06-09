using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Web;
using SilentNotes.Workers;

namespace SilentNotes.HtmlView
{
    public partial class VueJsDataBinding : IDisposable
    {
        /// <summary>The Vue instance in the View must be named like this.</summary>
        private const string VueInstanceName = "vm";

        /// <summary> The name of the javascript function, which is called on a Vue-model change.</summary>
        private const string VuePropertyChanged = "vuePropertyChanged"; // Name of the function 

        private readonly object _viewModel;
        private readonly INotifyPropertyChanged _viewModelNotifier;
        private readonly IHtmlView _htmlView;
        private readonly BindingDescriptions _bindingDescriptions;

        public VueJsDataBinding(object viewModel, IHtmlView htmlView, IEnumerable<BindingDescription> propertyBindings)
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));
            if (htmlView == null)
                throw new ArgumentNullException(nameof(htmlView));
            if (propertyBindings == null)
                throw new ArgumentNullException(nameof(propertyBindings));

            _viewModel = viewModel;
            _viewModelNotifier = viewModel as INotifyPropertyChanged;
            if (_viewModelNotifier == null)
                throw new ArgumentException("The parameter must support the interface INotifyPropertyChanged.", nameof(viewModel));
            _htmlView = htmlView;
            _bindingDescriptions = new BindingDescriptions(propertyBindings);

            _htmlView.Navigating += NavigatingEventHandler;
        }

        public void Dispose()
        {
            StopListening();
        }

        public bool IsListening { get; set; }

        public void StartListening()
        {
            if (!IsListening)
            {
                IsListening = true;
                _viewModelNotifier.PropertyChanged += ViewmodelPropertyChangedHandler;

                foreach (BindingDescription bindingDescription in _bindingDescriptions)
                {
                    if (bindingDescription.Mode == HtmlViewBindingMode.TwoWayPlusOneTimeToView)
                    {
                        object value = GetFromViewmodel(bindingDescription);
                        SetToView(bindingDescription, value);
                    }
                }
            }
        }

        public void StopListening()
        {
            if (IsListening)
            {
                IsListening = false;
                _viewModelNotifier.PropertyChanged -= ViewmodelPropertyChangedHandler;
            }
        }

        /// <summary>
        /// Gets a value from a property of the ViewModel.
        /// </summary>
        /// <param name="binding">The description of the property binding.</param>
        /// <returns>Value of the property.</returns>
        public object GetFromViewmodel(BindingDescription binding)
        {
            return _viewModel.GetType().GetProperty(binding.PropertyName).GetValue(_viewModel, null);
        }

        /// <summary>
        /// Sets the value of a property to the Vue-model in the View.
        /// </summary>
        /// <param name="binding">The description of the property binding.</param>
        /// <param name="value">New value of the property.</param>
        private void SetToView(BindingDescription binding, object value)
        {
            string script = string.Format(
                "if ({0}.{1} != {2}) {0}.{1} = {2};", VueInstanceName, binding.PropertyName, value.ToString());
            _htmlView.ExecuteJavaScript(script);
        }

        private void SetToViewmodel(BindingDescription binding, string value)
        {
            PropertyInfo propertyInfo = _viewModel.GetType().GetProperty(binding.PropertyName);
            if (propertyInfo != null)
            {
                Type propertyType = propertyInfo.PropertyType;
                if (propertyType == typeof(string))
                {
                    propertyInfo.SetValue(_viewModel, value);
                }
                else if (propertyType == typeof(int))
                {
                    int intValue = int.Parse(value);
                    propertyInfo.SetValue(_viewModel, intValue);
                }
            }
        }

        private void ViewmodelPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            BindingDescription binding = _bindingDescriptions.FindByPropertyName(e.PropertyName);
            if (binding != null)
            {
                object value = GetFromViewmodel(binding);
                SetToView(binding, value);
            }
        }

        private void NavigatingEventHandler(object sender, string uri)
        {
            if (!IsListening || !IsVueBindingUri(uri))
                return;

            string queryPart = GetUriQueryPart(uri);
            NameValueCollection queryArguments = HttpUtility.ParseQueryString(queryPart);
            string propertyName = queryArguments.Get("name");
            string value = queryArguments.Get("value");

            BindingDescription binding = _bindingDescriptions.FindByPropertyName(propertyName);
            if (binding != null)
            {
                SetToViewmodel(binding, value);
            }
        }

        private bool IsVueBindingUri(string uri)
        {
            return !string.IsNullOrEmpty(uri) && uri.Contains(VuePropertyChanged) && !WebviewUtils.IsExternalUri(uri);
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
    }
}
