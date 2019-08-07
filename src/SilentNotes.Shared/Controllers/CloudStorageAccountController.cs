// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

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
        public override void ShowInView(IHtmlView htmlView, KeyValueList<string, string> variables)
        {
            base.ShowInView(htmlView, variables);
            IStoryBoardService storyBoardService = Ioc.GetOrCreate<IStoryBoardService>();
            SerializeableCloudStorageCredentials credentials = storyBoardService.ActiveStory.LoadFromSession<SerializeableCloudStorageCredentials>(SynchronizationStorySessionKey.CloudStorageCredentials.ToInt());

            _viewModel = new CloudStorageAccountViewModel(
                Ioc.GetOrCreate<INavigationService>(),
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<ISvgIconService>(),
                Ioc.GetOrCreate<IBaseUrlService>(),
                storyBoardService,
                Ioc.GetOrCreate<IFeedbackService>(),
                Ioc.GetOrCreate<ICloudStorageClientFactory>(),
                credentials);

            Bindings.BindCommand("GoBack", _viewModel.GoBackCommand);
            Bindings.BindCommand("OkCommand", _viewModel.OkCommand);
            Bindings.BindCommand("CancelCommand", _viewModel.CancelCommand);
            Bindings.BindText("Url", null, (v) => _viewModel.Url = v, null, null, HtmlViewBindingMode.OneWayToViewmodel);
            Bindings.BindText("Username", null, (v) => _viewModel.Username = v, null, null, HtmlViewBindingMode.OneWayToViewmodel);
            Bindings.BindText("Password", () => SecureStringExtensions.SecureStringToString(_viewModel.Password), (v) => _viewModel.Password = SecureStringExtensions.StringToSecureString(v), _viewModel, nameof(_viewModel.Password), HtmlViewBindingMode.TwoWayPlusOneTimeToView);
            Bindings.BindCheckbox("Secure", null, (v) => _viewModel.Secure = v, null, null, HtmlViewBindingMode.OneWayToViewmodel);

            string html = _viewService.GenerateHtml(_viewModel);
            View.LoadHtml(html);
        }
    }
}
