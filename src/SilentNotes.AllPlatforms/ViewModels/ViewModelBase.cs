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
    public abstract class ViewModelBase : ObservableObject
    {
    }
}
