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
    /// Controller to show the current transfercode and its history in an (Html)view.
    /// </summary>
    public class TransferCodeHistoryController : ControllerBase
    {
        private TransferCodeHistoryViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransferCodeHistoryController"/> class.
        /// </summary>
        /// <param name="viewService">Razor view service whichcan generate the HTML view.</param>
        public TransferCodeHistoryController(IRazorViewService viewService)
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
            _viewModel = new TransferCodeHistoryViewModel(
                Ioc.GetOrCreate<INavigationService>(),
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<ISvgIconService>(),
                Ioc.GetOrCreate<IThemeService>(),
                Ioc.GetOrCreate<IBaseUrlService>(),
                Ioc.GetOrCreate<ISettingsService>());

            Bindings.BindCommand("GoBack", _viewModel.GoBackCommand);
            Bindings.BindCommand("ShowTransfercodeHistoryCommand", _viewModel.ShowTransfercodeHistoryCommand);
            Bindings.BindVisibility("ShowTransfercodeHistoryCommand", () => _viewModel.ShowTransfercodeHistoryVisible, _viewModel, nameof(_viewModel.ShowTransfercodeHistoryVisible), HtmlViewBindingMode.OneWayToViewPlusOneTimeToView);
            Bindings.BindVisibility("TransfercodeHistory", () => _viewModel.TransfercodeHistoryVisible, _viewModel, nameof(_viewModel.TransfercodeHistoryVisible), HtmlViewBindingMode.OneWayToViewPlusOneTimeToView);

            string html = _viewService.GenerateHtml(_viewModel);
            View.LoadHtml(html);
        }
    }
}
