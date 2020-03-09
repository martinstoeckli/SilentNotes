// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.Controllers;
using SilentNotes.Services;
using SilentNotes.Views;

namespace SilentNotes
{
    /// <summary>
    /// Initializes the shared dependencies and services of the application, which are not platform
    /// specific.
    /// </summary>
    public class StartupShared
    {
        /// <summary>
        /// Registers all available controllers.
        /// </summary>
        public static void RegisterControllers()
        {
            Ioc.RegisterFactoryWithKey<IController>(ControllerNames.NoteRepository, () => new NoteRepositoryController(
                Ioc.GetOrCreateWithKey<IRazorViewService>(ViewNames.NoteRepository),
                Ioc.GetOrCreateWithKey<IRazorViewService>(ViewNames.NoteRepositoryContent),
                Ioc.GetOrCreateWithKey<IRazorViewService>(ViewNames.Stop)));
            Ioc.RegisterFactoryWithKey<IController>(ControllerNames.Note, () => new NoteController(
                Ioc.GetOrCreate<IRepositoryStorageService>(),
                Ioc.GetOrCreateWithKey<IRazorViewService>(ViewNames.Note)));
            Ioc.RegisterFactoryWithKey<IController>(ControllerNames.Checklist, () => new NoteController(
                Ioc.GetOrCreate<IRepositoryStorageService>(),
                Ioc.GetOrCreateWithKey<IRazorViewService>(ViewNames.Checklist)));
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
            Ioc.RegisterFactoryWithKey<IController>(ControllerNames.OpenSafe, () => new OpenSafeController(
                Ioc.CreateWithKey<IRazorViewService>(ViewNames.OpenSafe)));
            Ioc.RegisterFactoryWithKey<IController>(ControllerNames.ChangePassword, () => new ChangePasswordController(
                Ioc.CreateWithKey<IRazorViewService>(ViewNames.ChangePassword)));
        }

        /// <summary>
        /// Registers all available views.
        /// </summary>
        public static void RegisterRazorViews()
        {
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.NoteRepository, () => new RazorViewService<NoteRepositoryRazorView>());
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.NoteRepositoryContent, () => new RazorViewService<NoteRepositoryContentRazorView>());
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.Note, () => new RazorViewService<NoteRazorView>());
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.Checklist, () => new RazorViewService<ChecklistRazorView>());
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
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.OpenSafe, () => new RazorViewService<OpenSafeRazorView>());
            Ioc.RegisterFactoryWithKey<IRazorViewService>(ViewNames.ChangePassword, () => new RazorViewService<ChangePasswordRazorView>());
        }

        /// <summary>
        /// Registers all available cloud storage clients.
        /// </summary>
        public static void RegisterCloudStorageClientFactory()
        {
            Ioc.RegisterFactory<ICloudStorageClientFactory>(() => new CloudStorageClientFactory());
        }
    }
}
