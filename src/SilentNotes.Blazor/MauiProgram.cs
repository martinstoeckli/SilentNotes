// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;
using MudBlazor.Services;
using SilentNotes.Platforms;
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
            // todo: config.SnackbarConfiguration.HideIcon = true;
            config.SnackbarConfiguration.HideTransitionDuration = 500;
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
            config.SnackbarConfiguration.ShowCloseIcon = false;
            config.SnackbarConfiguration.ShowTransitionDuration = 500;
            config.SnackbarConfiguration.SnackbarVariant = Variant.Text;
            config.SnackbarConfiguration.VisibleStateDuration = 6000;
        });

        RegisterSharedServices(builder.Services);
        RegisterPlatformServices(builder.Services);

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug().SetMinimumLevel(LogLevel.Information);
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
        services.AddSingleton<ICloudStorageClientFactory>((serviceProvider) => new CloudStorageClientFactory());
        services.AddSingleton<IClipboardService>((serviceProvider) => new ClipboardService());

        // Scoped services (some Blazor services like IJSRuntime seem to be scoped)
        services.AddScoped<INavigationService>((serviceProvider) => new NavigationService(
            serviceProvider.GetService<NavigationManager>(),
            serviceProvider.GetService<IJSRuntime>()));
        services.AddScoped<IFeedbackService>((serviceProvider) => new FeedbackService(
            serviceProvider.GetService<IDialogService>(),
            serviceProvider.GetService<ISnackbar>(),
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
        services.AddSingleton<IFolderPickerService>((serviceProvider) => new FolderPickerService());
        services.AddSingleton<IFilePickerService>((serviceProvider) => new FilePickerService());
        services.AddSingleton<IAutoSynchronizationService>((serviceProvider) => new AutoSynchronizationService());
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
        services.AddSingleton<IActivityResultAwaiter>((ServiceProvider) => new ActivityResultAwaiter());
        services.AddSingleton<IFolderPickerService>((serviceProvider) => new FolderPickerService(
            serviceProvider.GetService<IAppContextService>(),
            serviceProvider.GetService<IActivityResultAwaiter>()));
        services.AddSingleton<IFilePickerService>((serviceProvider) => new FilePickerService(
            serviceProvider.GetService<IAppContextService>(),
            serviceProvider.GetService<IActivityResultAwaiter>()));
        services.AddSingleton<IAutoSynchronizationService>((serviceProvider) => new AutoSynchronizationService());
    }

#endif
}
