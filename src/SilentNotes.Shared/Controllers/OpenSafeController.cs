// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.HtmlView;
using SilentNotes.Services;
using SilentNotes.ViewModels;
using VanillaCloudStorageClient;

namespace SilentNotes.Controllers
{
    /// <summary>
    /// Controller to show the current transfercode and its history in an (Html)view.
    /// </summary>
    public class OpenSafeController : ControllerBase
    {
        private OpenSafeViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSafeController"/> class.
        /// </summary>
        /// <param name="viewService">Razor view service which can generate the HTML view.</param>
        public OpenSafeController(IRazorViewService viewService)
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
            _viewModel = new OpenSafeViewModel(
                Ioc.GetOrCreate<INavigationService>(),
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<ISvgIconService>(),
                Ioc.GetOrCreate<IThemeService>(),
                Ioc.GetOrCreate<IBaseUrlService>(),
                Ioc.GetOrCreate<IFeedbackService>(),
                Ioc.GetOrCreate<ICryptoRandomService>(),
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<IRepositoryStorageService>(),
                RedirectedFrom);

            Bindings.BindCommand("GoBack", _viewModel.GoBackCommand);
            Bindings.BindCommand("OkCommand", _viewModel.OkCommand);
            Bindings.BindCommand("CancelCommand", _viewModel.CancelCommand);
            Bindings.BindCommand("ResetSafeCommand", _viewModel.ResetSafeCommand);
            Bindings.BindText("Password", null, (v) => _viewModel.Password = SecureStringExtensions.StringToSecureString(v), null, null, HtmlViewBindingMode.OneWayToViewmodel);
            Bindings.BindText("PasswordConfirmation", null, (v) => _viewModel.PasswordConfirmation = SecureStringExtensions.StringToSecureString(v), null, null, HtmlViewBindingMode.OneWayToViewmodel);
            Bindings.BindInvalid("Password", () => _viewModel.InvalidPasswordError, _viewModel, nameof(_viewModel.InvalidPasswordError), HtmlViewBindingMode.OneWayToView);
            Bindings.BindInvalid("PasswordConfirmation", () => _viewModel.InvalidPasswordConfirmationError, _viewModel, nameof(_viewModel.InvalidPasswordConfirmationError), HtmlViewBindingMode.OneWayToView);

            string html = _viewService.GenerateHtml(_viewModel);
            View.LoadHtml(html);
        }
    }
}
