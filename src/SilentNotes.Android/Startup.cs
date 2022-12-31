// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using Android.App;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Android.Services;
using SilentNotes.HtmlView;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotes.Android
{
    /// <summary>
    /// Initializes the dependencies and services of the application.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Sets up the application and initializes the services.
        /// </summary>
        /// <param name="rootActivity">The main activity of the Android app.</param>
        public static void InitializeApplication(Activity rootActivity, ActivityResultAwaiter rootActivityResultWaiter)
        {
            ServiceCollection services = new ServiceCollection();

            RegisterServices(services, rootActivity, rootActivityResultWaiter);
            StartupShared.RegisterControllers(services);
            StartupShared.RegisterRazorViews(services);
            StartupShared.RegisterCloudStorageClientFactory(services);

            Ioc.Default.ConfigureServices(services.BuildServiceProvider());
        }

        private static void RegisterServices(ServiceCollection services, Activity rootActivity, ActivityResultAwaiter rootActivityResultWaiter)
        {
            services.AddSingleton<IEnvironmentService>((serviceProvider) => new EnvironmentService(OperatingSystem.Android, rootActivity));
            services.AddSingleton<IHtmlView>((serviceProvider) => rootActivity as IHtmlView);
            services.AddSingleton<IBaseUrlService>((serviceProvider) => new BaseUrlService());
            services.AddSingleton<ILanguageService>((serviceProvider) => new LanguageService(new LanguageServiceResourceReader(rootActivity), "SilentNotes", new LanguageCodeService().GetSystemLanguageCode()));
            services.AddSingleton<ISvgIconService>((serviceProvider) => new SvgIconService());
            services.AddSingleton<INavigationService>((serviceProvider) => new NavigationService(
                serviceProvider.GetService<IHtmlView>()));
            services.AddSingleton<INativeBrowserService>((serviceProvider) => new NativeBrowserService(rootActivity));
            services.AddSingleton<IXmlFileService>((serviceProvider) => new XmlFileService());
            services.AddSingleton<IVersionService>((serviceProvider) => new VersionService(rootActivity));
            services.AddSingleton<ISettingsService>((serviceProvider) => new SettingsService(
                rootActivity,
                serviceProvider.GetService<IXmlFileService>(),
                serviceProvider.GetService<IDataProtectionService>()));
            services.AddSingleton<IRepositoryStorageService>((serviceProvider) => new RepositoryStorageService(
                rootActivity,
                serviceProvider.GetService<IXmlFileService>(),
                serviceProvider.GetService<ILanguageService>()));
            services.AddSingleton<ICryptoRandomService>((serviceProvider) => new CryptoRandomService());
            services.AddSingleton<INoteRepositoryUpdater>((serviceProvider) => new NoteRepositoryUpdater());
            services.AddSingleton<IStoryBoardService>((serviceProvider) => new StoryBoardService());
            services.AddSingleton<IFeedbackService>((serviceProvider) => new FeedbackService(
                rootActivity,
                serviceProvider.GetService<ILanguageService>()));
            services.AddSingleton<IDataProtectionService>((serviceProvider) => new DataProtectionService(
                rootActivity,
                serviceProvider.GetService<ICryptoRandomService>()));
            services.AddSingleton<IInternetStateService>((serviceProvider) => new InternetStateService(rootActivity));
            services.AddSingleton<IAutoSynchronizationService>((serviceProvider) => new AutoSynchronizationService(
                serviceProvider.GetService<IInternetStateService>(),
                serviceProvider.GetService<ISettingsService>(),
                serviceProvider.GetService<IRepositoryStorageService>(),
                serviceProvider.GetService<INavigationService>()));
            services.AddSingleton<IThemeService>((serviceProvider) => new ThemeService(
                serviceProvider.GetService<ISettingsService>(),
                serviceProvider.GetService<IEnvironmentService>()));
            services.AddSingleton<IFolderPickerService>((serviceProvider) => new FolderPickerService(rootActivity, rootActivityResultWaiter));
            services.AddSingleton<IFilePickerService>((serviceProvider) => new FilePickerService(rootActivity, rootActivityResultWaiter));
        }
    }
}