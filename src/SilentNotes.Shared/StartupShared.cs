// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using SilentNotes.Controllers;
using SilentNotes.Services;
using SilentNotes.Views;

namespace SilentNotes
{
    /// <summary>A service factory for <see cref="IController"/>.</summary>
    internal class ControllerFactory : ServiceFactory<string, IController>
    {
        public ControllerFactory()
            : base(false)
        {
        }
    }

    /// <summary>A service factory for <see cref="IRazorViewService"/>.</summary>
    internal class RazorViewFactory : ServiceFactory<string, IRazorViewService>
    {
        public RazorViewFactory()
            : base(false)
        {
        }
    }

    /// <summary>
    /// Initializes the shared dependencies and services of the application, which are not platform
    /// specific.
    /// </summary>
    public class StartupShared
    {
        /// <summary>
        /// Registers all available controllers.
        /// </summary>
        public static void RegisterControllers(ServiceCollection services)
        {
            var serviceFactory = new ControllerFactory();
            services.AddSingleton<ControllerFactory>(serviceFactory);

            serviceFactory.Add(ControllerNames.NoteRepository, () => new NoteRepositoryController(
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.NoteRepository),
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.NoteRepositoryContent),
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.Stop)));
            serviceFactory.Add(ControllerNames.Note, () => new NoteController(
                Ioc.Default.GetService<IRepositoryStorageService>(),
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.Note)));
            serviceFactory.Add(ControllerNames.Checklist, () => new NoteController(
                Ioc.Default.GetService<IRepositoryStorageService>(),
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.Checklist)));
            serviceFactory.Add(ControllerNames.Info, () => new InfoController(
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.Info)));
            serviceFactory.Add(ControllerNames.RecycleBin, () => new RecycleBinController(
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.RecycleBin),
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.RecycleBinContent)));
            serviceFactory.Add(ControllerNames.Settings, () => new SettingsController(
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.Settings)));
            serviceFactory.Add(ControllerNames.TransferCodeHistory, () => new TransferCodeHistoryController(
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.TransferCodeHistory)));
            serviceFactory.Add(ControllerNames.FirstTimeSync, () => new FirstTimeSyncController(
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.FirstTimeSync)));
            serviceFactory.Add(ControllerNames.CloudStorageChoice, () => new CloudStorageChoiceController(
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.CloudStorageChoice)));
            serviceFactory.Add(ControllerNames.CloudStorageAccount, () => new CloudStorageAccountController(
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.CloudStorageAccount)));
            serviceFactory.Add(ControllerNames.CloudStorageOauthWaiting, () => new CloudStorageOauthWaitingController(
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.CloudStorageOauthWaiting)));
            serviceFactory.Add(ControllerNames.TransferCode, () => new TransferCodeController(
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.TransferCode)));
            serviceFactory.Add(ControllerNames.MergeChoice, () => new MergeChoiceController(
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.MergeChoice)));
            serviceFactory.Add(ControllerNames.OpenSafe, () => new OpenSafeController(
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.OpenSafe)));
            serviceFactory.Add(ControllerNames.ChangePassword, () => new ChangePasswordController(
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.ChangePassword)));
            serviceFactory.Add(ControllerNames.Export, () => new ExportController(
                Ioc.Default.GetService<RazorViewFactory>().GetByKey(ViewNames.Export)));
        }

        /// <summary>
        /// Registers all available views.
        /// </summary>
        public static void RegisterRazorViews(ServiceCollection services)
        {
            var serviceFactory = new RazorViewFactory();
            services.AddSingleton<RazorViewFactory>(serviceFactory);

            serviceFactory.Add(ViewNames.NoteRepository, () => new RazorViewService<NoteRepositoryRazorView>());
            serviceFactory.Add(ViewNames.NoteRepositoryContent, () => new RazorViewService<NoteRepositoryContentRazorView>());
            serviceFactory.Add(ViewNames.Note, () => new RazorViewService<NoteRazorView>());
            serviceFactory.Add(ViewNames.Checklist, () => new RazorViewService<ChecklistRazorView>());
            serviceFactory.Add(ViewNames.Info, () => new RazorViewService<InfoRazorView>());
            serviceFactory.Add(ViewNames.RecycleBin, () => new RazorViewService<RecycleBinRazorView>());
            serviceFactory.Add(ViewNames.RecycleBinContent, () => new RazorViewService<RecycleBinContentRazorView>());
            serviceFactory.Add(ViewNames.Settings, () => new RazorViewService<SettingsRazorView>());
            serviceFactory.Add(ViewNames.TransferCodeHistory, () => new RazorViewService<TransferCodeHistoryRazorView>());
            serviceFactory.Add(ViewNames.Stop, () => new RazorViewService<StopRazorView>());
            serviceFactory.Add(ViewNames.FirstTimeSync, () => new RazorViewService<FirstTimeSyncRazorView>());
            serviceFactory.Add(ViewNames.CloudStorageChoice, () => new RazorViewService<CloudStorageChoiceRazorView>());
            serviceFactory.Add(ViewNames.CloudStorageAccount, () => new RazorViewService<CloudStorageAccountRazorView>());
            serviceFactory.Add(ViewNames.CloudStorageOauthWaiting, () => new RazorViewService<CloudStorageOauthWaitingRazorView>());
            serviceFactory.Add(ViewNames.TransferCode, () => new RazorViewService<TransferCodeRazorView>());
            serviceFactory.Add(ViewNames.MergeChoice, () => new RazorViewService<MergeChoiceRazorView>());
            serviceFactory.Add(ViewNames.OpenSafe, () => new RazorViewService<OpenSafeRazorView>());
            serviceFactory.Add(ViewNames.ChangePassword, () => new RazorViewService<ChangePasswordRazorView>());
            serviceFactory.Add(ViewNames.Export, () => new RazorViewService<ExportRazorView>());
        }

        /// <summary>
        /// Registers all available cloud storage clients.
        /// </summary>
        public static void RegisterCloudStorageClientFactory(ServiceCollection services)
        {
            ICloudStorageClientFactory serviceFactory = new CloudStorageClientFactory();
            services.AddSingleton<ICloudStorageClientFactory>(serviceFactory);
        }
    }
}
