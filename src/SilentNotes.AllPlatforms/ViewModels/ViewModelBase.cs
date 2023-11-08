// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using MudBlazor;
using SilentNotes.Services;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// Base class for all other view models.
    /// </summary>
    public abstract class ViewModelBase : ObservableObject //, IViewModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelBase"/> class.
        /// </summary>
        protected ViewModelBase()
        {
        }

        ///// <inheritdoc/>
        //public abstract void OnGoBackPressed(out bool handled);

        /// <summary>
        /// Gets or sets a value indicating whether modifications where done to the
        /// view model. This property is automatically updated when calling <see cref="SetPropertyAndModified{T}(T, T, Action{T}, string)"/>.
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
    }
}
