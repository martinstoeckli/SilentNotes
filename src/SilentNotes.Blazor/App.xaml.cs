using System;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.WebView.Maui;
using SilentNotes.Services;

namespace SilentNotes;

public partial class App : Microsoft.Maui.Controls.Application
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="serviceProvider">Sets the <see cref="Ioc"/> property.</param>
	public App(IServiceProvider serviceProvider)
	{
		InitializeComponent();
        App.Ioc = serviceProvider;

        MainPage = new MainPage();

#if ANDROID
        // Workaround: Android soft keyboard hides the lower part of the content
        Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.UseWindowSoftInputModeAdjust(
            Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>(),
            Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust.Resize);
#endif
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);
#if WINDOWS
        window.Destroying += OnDestroying;
#endif
        return window;
    }

#if WINDOWS
    private void OnDestroying(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.Send<StoreUnsavedDataMessage>(new StoreUnsavedDataMessage());

        IAutoSynchronizationService syncService = App.Ioc.GetService<IAutoSynchronizationService>();
        syncService.SynchronizeAtShutdown().GetAwaiter().GetResult();
    }
#endif

    /// <summary>
    /// Gets the service provider for IOC.
    /// </summary>
    public static IServiceProvider Ioc { get; private set; }
}
