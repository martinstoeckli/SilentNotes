using Microsoft.AspNetCore.Components.WebView.Maui;

namespace SilentNotes;

public partial class App : Application
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
    }

    /// <summary>
    /// Gets the service provider for IOC.
    /// </summary>
    public static IServiceProvider Ioc { get; private set; }
}
