// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using CommunityToolkit.Mvvm.DependencyInjection;
using SilentNotes.HtmlView;
using SilentNotes.Services;
using SilentNotes.StoryBoards.SynchronizationStory;
using SilentNotes.ViewModels;
using VanillaCloudStorageClient;

namespace SilentNotes.Controllers
{
    /// <summary>
    /// Controller to show a <see cref="CloudStorageAccountViewModel"/> in an (Html)view.
    /// </summary>
    public class CloudStorageAccountController : ControllerBase
    {
        private CloudStorageAccountViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudStorageAccountController"/> class.
        /// </summary>
        /// <param name="viewService">Razor view service which can generate the HTML view.</param>
        public CloudStorageAccountController(IRazorViewService viewService)
            : base(viewService)
        {
        }

        /// <inheritdoc/>
        protected override IViewModel GetViewModel()
        {
            return _viewModel;
        }

        /// <inheritdoc/>
        public override void ShowInView(IHtmlView htmlView, KeyValueList<string, string> variables, Navigation redirectedFrom)
        {
            base.ShowInView(htmlView, variables, redirectedFrom);
            IStoryBoardService storyBoardService = Ioc.Default.GetService<IStoryBoardService>();
            SerializeableCloudStorageCredentials credentials = storyBoardService.ActiveStory.LoadFromSession<SerializeableCloudStorageCredentials>(SynchronizationStorySessionKey.CloudStorageCredentials);

            _viewModel = new CloudStorageAccountViewModel(
                Ioc.Default.GetService<INavigationService>(),
                Ioc.Default.GetService<ILanguageService>(),
                Ioc.Default.GetService<ISvgIconService>(),
                Ioc.Default.GetService<IThemeService>(),
                Ioc.Default.GetService<IBaseUrlService>(),
                storyBoardService,
                Ioc.Default.GetService<IFeedbackService>(),
                Ioc.Default.GetService<ICloudStorageClientFactory>(),
                credentials);

            VueBindingShortcut[] shortcuts = new[]
            {
                new VueBindingShortcut(VueBindingShortcut.KeyEscape, nameof(CloudStorageAccountViewModel.CancelCommand)),
                new VueBindingShortcut(VueBindingShortcut.KeyEnter, nameof(CloudStorageAccountViewModel.OkCommand)),
            };
            VueBindings = new VueDataBinding(_viewModel, View, shortcuts);
            _viewModel.VueDataBindingScript = VueBindings.BuildVueScript();
            VueBindings.StartListening();

            string html = _viewService.GenerateHtml(_viewModel);
            View.LoadHtml(html);
        }
    }
}
