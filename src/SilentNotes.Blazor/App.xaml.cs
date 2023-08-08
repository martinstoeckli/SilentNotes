using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.WebView.Maui;

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
        MainPage.Disappearing += DisappearingEventHandler;

#if ANDROID
        // Workaround: Android soft keyboard hides the lower part of the content
        Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.UseWindowSoftInputModeAdjust(
            Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>(),
            Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust.Resize);
#endif
    }

    private void DisappearingEventHandler(object sender, EventArgs e)
    {
    }

    /// <summary>
    /// Gets the service provider for IOC.
    /// </summary>
    public static IServiceProvider Ioc { get; private set; }

    protected override void OnStart()
    {
        base.OnStart();
    }

    protected override void OnResume()
    {
        base.OnResume();
    }

    protected override void OnSleep()
    {
        base.OnSleep();
    }

    protected override void CleanUp()
    {
        base.CleanUp();
    }
}
