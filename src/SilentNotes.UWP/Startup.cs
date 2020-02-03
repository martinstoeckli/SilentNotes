// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
using SilentNotes.HtmlView;
using SilentNotes.Services;
using SilentNotes.UWP.Services;
using SilentNotes.Workers;

namespace SilentNotes.UWP
{
    /// <summary>
    /// Initializes the dependencies and services of the application.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1115:ParameterMustFollowComma", Justification = "Keep readability.")]
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1116:SplitParametersMustStartOnLineAfterDeclaration", Justification = "Keep readability.")]
    public class Startup
    {
        private static bool _isFirstTime = true;

        /// <summary>
        /// Sets up the application and initializes the services.
        /// </summary>
        public static void InitializeApplication()
        {
            RegisterServices();
            StartupShared.RegisterControllers();
            StartupShared.RegisterRazorViews();
            StartupShared.RegisterCloudStorageClientFactory();
        }

        /// <summary>
        /// Sets up the application and initializes the services.
        /// </summary>
        /// <param name="mainPage">Main page of the application.</param>
        public static void InitializeApplicationWithMainPage(MainPage mainPage)
        {
            // do it only the first time
            if (_isFirstTime)
            {
                _isFirstTime = false;
                RegisterServicesWithMainPage(mainPage);
            }
        }

        private static void RegisterServices()
        {
            Ioc.RegisterFactory<IEnvironmentService>(() => new EnvironmentService(OperatingSystem.Windows));
            Ioc.RegisterFactory<IBaseUrlService>(() => new BaseUrlService());
            Ioc.RegisterFactory<ILanguageService>(() => new LanguageService(new LanguageCodeService().GetSystemLanguageCode()));
            Ioc.RegisterFactory<ISvgIconService>(() => new SvgIconService());
            Ioc.RegisterFactory<INavigationService>(() => new NavigationService(
                Ioc.GetOrCreate<IHtmlView>()));
            Ioc.RegisterFactory<INativeBrowserService>(() => new NativeBrowserService());
            Ioc.RegisterFactory<IXmlFileService>(() => new XmlFileService());
            Ioc.RegisterFactory<IVersionService>(() => new VersionService());
            Ioc.RegisterFactory<ISettingsService>(() => new SettingsService(
                Ioc.GetOrCreate<IXmlFileService>(),
                Ioc.GetOrCreate<IDataProtectionService>(),
                Ioc.GetOrCreate<IEnvironmentService>()));
            Ioc.RegisterFactory<IRepositoryStorageService>(() => new RepositoryStorageService(
                Ioc.GetOrCreate<IXmlFileService>(),
                Ioc.GetOrCreate<ILanguageService>()));
            Ioc.RegisterFactory<ICryptoRandomService>(() => new CryptoRandomService());
            Ioc.RegisterFactory<INoteRepositoryUpdater>(() => new NoteRepositoryUpdater());
            Ioc.RegisterFactory<IStoryBoardService>(() => new StoryBoardService());
            Ioc.RegisterFactory<IDataProtectionService>(() => new DataProtectionService());
            Ioc.RegisterFactory<IInternetStateService>(() => new InternetStateService());
            Ioc.RegisterFactory<IAutoSynchronizationService>(() => new AutoSynchronizationService(
                Ioc.GetOrCreate<IInternetStateService>(),
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<IRepositoryStorageService>(),
                Ioc.GetOrCreate<INavigationService>()));
            Ioc.RegisterFactory<IThemeService>(() => new ThemeService(
                Ioc.GetOrCreate<ISettingsService>(), Ioc.GetOrCreate<IEnvironmentService>()));
        }

        private static void RegisterServicesWithMainPage(MainPage mainPage)
        {
            Ioc.RegisterFactory<IHtmlView>(() => mainPage);
            Ioc.RegisterFactory<IFeedbackService>(() => new FeedbackService(
                mainPage, Ioc.GetOrCreate<ILanguageService>()));
        }
    }
}
