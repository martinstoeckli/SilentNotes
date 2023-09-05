using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using CommunityToolkit.Mvvm.Messaging;
using SilentNotes.Platforms;
using SilentNotes.Platforms.Services;
using SilentNotes.Services;
using SilentNotes.Views;

namespace SilentNotes;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    Exported = true,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        System.Diagnostics.Debug.WriteLine("*** MainActivity.OnCreate() " + Id);

        // Inform services about the new main activity.
        Ioc.Instance.GetService<IAppContextService>().Initialize(this);
        base.OnCreate(savedInstanceState);
    }

    /// <inheritdoc/>
    protected override void OnDestroy()
    {
        System.Diagnostics.Debug.WriteLine("*** MainActivity.OnDestroy() " + Id);
        WeakReferenceMessenger.Default.Send<ClosePageMessage>(new ClosePageMessage());
        Ioc.Instance.GetService<IActivityResultAwaiter>().RedirectedOnDestroy();
        base.OnDestroy();
    }

    public Guid Id { get; } = Guid.NewGuid();

    /// <inheritdoc/>
    protected override void OnPause()
    {
        System.Diagnostics.Debug.WriteLine("*** MainActivity.OnPause() " + Id);
        base.OnPause();
        WeakReferenceMessenger.Default.Send<StoreUnsavedDataMessage>(new StoreUnsavedDataMessage());
    }

    /// <inheritdoc/>
    protected override void OnStop()
    {
        System.Diagnostics.Debug.WriteLine("*** MainActivity.OnStop() " + Id);

        // We do not await the synchronization, it runs in a background service which can stay
        // alive a bit longer than the app itself.
        IAutoSynchronizationService syncService = Ioc.Instance.GetService<IAutoSynchronizationService>();
        syncService.SynchronizeAtShutdown();
        base.OnStop();
    }

    /// <inheritdoc/>
    public override bool DispatchKeyEvent(KeyEvent e)
    {
        // Workaround: Unfortunately the back button will automatically navigate back whenever
        // possible, there is no way to intercept it in MainPage.OnBackButtonPressed().
        if ((e.KeyCode == Keycode.Back) && (e.Action == KeyEventActions.Down))
        {
            var message = new BackButtonPressedMessage { Handled = false };
            WeakReferenceMessenger.Default.Send(message);
            if (message.Handled)
                return true;
        }
        return base.DispatchKeyEvent(e);
    }

    /// <inheritdoc/>
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
    {
        Ioc.Instance.GetService<IActivityResultAwaiter>().RedirectedOnActivityResult(
            requestCode, resultCode, data);
        base.OnActivityResult(requestCode, resultCode, data);
    }
}
