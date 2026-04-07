using System;
using Avalonia;
using AvaloniaCrossTest.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Services;
using SilentNotesAvalonia.Desktop.Services;

namespace SilentNotesAvalonia.Desktop;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .AfterSetup(builder =>
            {
                var app = (App)builder.Instance;
                app.InitializeServices(CreateServiceProvider());
            });

    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IMainWindowProvider, MainWindowProvider>();
        services.AddSingleton<IFeedbackService>((services) =>
            new FeedbackService(services.GetRequiredService<IMainWindowProvider>()));

        // todo: stom use keystore on linux?
        services.AddDataProtection(); // todo: .ProtectKeysWithCertificate("thumbprint")
        services.AddSingleton<IDataProtectionService>((services) =>
            new DataProtectionService(services.GetRequiredService<IDataProtectionProvider>()));
        return services.BuildServiceProvider();
    }
}
