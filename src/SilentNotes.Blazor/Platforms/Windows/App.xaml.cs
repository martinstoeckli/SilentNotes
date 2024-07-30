using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SilentNotes.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            // Allow only a single application instance
            if (TryGetAlreadyRunningInstance(out AppInstance otherInstance))
            {
                var activatedEventArgs = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent()?.GetActivatedEventArgs();
                otherInstance.RedirectActivationToAsync(activatedEventArgs).AsTask().Wait();
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }

            // Workaround: On Windows we have to convince the WebView to use the user language (e.g. for auto correction).
            string userLanguage = Windows.System.UserProfile.GlobalizationPreferences.Languages.FirstOrDefault();
            AddAdditionalBrowserArguments("--lang", userLanguage);
            AddAdditionalBrowserArguments("--disable-features", "msSmartScreenProtection");

            this.InitializeComponent();
        }

        /// <summary>
        /// Adds a new variable to the WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS in the Environment.
        /// </summary>
        /// <param name="name">Name of the variable.</param>
        /// <param name="value">Value of the variable.</param>
        private static void AddAdditionalBrowserArguments(string name, string value)
        {
            var arguments = new string[]
            {
                Environment.GetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS"), // old arguments
                name + "=" + value, // new argument
            };
            string combinedArguments = string.Join(" ", arguments.Where(part => !string.IsNullOrEmpty(part)));
            Environment.SetEnvironmentVariable("WEBVIEW2_ADDITIONAL_BROWSER_ARGUMENTS", combinedArguments);
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        /// <summary>
        /// Checks whether another instance of this application is already open and returns it.
        /// </summary>
        /// <param name="otherInstance">The already open instance or null if no other instance is running.</param>
        /// <returns>Returns true if another instance was found, otherwise false.</returns>
        private bool TryGetAlreadyRunningInstance(out AppInstance otherInstance)
        {
            AppInstance thisInstance = Microsoft.Windows.AppLifecycle.AppInstance.GetCurrent();
            otherInstance = Microsoft.Windows.AppLifecycle.AppInstance.GetInstances().FirstOrDefault(
                instance => instance != thisInstance);
            return otherInstance != null;
        }
    }
}
