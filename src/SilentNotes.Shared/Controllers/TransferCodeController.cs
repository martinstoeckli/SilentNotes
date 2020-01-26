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
    /// Controller to show a <see cref="TransferCodeViewModel"/> in an (Html)view.
    /// </summary>
    public class TransferCodeController : ControllerBase
    {
        private TransferCodeViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransferCodeController"/> class.
        /// </summary>
        /// <param name="viewService">Razor view service which can generate the HTML view.</param>
        public TransferCodeController(IRazorViewService viewService)
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
            _viewModel = new TransferCodeViewModel(
                Ioc.GetOrCreate<INavigationService>(),
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<ISvgIconService>(),
                Ioc.GetOrCreate<IThemeService>(),
                Ioc.GetOrCreate<IBaseUrlService>(),
                Ioc.GetOrCreate<IStoryBoardService>(),
                Ioc.GetOrCreate<IFeedbackService>());

            Bindings.BindCommand("OkCommand", _viewModel.OkCommand);
            Bindings.BindCommand("GoBack", _viewModel.GoBackCommand);
            Bindings.BindCommand("CancelCommand", _viewModel.CancelCommand);
            Bindings.BindText("Code", () => _viewModel.Code, (v) => _viewModel.Code = v, null, null, HtmlViewBindingMode.OneWayToViewmodel);

            string html = _viewService.GenerateHtml(_viewModel);
            View.LoadHtml(html);
        }
    }
}
