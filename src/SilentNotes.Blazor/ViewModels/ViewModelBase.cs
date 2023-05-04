// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using SilentNotes.Services;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// Base class for all other view models.
    /// </summary>
    public abstract class ViewModelBase : ObservableObject //, IViewModel
    {
        ///// <summary>Gets the injected navigation service.</summary>
        //protected readonly INavigationService _navigationService;

        ///// <summary>Gets the injected webviewbaseurl service.</summary>
        //protected readonly IBaseUrlService _webviewBaseUrl;

        ///// <summary>
        ///// Initializes a new instance of the <see cref="ViewModelBase"/> class.
        ///// </summary>
        //protected ViewModelBase(
        //    INavigationService navigationService,
        //    ILanguageService languageService,
        //    ISvgIconService svgIconService,
        //    IThemeService themeService,
        //    IBaseUrlService webviewBaseUrl)
        //{
        //    _navigationService = navigationService;
        //    Language = languageService;
        //    Icon = svgIconService;
        //    Theme = themeService;
        //    _webviewBaseUrl = webviewBaseUrl;

        //    HtmlRecource = new HtmlRecourceService();
        //}

        ///// <inheritdoc/>
        //public virtual void OnClosing()
        //{
        //}

        /// <inheritdoc/>
        public virtual void OnStoringUnsavedData()
        {
        }

        ///// <inheritdoc/>
        //public abstract void OnGoBackPressed(out bool handled);

        /// <summary>
        /// Gets or sets a value indicating whether modifications where done to the
        /// view model. This property is automatically updated when calling <see cref="ChangeProperty{T}(ref T, T, bool, string)"/>.
        /// </summary>
        public bool Modified { get; set; }

        /// <summary>
        /// Assigns a new value to the property, raises the PropertyChanged event and sets the
        /// <see cref="Modified"/> flag if the value has changed.
        /// </summary>
        /// <typeparam name="T">The type of the property to change.</typeparam>
        /// <param name="oldValue">The old value of the property from the model.</param>
        /// <param name="newValue">The new value of the property.</param>
        /// <param name="setterToModel">Sets the value of the property to the model.</param>
        /// <param name="propertyName">(optional) The name of the property to change.</param>
        /// <returns>Returns true if the value really was different and has changed, otherwise false.</returns>
        protected bool SetPropertyAndModified<T>(T oldValue, T newValue, Action<T> setterToModel, [CallerMemberName] string propertyName = null)
        {
            bool result = SetProperty(oldValue, newValue, setterToModel, propertyName);
            if (result)
                Modified = true;
            return result;
        }

    //    /// <summary>
    //    /// Gets the platform specific base url, see also <see cref="IBaseUrlService"/>.
    //    /// </summary>
    //    public string HtmlBase
    //    {
    //        get { return _webviewBaseUrl.HtmlBase; }
    //    }

    //    /// <summary>
    //    /// Gets the version independend path to an Html recource <see cref="IHtmlRecourceService"/>.
    //    /// This property can be bound to in *.cshtml files, which is not possible with simple constants.
    //    /// </summary>
    //    public IHtmlRecourceService HtmlRecource { get; }

    //    /// <summary>
    //    /// Gets or sets a piece of JavaScript which initializes the Vue.js model and can be
    //    /// inserted into the HTML page. This script shouldn't be escaped.
    //    /// </summary>
    //    public string VueDataBindingScript { get; set; }

    //    /// <summary>
    //    /// Gets a bindable indexed property to load localized text resources.
    //    /// In Xaml one can use it like this:
    //    /// <code>
    //    /// Text="{Binding Language[TextResourceName]}"
    //    /// </code>
    //    /// In a Razor view (.cshtml) one can use it like this:
    //    /// <code>
    //    /// @Model.Language["TextResourceName"]
    //    /// </code>
    //    /// </summary>
    //    public ILanguageService Language { get; private set; }

    //    /// <summary>
    //    /// Gets the SVG icon service, which can load vector graphic icons.
    //    /// In a Razor view (.cshtml) one can use it like this:
    //    /// <code>
    //    /// @Model.Icon["IconResourceName"]
    //    /// </code>
    //    /// </summary>
    //    public ISvgIconService Icon { get; private set; }

    //    /// <summary>
    //    /// Gets the theme service, which controls the design of the application.
    //    /// </summary>
    //    public IThemeService Theme { get; private set; }
    }
}
