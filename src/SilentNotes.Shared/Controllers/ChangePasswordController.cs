// Copyright © 2019 Martin Stoeckli.
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
    /// Controller to show the password change dialog.
    /// </summary>
    public class ChangePasswordController : ControllerBase
    {
        private ChangePasswordViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChangePasswordController"/> class.
        /// </summary>
        /// <param name="viewService">Razor view service which can generate the HTML view.</param>
        public ChangePasswordController(IRazorViewService viewService)
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
            _viewModel = new ChangePasswordViewModel(
                Ioc.GetOrCreate<INavigationService>(),
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<ISvgIconService>(),
                Ioc.GetOrCreate<IThemeService>(),
                Ioc.GetOrCreate<IBaseUrlService>(),
                Ioc.GetOrCreate<IFeedbackService>(),
                Ioc.GetOrCreate<ICryptoRandomService>(),
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<IRepositoryStorageService>());

            Bindings.BindCommand("GoBack", _viewModel.GoBackCommand);
            Bindings.BindCommand("OkCommand", _viewModel.OkCommand);
            Bindings.BindCommand("CancelCommand", _viewModel.CancelCommand);
            Bindings.BindText("OldPassword", null, (v) => _viewModel.OldPassword = SecureStringExtensions.StringToSecureString(v), null, null, HtmlViewBindingMode.OneWayToViewmodel);
            Bindings.BindText("Password", null, (v) => _viewModel.Password = SecureStringExtensions.StringToSecureString(v), null, null, HtmlViewBindingMode.OneWayToViewmodel);
            Bindings.BindText("PasswordConfirmation", null, (v) => _viewModel.PasswordConfirmation = SecureStringExtensions.StringToSecureString(v), null, null, HtmlViewBindingMode.OneWayToViewmodel);
            Bindings.BindInvalid("OldPassword", () => _viewModel.InvalidOldPasswordError, _viewModel, nameof(_viewModel.InvalidOldPasswordError), HtmlViewBindingMode.OneWayToView);
            Bindings.BindInvalid("Password", () => _viewModel.InvalidPasswordError, _viewModel, nameof(_viewModel.InvalidPasswordError), HtmlViewBindingMode.OneWayToView);
            Bindings.BindInvalid("PasswordConfirmation", () => _viewModel.InvalidPasswordConfirmationError, _viewModel, nameof(_viewModel.InvalidPasswordConfirmationError), HtmlViewBindingMode.OneWayToView);

            string html = _viewService.GenerateHtml(_viewModel);
            View.LoadHtml(html);
        }
    }
}
