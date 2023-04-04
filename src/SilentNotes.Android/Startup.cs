// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Android.Services;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotes.Android
{
    /// <summary>
    /// Initializes the dependencies and services of the application.
    /// </summary>
    public class Startup
    {
        private static bool _isInitialized = false;

        /// <summary>
        /// Sets up the application and initializes the services. The Ioc can be initialized only
        /// once in the application life time, thus consecutive calls to this function will be ignored.
        /// Services must get the app context dynamically from the <see cref="IAppContextService"/>
        /// and should not store a reference to it.
        /// </summary>
        public static void InitializeApplication()
        {
            if (_isInitialized)
                return;
            _isInitialized = true;

            ServiceCollection services = new ServiceCollection();

            RegisterServices(services);
            StartupShared.RegisterControllers(services);
            StartupShared.RegisterRazorViews(services);
            StartupShared.RegisterCloudStorageClientFactory(services);

            Ioc.Default.ConfigureServices(services.BuildServiceProvider());
        }

        internal static void RegisterServices(ServiceCollection services)
        {
            services.AddSingleton<IAppContextService>((serviceProvider) => new AppContextService());
            services.AddTransient<IHtmlViewService>((serviceProvider) =>
                serviceProvider.GetService<IAppContextService>() as IHtmlViewService);
            services.AddSingleton<IEnvironmentService>((serviceProvider) => new EnvironmentService(
                OperatingSystem.Android, serviceProvider.GetService<IAppContextService>()));
            services.AddSingleton<IBaseUrlService>((serviceProvider) => new BaseUrlService());
            services.AddSingleton<ILanguageService>((serviceProvider) => new LanguageService(
                new LanguageServiceResourceReader(serviceProvider.GetService<IAppContextService>()),
                "SilentNotes",
                new LanguageCodeService().GetSystemLanguageCode()));
            services.AddSingleton<ISvgIconService>((serviceProvider) => new SvgIconService());
            services.AddSingleton<INavigationService>((serviceProvider) => new NavigationService(
                serviceProvider.GetService<IHtmlViewService>()));
            services.AddSingleton<INativeBrowserService>((serviceProvider) => new NativeBrowserService(serviceProvider.GetService<IAppContextService>()));
            services.AddSingleton<IXmlFileService>((serviceProvider) => new XmlFileService());
            services.AddSingleton<IVersionService>((serviceProvider) => new VersionService(
                serviceProvider.GetService<IAppContextService>()));
            services.AddSingleton<ISettingsService>((serviceProvider) => new SettingsService(
                serviceProvider.GetService<IAppContextService>(),
                serviceProvider.GetService<IXmlFileService>(),
                serviceProvider.GetService<IDataProtectionService>()));
            services.AddSingleton<IRepositoryStorageService>((serviceProvider) => new RepositoryStorageService(
                serviceProvider.GetService<IAppContextService>(),
                serviceProvider.GetService<IXmlFileService>(),
                serviceProvider.GetService<ILanguageService>()));
            services.AddSingleton<ICryptoRandomService>((serviceProvider) => new CryptoRandomService());
            services.AddSingleton<INoteRepositoryUpdater>((serviceProvider) => new NoteRepositoryUpdater());
            services.AddSingleton<IStoryBoardService>((serviceProvider) => new StoryBoardService());
            services.AddSingleton<IFeedbackService>((serviceProvider) => new FeedbackService(
                serviceProvider.GetService<IAppContextService>(),
                serviceProvider.GetService<ILanguageService>()));
            services.AddSingleton<IDataProtectionService>((serviceProvider) => new DataProtectionService(
                serviceProvider.GetService<ICryptoRandomService>()));
            services.AddSingleton<IAutoSynchronizationService>((serviceProvider) => new AutoSynchronizationService());
            services.AddSingleton<IThemeService>((serviceProvider) => new ThemeService(
                serviceProvider.GetService<ISettingsService>(),
                serviceProvider.GetService<IEnvironmentService>()));
            services.AddSingleton<IActivityResultAwaiter>((ServiceProvider) => new ActivityResultAwaiter());
            services.AddSingleton<IFolderPickerService>((serviceProvider) => new FolderPickerService(
                serviceProvider.GetService<IAppContextService>(),
                serviceProvider.GetService<IActivityResultAwaiter>()));
            services.AddSingleton<IFilePickerService>((serviceProvider) => new FilePickerService(
                serviceProvider.GetService<IAppContextService>(),
                serviceProvider.GetService<IActivityResultAwaiter>()));
        }
    }
}