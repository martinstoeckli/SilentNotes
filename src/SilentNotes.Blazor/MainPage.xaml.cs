using CommunityToolkit.Mvvm.Messaging;

namespace SilentNotes;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
        Loaded += LoadedEventHandler;
	}

    private async void LoadedEventHandler(object sender, EventArgs e)
    {
        Loaded -= LoadedEventHandler;
#if (WINDOWS)
        var splashColor = (Microsoft.Maui.Graphics.Color)App.Current.Resources["SplashBackgroundColor"];
        var splashUiColor = Windows.UI.Color.FromArgb((byte)(splashColor.Alpha * 255), (byte)(splashColor.Red * 255), (byte)(splashColor.Green * 255), (byte)(splashColor.Blue * 255));
        var webView2 = (blazorWebView.Handler.PlatformView as Microsoft.UI.Xaml.Controls.WebView2);
        await webView2.EnsureCoreWebView2Async();
        webView2.DefaultBackgroundColor = splashUiColor;
#else
        await Task.CompletedTask;
#endif

#if (WINDOWS && !DEBUG)
        var settings = webView2.CoreWebView2.Settings;
        settings.AreBrowserAcceleratorKeysEnabled = false; // In debug mode we need ctrl-shift-i to open the developer view
        settings.IsPasswordAutosaveEnabled = false;
#endif
    }
}
