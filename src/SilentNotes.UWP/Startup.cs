// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.HtmlView;
using SilentNotes.Services;
using SilentNotes.UWP.Services;
using SilentNotes.Workers;

namespace SilentNotes.UWP
{
    /// <summary>
    /// Initializes the dependencies and services of the application.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Sets up the application and initializes the services.
        /// </summary>
        public static void InitializeApplication()
        {
            ServiceCollection services = new ServiceCollection();

            RegisterServices(services);
            StartupShared.RegisterControllers(services);
            StartupShared.RegisterRazorViews(services);
            StartupShared.RegisterCloudStorageClientFactory(services);

            Ioc.Default.ConfigureServices(services.BuildServiceProvider());
        }

        private static void RegisterServices(ServiceCollection services)
        {
            services.AddSingleton<IEnvironmentService>((serviceProvider) => new EnvironmentService(OperatingSystem.Windows));
            services.AddSingleton<IBaseUrlService>((serviceProvider) => new BaseUrlService());
            services.AddSingleton<ILanguageService>((serviceProvider) => new LanguageService(new LanguageServiceResourceReader(), "SilentNotes", new LanguageCodeService().GetSystemLanguageCode()));
            services.AddSingleton<ISvgIconService>((serviceProvider) => new SvgIconService());
            services.AddSingleton<INavigationService>((serviceProvider) => new NavigationService(
                serviceProvider.GetService<IHtmlView>()));
            services.AddSingleton<INativeBrowserService>((serviceProvider) => new NativeBrowserService());
            services.AddSingleton<IXmlFileService>((serviceProvider) => new XmlFileService());
            services.AddSingleton<IVersionService>((serviceProvider) => new VersionService());
            services.AddSingleton<ISettingsService>((serviceProvider) => new SettingsService(
                serviceProvider.GetService<IXmlFileService>(),
                serviceProvider.GetService<IDataProtectionService>()));
            services.AddSingleton<IRepositoryStorageService>((serviceProvider) => new RepositoryStorageService(
                serviceProvider.GetService<IXmlFileService>(),
                serviceProvider.GetService<ILanguageService>()));
            services.AddSingleton<ICryptoRandomService>((serviceProvider) => new CryptoRandomService());
            services.AddSingleton<INoteRepositoryUpdater>((serviceProvider) => new NoteRepositoryUpdater());
            services.AddSingleton<IStoryBoardService>((serviceProvider) => new StoryBoardService());
            services.AddSingleton<IDataProtectionService>((serviceProvider) => new DataProtectionService());
            services.AddSingleton<IInternetStateService>((serviceProvider) => new InternetStateService());
            services.AddSingleton<IAutoSynchronizationService>((serviceProvider) => new AutoSynchronizationService(
                serviceProvider.GetService<IInternetStateService>(),
                serviceProvider.GetService<ISettingsService>(),
                serviceProvider.GetService<IRepositoryStorageService>(),
                serviceProvider.GetService<INavigationService>()));
            services.AddSingleton<IThemeService>((serviceProvider) => new ThemeService(
                serviceProvider.GetService<ISettingsService>(),
                serviceProvider.GetService<IEnvironmentService>()));
            services.AddSingleton<IFolderPickerService>((serviceProvider) => new FolderPickerService());
            services.AddSingleton<IFilePickerService>((serviceProvider) => new FilePickerService());

            // The main page is not yet known but can be set to the service as soon as the page exists.
            services.AddSingleton<MainPageService>((serviceProvider) => new MainPageService());
            services.AddSingleton<IHtmlView>((serviceProvider) =>
                serviceProvider.GetService<MainPageService>().MainPage);
            services.AddSingleton<IFeedbackService>((serviceProvider) => new FeedbackService(
                serviceProvider.GetService<MainPageService>().MainPage,
                serviceProvider.GetService<ILanguageService>()));
        }
    }
}
