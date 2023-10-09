// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Platform;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SilentNotes.Services;
using Windows.ApplicationModel.ExtendedExecution;
using WinRT.Interop;

namespace SilentNotes.Platforms
{
    /// <summary>
    /// Handles application lifecycle events for the Windows platform.
    /// Possible events are listed here: https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/app-lifecycle
    /// </summary>
    internal class ApplicationEventHandler : ApplicationEventHandlerBase
    {
        internal void OnClosed(Microsoft.UI.Xaml.Window window, WindowEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("*** ApplicationEventHandler.OnClosed()");
            WeakReferenceMessenger.Default.Send<StoreUnsavedDataMessage>(new StoreUnsavedDataMessage());

            // We need to wait for the end of the synchronization, otherwise the app exits before
            // the work is done.
            var synchronizationService = Ioc.Instance.GetService<ISynchronizationService>();
            Task.Run(() => synchronizationService.AutoSynchronizeAtShutdown(Ioc.Instance)).Wait();
        }
    }
}
