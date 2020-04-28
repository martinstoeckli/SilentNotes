// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.HtmlView;
using SilentNotes.Services;
using SilentNotes.ViewModels;

namespace SilentNotes.Controllers
{
    /// <summary>
    /// Controller to show a <see cref="CloudStorageChoiceViewModel"/> in an (Html)view.
    /// </summary>
    public class CloudStorageChoiceController : ControllerBase
    {
        private CloudStorageChoiceViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudStorageChoiceController"/> class.
        /// </summary>
        /// <param name="viewService">Razor view service which can generate the HTML view.</param>
        public CloudStorageChoiceController(IRazorViewService viewService)
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
            _viewModel = new CloudStorageChoiceViewModel(
                Ioc.GetOrCreate<INavigationService>(),
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<ISvgIconService>(),
                Ioc.GetOrCreate<IThemeService>(),
                Ioc.GetOrCreate<IBaseUrlService>(),
                Ioc.GetOrCreate<IStoryBoardService>(),
                Ioc.GetOrCreate<ICloudStorageClientFactory>());

            Bindings.BindCommand("GoBack", _viewModel.GoBackCommand);
            Bindings.UnhandledViewBindingEvent += BindingsEventHandler;

            string html = _viewService.GenerateHtml(_viewModel);
            View.LoadHtml(html);
        }

        private void BindingsEventHandler(object sender, HtmlViewBindingNotifiedEventArgs e)
        {
            switch (e.BindingName.ToLowerInvariant())
            {
                case "choose":
                    string cloudStorageId = e.Parameters["data-cloudstorageid"];
                    _viewModel.ChooseCommand.Execute(cloudStorageId);
                    break;
            }
        }
    }
}
