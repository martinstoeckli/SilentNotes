// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
using Android.App;
using SilentNotes.Android.Services;
using SilentNotes.HtmlView;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotes.Android
{
    /// <summary>
    /// Initializes the dependencies and services of the application.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1115:ParameterMustFollowComma", Justification = "Keep readability.")]
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1116:SplitParametersMustStartOnLineAfterDeclaration", Justification = "Keep readability.")]
    public class Startup
    {
        /// <summary>
        /// Sets up the application and initializes the services.
        /// </summary>
        /// <param name="rootActivity">The main activity of the Android app.</param>
        public static void InitializeApplication(Activity rootActivity, ActivityResultAwaiter rootActivityResultWaiter)
        {
            // do it only the first time
            if (IsFirstTime())
            {
                RegisterServices(rootActivity, rootActivityResultWaiter);
                StartupShared.RegisterControllers();
                StartupShared.RegisterRazorViews();
                StartupShared.RegisterCloudStorageClientFactory();
            }
        }

        private static void RegisterServices(Activity rootActivity, ActivityResultAwaiter rootActivityResultWaiter)
        {
            Ioc.RegisterFactory<IEnvironmentService>(() => new EnvironmentService(OperatingSystem.Android, rootActivity));
            Ioc.RegisterFactory<IHtmlView>(() => rootActivity as IHtmlView);
            Ioc.RegisterFactory<IBaseUrlService>(() => new BaseUrlService());
            Ioc.RegisterFactory<ILanguageService>(() => new LanguageService(new LanguageCodeService().GetSystemLanguageCode()));
            Ioc.RegisterFactory<ISvgIconService>(() => new SvgIconService());
            Ioc.RegisterFactory<INavigationService>(() => new NavigationService(
                Ioc.GetOrCreate<IHtmlView>()));
            Ioc.RegisterFactory<INativeBrowserService>(() => new NativeBrowserService(rootActivity));
            Ioc.RegisterFactory<IXmlFileService>(() => new XmlFileService());
            Ioc.RegisterFactory<IVersionService>(() => new VersionService(rootActivity));
            Ioc.RegisterFactory<ISettingsService>(() => new SettingsService(
                rootActivity,
                Ioc.GetOrCreate<IXmlFileService>(),
                Ioc.GetOrCreate<IDataProtectionService>(),
                Ioc.GetOrCreate<IEnvironmentService>()));
            Ioc.RegisterFactory<IRepositoryStorageService>(() => new RepositoryStorageService(
                rootActivity,
                Ioc.GetOrCreate<IXmlFileService>(),
                Ioc.GetOrCreate<ILanguageService>()));
            Ioc.RegisterFactory<ICryptoRandomService>(() => new CryptoRandomService());
            Ioc.RegisterFactory<INoteRepositoryUpdater>(() => new NoteRepositoryUpdater());
            Ioc.RegisterFactory<IStoryBoardService>(() => new StoryBoardService());
            Ioc.RegisterFactory<IFeedbackService>(() => new FeedbackService(
                rootActivity, Ioc.GetOrCreate<ILanguageService>()));
            Ioc.RegisterFactory<IDataProtectionService>(() => new DataProtectionService(
                rootActivity,
                Ioc.GetOrCreate<ICryptoRandomService>()));
            Ioc.RegisterFactory<IInternetStateService>(() => new InternetStateService(rootActivity));
            Ioc.RegisterFactory<IAutoSynchronizationService>(() => new AutoSynchronizationService(
                Ioc.GetOrCreate<IInternetStateService>(),
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<IRepositoryStorageService>(),
                Ioc.GetOrCreate<INavigationService>()));
            Ioc.RegisterFactory<IThemeService>(() => new ThemeService(
                Ioc.GetOrCreate<ISettingsService>(), Ioc.GetOrCreate<IEnvironmentService>()));
            Ioc.RegisterFactory<IFolderPickerService>(() => new FolderPickerService(rootActivity, rootActivityResultWaiter));
        }

        private static bool IsFirstTime()
        {
            return !Ioc.IsRegistered<IEnvironmentService>();
        }
    }
}