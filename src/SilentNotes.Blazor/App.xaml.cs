using System;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace SilentNotes;

public partial class App : Application, IDisposable
{
    private IDisposable _locationChangingHandler;

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
    }

    public void Dispose()
    {
        _locationChangingHandler?.Dispose();
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
        //var navigation = Ioc.GetService<NavigationManager>();
        //_locationChangingHandler = navigation.RegisterLocationChangingHandler(CheckFromState);
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

    //private ValueTask CheckFromState(LocationChangingContext context)
    //{

    //    return ValueTask.CompletedTask;
    //}
}
