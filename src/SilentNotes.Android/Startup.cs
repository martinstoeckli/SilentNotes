// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Diagnostics.CodeAnalysis;
using Android.App;
using SilentNotes.Android.Services;
using SilentNotes.Controllers;
using SilentNotes.HtmlView;
using SilentNotes.Services;
using SilentNotes.Services.CloudStorageServices;
using SilentNotes.Views;
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
        public static void InitializeApplication(Activity rootActivity)
        {
            // do it only the first time
            if (IsFirstTime())
            {
                RegisterServices(rootActivity);
                RegisterControllers();
                RegisterRazorViews();
            }
        }

        private static void RegisterServices(Activity rootActivity)
        {
            Ioc.RegisterFactory<IEnvironmentService>(() => new EnvironmentService(SilentNotes.Services.OperatingSystem.Android));
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
                Ioc.GetOrCreate<IDataProtectionService>()));
            Ioc.RegisterFactory<IRepositoryStorageService>(() => new RepositoryStorageService(
                rootActivity,
                Ioc.GetOrCreate<IXmlFileService>(),
                Ioc.GetOrCreate<ILanguageService>()));
            Ioc.RegisterFactory<ICryptoRandomService>(() => new CryptoRandomService());
            Ioc.RegisterFactory<INoteRepositoryUpdater>(() => new NoteRepositoryUpdater());
            Ioc.RegisterFactory<IStoryBoardService>(() => new StoryBoardService());
            Ioc.RegisterFactory<ICloudStorageServiceFactory>(() => new CloudStorageServiceFactory(
                Ioc.GetOrCreate<INativeBrowserService>(),
                Ioc.GetOrCreate<ICryptoRandomService>()));
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
                Ioc.GetOrCreate<ISettingsService>()));
        }

        private static void RegisterControllers()
        {
            Ioc.RegisterFactoryWithKey<IController>(ControllerNames.NoteRepository, () => new NoteRepositoryController(
                Ioc.GetOrCreateWithKey<IRazorViewService>(ViewNames.NoteRepository),
                Ioc.GetOrCreateWithKey<IRazorViewService>(ViewNames.NoteRepositoryContent),
                Ioc.GetOrCreateWithKey<IRazorViewService>(ViewNames.Stop)));
            Ioc.RegisterFactoryWithKey<IController>(ControllerNames.Note, () => new NoteController(
                Ioc.GetOrCreate<IRepositoryStorageService>(),
                Ioc.GetOrCreateWithKey<IRazorViewService>(ViewNames.Note)));
            Ioc.RegisterFactoryWithKey<IController>(ControllerNames.Info, () => new InfoController(
                Ioc.GetOrCreateWithKey<IRazorViewService>(ViewNames.Info)));
            Ioc.RegisterFactoryWithKey<IController>(ControllerNames.RecycleBin, () => new RecycleBinController(
                Ioc.GetOrCreateWithKey<IRazorViewService>(ViewNames.RecycleBin),
                Ioc.GetOrCreateWithKey<IRazorViewService>(ViewNames.RecycleBinContent)));
            Ioc.RegisterFactoryWithKey<IController>(ControllerNames.Settings, () => new SettingsController(
                Ioc.CreateWithKey<IRazorViewService>(ViewNames.Settings)));
            Ioc.RegisterFactoryWithKey<IController>(ControllerNames.TransferCodeHistory, () => new TransferCodeHistoryController(
                Ioc.CreateWithKey<IRazorViewService>(ViewNames.TransferCodeHistory)));
            Ioc.RegisterFactoryWithKey<IController>(ControllerNames.FirstTimeSync, () => new FirstTimeSyncController(
                Ioc.CreateWithKey<IRazorViewService>(ViewNames.FirstTimeSync)));
            Ioc.RegisterFactoryWithKey<IController>(ControllerNames.CloudStorageChoice, () => new CloudStorageChoiceController(
                Ioc.CreateWithKey<IRazorViewService>(ViewNames.CloudStorageChoice)));
            Ioc.RegisterFactoryWithKey<IController>(ControllerNames.CloudStorageAccount, () => new CloudStorageAccountController(
                Ioc.CreateWithKey<IRazorViewService>(ViewNames.CloudStorageAccount)));
            Ioc.RegisterFactoryWithKey<IController>(ControllerNames.CloudStorageOauthWaiting, () => new CloudStorageOauthWaitingController(
                Ioc.CreateWithKey<IRazorViewService>(ViewNames.CloudStorageOauthWaiting)));
            Ioc.RegisterFactoryWithKey<IController>(ControllerNames.TransferCode, () => new TransferCodeController(
                Ioc.CreateWithKey<IRazorViewService>(ViewNames.TransferCode)));
            Ioc.RegisterFactoryWithKey<IController>(ControllerNames.MergeChoice, () => new MergeChoiceController(
                Ioc.CreateWithKey<IRazorViewService>(ViewNames.MergeChoice)));
        }

        private static void RegisterRazorViews()
        {
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.NoteRepository, () => new RazorViewService<NoteRepositoryRazorView>());
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.NoteRepositoryContent, () => new RazorViewService<NoteRepositoryContentRazorView>());
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.Note, () => new RazorViewService<NoteRazorView>());
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.Info, () => new RazorViewService<InfoRazorView>());
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.RecycleBin, () => new RazorViewService<RecycleBinRazorView>());
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.RecycleBinContent, () => new RazorViewService<RecycleBinContentRazorView>());
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.Settings, () => new RazorViewService<SettingsRazorView>());
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.TransferCodeHistory, () => new RazorViewService<TransferCodeHistoryRazorView>());
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.Stop, () => new RazorViewService<StopRazorView>());
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.FirstTimeSync, () => new RazorViewService<FirstTimeSyncRazorView>());
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.CloudStorageChoice, () => new RazorViewService<CloudStorageChoiceRazorView>());
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.CloudStorageAccount, () => new RazorViewService<CloudStorageAccountRazorView>());
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.CloudStorageOauthWaiting, () => new RazorViewService<CloudStorageOauthWaitingRazorView>());
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.TransferCode, () => new RazorViewService<TransferCodeRazorView>());
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.MergeChoice, () => new RazorViewService<MergeChoiceRazorView>());
        }

        private static bool IsFirstTime()
        {
            return !Ioc.IsRegistered<ILanguageService>();
        }
    }
}