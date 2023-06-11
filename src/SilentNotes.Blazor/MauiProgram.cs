// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using SilentNotes.Platforms.Services;
using SilentNotes.Services;

namespace SilentNotes;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.VisibleStateDuration = 6000;
        });

        RegisterSharedServices(builder.Services);
        RegisterPlatformServices(builder.Services);

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif
		return builder.Build();
	}

    private static void RegisterSharedServices(IServiceCollection services)
    {
        services.AddSingleton<ISvgIconService>((serviceProvider) => new SvgIconService());
        services.AddSingleton<ILanguageService>((serviceProvider) => new LanguageService(new LanguageServiceResourceReader(), "SilentNotes", new LanguageCodeService().GetSystemLanguageCode()));
        //services.AddSingleton<INoteRepositoryUpdater>((serviceProvider) => new NoteRepositoryUpdater());
        services.AddSingleton<IThemeService>((serviceProvider) => new ThemeService(
            serviceProvider.GetService<ISettingsService>(),
            serviceProvider.GetService<IEnvironmentService>()));
        //services.AddSingleton<INotificationService>((serviceProvider) => new NotificationService(
        //    serviceProvider.GetService<IFeedbackService>(),
        //    serviceProvider.GetService<ILanguageService>(),
        //    serviceProvider.GetService<ISettingsService>()));
        services.AddSingleton<IVersionService>((serviceProvider) => new VersionService());

        // Scoped services (some Blazor services like IJSRuntime seem to be scoped)
        services.AddScoped<INavigationService>((serviceProvider) => new NavigationService(
            serviceProvider.GetService<NavigationManager>(),
            serviceProvider.GetService<IJSRuntime>()));
        services.AddScoped<IFeedbackService>((serviceProvider) => new FeedbackService(
            serviceProvider.GetService<IDialogService>(),
            serviceProvider.GetService<ILanguageService>()));
    }

#if WINDOWS

    private static void RegisterPlatformServices(IServiceCollection services)
    {
        services.AddSingleton<ICryptoRandomService>((serviceProvider) => new CryptoRandomService());
        services.AddSingleton<IDataProtectionService>((serviceProvider) => new DataProtectionService());
        services.AddSingleton<IXmlFileService>((serviceProvider) => new XmlFileService());
        services.AddSingleton<IEnvironmentService>((serviceProvider) => new EnvironmentService());
        services.AddSingleton<IRepositoryStorageService>((serviceProvider) => new RepositoryStorageService(
            serviceProvider.GetService<IXmlFileService>(),
            serviceProvider.GetService<ILanguageService>()));
        services.AddSingleton<ISettingsService>((serviceProvider) => new SettingsService(
            serviceProvider.GetService<IXmlFileService>(),
            serviceProvider.GetService<IDataProtectionService>()));
        services.AddSingleton<INativeBrowserService>((serviceProvider) => new NativeBrowserService());
    }

#elif ANDROID

    private static void RegisterPlatformServices(IServiceCollection services)
    {
        services.AddSingleton<IAppContextService>((serviceProvider) => new AppContextService());
        services.AddSingleton<ICryptoRandomService>((serviceProvider) => new CryptoRandomService());
        services.AddSingleton<IDataProtectionService>((serviceProvider) => new DataProtectionService(
            serviceProvider.GetService<ICryptoRandomService>()));
        services.AddSingleton<IXmlFileService>((serviceProvider) => new XmlFileService());
        services.AddSingleton<IEnvironmentService>((serviceProvider) => new EnvironmentService(
            serviceProvider.GetService<IAppContextService>()));
        services.AddSingleton<IRepositoryStorageService>((serviceProvider) => new RepositoryStorageService(
            serviceProvider.GetService<IAppContextService>(),
            serviceProvider.GetService<IXmlFileService>(),
            serviceProvider.GetService<ILanguageService>()));
        services.AddSingleton<ISettingsService>((serviceProvider) => new SettingsService(
            serviceProvider.GetService<IAppContextService>(),
            serviceProvider.GetService<IXmlFileService>(),
            serviceProvider.GetService<IDataProtectionService>()));
        services.AddSingleton<INativeBrowserService>((serviceProvider) => new NativeBrowserService(
            serviceProvider.GetService<IAppContextService>()));
    }

#endif
}
