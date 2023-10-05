using Android.App;
using Android.Content.PM;
using Android.Views;
using CommunityToolkit.Mvvm.Messaging;
using SilentNotes.Services;

namespace SilentNotes;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    Exported = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
public class MainActivity : MauiAppCompatActivity
{
    public Guid Id { get; } = Guid.NewGuid();

    /// <inheritdoc/>
    public override bool DispatchKeyEvent(KeyEvent e)
    {
        // Workaround: Unfortunately the back button will automatically navigate back whenever
        // possible, there is no way to intercept it in MainPage.OnBackButtonPressed().
        if ((e.KeyCode == Keycode.Back) && (e.Action == KeyEventActions.Down))
        {
            // Ask the page to close currently open menus and dialogs.
            var message = new BackButtonPressedMessage { Handled = false };
            WeakReferenceMessenger.Default.Send(message);
            if (message.Handled)
                return true;

            // Check whether a backward navigation should take place (the WebView browser history is
            // deactivated, so it would always close the app).
            var navigation = Ioc.Instance.GetService<INavigationService>();
            if (navigation.CanNavigateBack)
            {
                navigation.NavigateBack();
                return true;
            }
        }
        return base.DispatchKeyEvent(e);
    }
}
