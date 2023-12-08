using CommunityToolkit.Mvvm.Messaging;
using SilentNotes.Workers;

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
        var webView2 = (blazorWebView.Handler.PlatformView as Microsoft.UI.Xaml.Controls.WebView2);
        await webView2.EnsureCoreWebView2Async();

        // Set the splash screen background color, shown from when the webview is displayed until
        // the the index.html is loaded.
        var backgroundColor = ColorExtensions.HexToColor("#4793d1");
        webView2.DefaultBackgroundColor = Windows.UI.Color.FromArgb(
            backgroundColor.A, backgroundColor.R, backgroundColor.G, backgroundColor.B);
#endif
#if (WINDOWS && !DEBUG)
        var settings = webView2.CoreWebView2.Settings;
        settings.AreBrowserAcceleratorKeysEnabled = false; // Only in debug mode we need ctrl-shift-i to open the developer view
        settings.IsPasswordAutosaveEnabled = false;
#endif

        await Task.CompletedTask;
    }
}
