using Microsoft.AspNetCore.Components.WebView;
using SilentNotes.Workers;

namespace SilentNotes;

public partial class MainPage : ContentPage
{
    public static string StartRoute = RouteNames.NoteRepository;

    public MainPage()
    {
        InitializeComponent();
        blazorWebView.StartPath = StartRoute;
        blazorWebView.BlazorWebViewInitializing += WebViewInitializingEventHandler;
        blazorWebView.BlazorWebViewInitialized += WebViewInitializedEventHandler;
    }

    /// <summary>
    /// Tweak the WebView environment, before the WebView is initialized.
    /// </summary>
    private void WebViewInitializingEventHandler(object sender, BlazorWebViewInitializingEventArgs e)
    {
        blazorWebView.BlazorWebViewInitializing -= WebViewInitializingEventHandler;
#if (WINDOWS)
        // Workaround: On Windows we have to convince the WebView to use the user language (e.g. for auto correction).
        var arguments = new KeyValueList<string, string>();
        arguments["--lang"] = Windows.System.UserProfile.GlobalizationPreferences.Languages.FirstOrDefault();
        arguments["--disable-features"] = "msSmartScreenProtection";
        string additionalBrowserArguments = string.Join(" ", arguments.Select(argument => argument.Key + "=" + argument.Value));

        if (e.EnvironmentOptions == null)
            e.EnvironmentOptions = new Microsoft.Web.WebView2.Core.CoreWebView2EnvironmentOptions();
        e.EnvironmentOptions.AdditionalBrowserArguments = additionalBrowserArguments;

        // Workaround: The e.EnvironmentOptions.AdditionalBrowserArguments set here seem to be
        // ignored, so we modify the environment directly.
        string oldArguments = Environment.GetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS");
        Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", oldArguments + " " + additionalBrowserArguments);
#endif
    }

    /// <summary>
    /// Tweak the WebView after it has been initialized.
    /// </summary>
    /// <remarks>
    /// e.WebView is the same as (blazorWebView.Handler.PlatformView as Microsoft.UI.Xaml.Controls.WebView2);
    /// </remarks>
    private void WebViewInitializedEventHandler(object sender, BlazorWebViewInitializedEventArgs e)
    {
        blazorWebView.BlazorWebViewInitialized -= WebViewInitializedEventHandler;
#if (WINDOWS)
        // Set the splash screen background color, shown from when the webview is displayed until
        // the the index.html is loaded.
        var backgroundColor = ColorExtensions.HexToColor("#4793d1");
        var uiBackgroundColor = Windows.UI.Color.FromArgb(backgroundColor.A, backgroundColor.R, backgroundColor.G, backgroundColor.B);
        e.WebView.DefaultBackgroundColor = uiBackgroundColor;
        e.WebView.CoreWebView2.Settings.IsPasswordAutosaveEnabled = false;
#endif
#if (WINDOWS && !DEBUG)
        // Workaround: Prevent browser keys like page refresh, or ctrl-shift-i to open the developer view
        e.WebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = false;
#endif
#if (ANDROID)
        // Settings cannot be set, application will stop running
        //var webSettings = e.WebView.Settings;
        //webSettings.JavaScriptEnabled = true;
        //webSettings.BlockNetworkLoads = true; // only local content allowed
        //webSettings.AllowFileAccess = false; // no local files but from the asset directory
        //webSettings.CacheMode = Android.Webkit.CacheModes.NoCache; // is already local content
        //webSettings.JavaScriptCanOpenWindowsAutomatically = false; // same as default
        //webSettings.SetSupportMultipleWindows(false); // same as default
        //webSettings.TextZoom = 100; // Ignores system font size, so the app controls the font size
        //webSettings.SaveFormData = false;
#endif
    }
}
