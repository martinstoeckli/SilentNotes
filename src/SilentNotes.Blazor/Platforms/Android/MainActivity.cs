using Android.App;
using Android.Content.PM;
using Android.OS;
using SilentNotes.Platforms.Services;

namespace SilentNotes;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        // Inform services about the new main activity.
        App.Ioc.GetService<IAppContextService>().Initialize(this);

        base.OnCreate(savedInstanceState);
    }
}
