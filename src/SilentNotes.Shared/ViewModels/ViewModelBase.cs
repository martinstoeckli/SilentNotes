// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SilentNotes.Services;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// Base class for all other view models.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged, IViewModel
    {
        /// <summary>Gets the injected navigation service.</summary>
        protected readonly INavigationService _navigationService;

        /// <summary>Gets the injected webviewbaseurl service.</summary>
        protected readonly IBaseUrlService _webviewBaseUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase"/> class.
        /// </summary>
        protected ViewModelBase(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IThemeService themeService,
            IBaseUrlService webviewBaseUrl)
        {
            _navigationService = navigationService;
            Language = languageService;
            Icon = svgIconService;
            Theme = themeService;
            _webviewBaseUrl = webviewBaseUrl;
        }

        /// <inheritdoc/>
        public virtual void OnClosing()
        {
        }

        /// <inheritdoc/>
        public virtual void OnStoringUnsavedData()
        {
        }

        /// <inheritdoc/>
        public abstract void OnGoBackPressed(out bool handled);

        /// <summary>
        /// Gets or sets a value indicating whether modifications where done to the
        /// view model. This property is automatically updated when calling <see cref="ChangeProperty{T}(ref T, T, bool, string)"/>.
        /// </summary>
        public bool Modified { get; set; }

        /// <summary>
        /// This event can be consumed by a view, to find out when a certain property has changed,
        /// so it can be updated.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the property changed event, to inform the view of changes.
        /// Calling this method is usually not necessary, because the method <see cref="ChangeProperty{T}(ref T, T, bool, string)"/>
        /// will call it automatically.
        /// </summary>
        /// <param name="propertyName">Name of the property which has changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Assigns a new value to the property and raises the PropertyChanged event if necessary.
        /// </summary>
        /// <typeparam name="T">The type of the property to change.</typeparam>
        /// <param name="propertyMember">The backing field in the viewmodel.</param>
        /// <param name="newValue">The new value of the property.</param>
        /// <param name="updateModified">Value indicating whether the <see cref="Modified"/> flag
        /// should be updated if the value changes.</param>
        /// <param name="propertyName">(optional) The name of the property to change.</param>
        /// <returns>Returns true if the PropertyChanged event was raised, otherwise false.</returns>
        protected bool ChangeProperty<T>(ref T propertyMember, T newValue, bool updateModified, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(propertyMember, newValue))
                return false;

            propertyMember = newValue;
            OnPropertyChanged(propertyName);
            if (updateModified)
                Modified = true;
            return true;
        }

        /// <summary>
        /// Assigns a new value to the property and raises the PropertyChanged event if necessary.
        /// </summary>
        /// <typeparam name="T">The type of the property to change.</typeparam>
        /// <param name="getterFromModel">Gets the value of the property from the model.</param>
        /// <param name="setterToModel">Sets the value of the property to the model.</param>
        /// <param name="newValue">The new value of the property.</param>
        /// <param name="updateModified">Value indicating whether the <see cref="Modified"/></param>
        /// should be updated if the value changes.
        /// <param name="propertyName">(optional) The name of the property to change.</param>
        /// <returns>Returns true if the PropertyChanged event was raised, otherwise false.</returns>
        protected bool ChangePropertyIndirect<T>(Func<T> getterFromModel, Action<T> setterToModel, T newValue, bool updateModified, [CallerMemberName] string propertyName = null)
        {
            T oldValue = getterFromModel();

            // If both values are equal, then do not change anything
            if ((oldValue == null) && (newValue == null))
                return false;
            if ((oldValue != null) && oldValue.Equals(newValue))
                return false;

            setterToModel(newValue);
            OnPropertyChanged(propertyName);
            if (updateModified)
                Modified = true;
            return true;
        }

        /// <summary>
        /// Gets the platform specific base url, see also <see cref="IBaseUrlService"/>.
        /// </summary>
        public string HtmlBase
        {
            get { return _webviewBaseUrl.HtmlBase; }
        }

        /// <summary>
        /// Gets or sets a piece of JavaScript which initializes the Vue.js model and can be
        /// inserted into the HTML page. This script shouldn't be escaped.
        /// </summary>
        public string VueDataBindingScript { get; set; }

        /// <summary>
        /// Gets a bindable indexed property to load localized text resources.
        /// In Xaml one can use it like this:
        /// <code>
        /// Text="{Binding Language[TextResourceName]}"
        /// </code>
        /// In a Razor view (.cshtml) one can use it like this:
        /// <code>
        /// @Model.Language["TextResourceName"]
        /// </code>
        /// </summary>
        public ILanguageService Language { get; private set; }

        /// <summary>
        /// Gets the SVG icon service, which can load vector graphic icons.
        /// In a Razor view (.cshtml) one can use it like this:
        /// <code>
        /// @Model.Icon["IconResourceName"]
        /// </code>
        /// </summary>
        public ISvgIconService Icon { get; private set; }

        /// <summary>
        /// Gets the theme service, which controls the design of the application.
        /// </summary>
        public IThemeService Theme { get; private set; }
    }
}
