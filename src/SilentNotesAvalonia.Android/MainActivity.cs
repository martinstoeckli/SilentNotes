using System;
using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;
using AvaloniaCrossTest.Services;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Services;
using SilentNotesAvalonia.Android.Services;

namespace SilentNotesAvalonia.Android;

[Activity(
    Label = "SilentNotesAvalonia.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CreateAppBuilder()
    {
        return AppBuilder.Configure<App>()
            .UseAndroid()
            .AfterSetup(builder =>
            {
                var app = (App)builder.Instance;
                app.InitializeServices(CreateServiceProvider());
            });
    }

    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }

    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IFeedbackService>((services) => new FeedbackService());
        return services.BuildServiceProvider();
    }
}
