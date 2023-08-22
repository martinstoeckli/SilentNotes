using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using CommunityToolkit.Mvvm.DependencyInjection;
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
        // Inform services about the new main activity.
        App.Ioc.GetService<IAppContextService>().Initialize(this);
        base.OnCreate(savedInstanceState);
    }

    /// <inheritdoc/>
    protected override void OnDestroy()
    {
        App.Ioc.GetService<IActivityResultAwaiter>().RedirectedOnDestroy();
        base.OnDestroy();
    }

    protected override void OnPause()
    {
        base.OnPause();
        WeakReferenceMessenger.Default.Send<StoreUnsavedDataMessage>(new StoreUnsavedDataMessage());
    }

    protected override void OnStop()
    {
        // We do not await the synchronization, it runs in a background service which can stay
        // alive a bit longer than the app itself.
        // todo:
        IAutoSynchronizationService syncService = App.Ioc.GetService<IAutoSynchronizationService>();
        syncService.SynchronizeAtShutdown();
        base.OnStop();
    }

    /// <inheritdoc/>
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
    {
        App.Ioc.GetService<IActivityResultAwaiter>().RedirectedOnActivityResult(
            requestCode, resultCode, data);
        base.OnActivityResult(requestCode, resultCode, data);
    }
}
