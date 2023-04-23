using Microsoft.AspNetCore.Components.WebView.Maui;

namespace SilentNotes;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new MainPage();
        // todo: stom
        BlazorWebView webView = MainPage.FindByName("blazorWebView") as BlazorWebView;
    }
}
