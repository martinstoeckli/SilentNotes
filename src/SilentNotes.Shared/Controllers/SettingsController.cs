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
    /// Controller to show a <see cref="SettingsViewModel"/> in an (Html)view.
    /// </summary>
    public class SettingsController : ControllerBase
    {
        private SettingsViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsController"/> class.
        /// </summary>
        /// <param name="viewService">Razor view service which can generate the HTML view.</param>
        public SettingsController(IRazorViewService viewService)
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
            _viewModel = new SettingsViewModel(
                Ioc.GetOrCreate<INavigationService>(),
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<ISvgIconService>(),
                Ioc.GetOrCreate<IBaseUrlService>(),
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<IStoryBoardService>());

            Bindings.BindCommand("GoBack", _viewModel.GoBackCommand);
            Bindings.BindCommand("ChangeCloudSettingsCommand", _viewModel.ChangeCloudSettingsCommand);
            Bindings.BindCommand("ClearCloudSettingsCommand", _viewModel.ClearCloudSettingsCommand);
            Bindings.BindDropdown("SelectedEncryptionAlgorithm", null, SetEncryptionAlgorithmToViewmodel, null, null, HtmlViewBindingMode.OneWayToViewmodel);
            Bindings.BindCheckbox("AdoptCloudEncryptionAlgorithm", null, (value) => _viewModel.AdoptCloudEncryptionAlgorithm = value, null, null, HtmlViewBindingMode.OneWayToViewmodel);
            Bindings.BindText("CloudStorageSettings", () => _viewModel.AccountSummary, null, _viewModel, nameof(_viewModel.AccountSummary), HtmlViewBindingMode.OneWayToView);

            string html = _viewService.GenerateHtml(_viewModel);
            View.LoadHtml(html);
        }

        private void SetEncryptionAlgorithmToViewmodel(string value)
        {
            _viewModel.SelectedEncryptionAlgorithm = _viewModel.EncryptionAlgorithms.Find(
                (item) => item.AlgorithmName == value);
        }
    }
}
