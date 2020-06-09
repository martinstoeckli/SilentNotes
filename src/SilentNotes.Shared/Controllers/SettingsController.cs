// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
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
        public override void ShowInView(IHtmlView htmlView, KeyValueList<string, string> variables, Navigation redirectedFrom)
        {
            base.ShowInView(htmlView, variables, redirectedFrom);
            _viewModel = new SettingsViewModel(
                Ioc.GetOrCreate<INavigationService>(),
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<ISvgIconService>(),
                Ioc.GetOrCreate<IThemeService>(),
                Ioc.GetOrCreate<IBaseUrlService>(),
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<IStoryBoardService>());

            Bindings.BindCommand("GoBack", _viewModel.GoBackCommand);
            Bindings.BindCommand("ChangeCloudSettingsCommand", _viewModel.ChangeCloudSettingsCommand);
            Bindings.BindCommand("ClearCloudSettingsCommand", _viewModel.ClearCloudSettingsCommand);
            Bindings.BindDropdown("SelectedEncryptionAlgorithm", null, SetEncryptionAlgorithmToViewmodel, null, null, HtmlViewBindingMode.OneWayToViewmodel);
            Bindings.BindDropdown("SelectedAutoSyncMode", null, (string value) => _viewModel.SelectedAutoSyncMode = value, null, null, HtmlViewBindingMode.OneWayToViewmodel);
            Bindings.BindText("CloudStorageSettings", () => _viewModel.AccountSummary, null, _viewModel, nameof(_viewModel.AccountSummary), HtmlViewBindingMode.OneWayToView);
            Bindings.BindBackgroundImage("SelectedTheme", () => _viewModel.SelectedTheme.Image, _viewModel, nameof(_viewModel.SelectedTheme), HtmlViewBindingMode.OneWayToView);
            Bindings.BindCheckbox("UseSolidColorTheme", null, (bool value) => _viewModel.UseSolidColorTheme = value, null, null, HtmlViewBindingMode.OneWayToViewmodel);
            Bindings.BindText("ColorForSolidThemeHex", null, (string value) => _viewModel.ColorForSolidThemeHex = value, null, null, HtmlViewBindingMode.OneWayToViewmodel);
            Bindings.BindDropdown("SelectedThemeMode", null, (string value) => _viewModel.SelectedThemeMode = value, null, null, HtmlViewBindingMode.OneWayToViewmodel);
            Bindings.BindDropdown("SelectedNoteInsertionMode", null, (string value) => _viewModel.SelectedNoteInsertionMode = value, null, null, HtmlViewBindingMode.OneWayToViewmodel);
            Bindings.BindCheckbox("UseColorForAllNotesInDarkMode", null, (bool value) => _viewModel.UseColorForAllNotesInDarkMode = value, null, null, HtmlViewBindingMode.OneWayToViewmodel);
            Bindings.BindText("ColorForAllNotesInDarkModeHex", null, (string value) => _viewModel.ColorForAllNotesInDarkModeHex = value, null, null, HtmlViewBindingMode.OneWayToViewmodel);
            Bindings.UnhandledViewBindingEvent += UnhandledViewBindingEventHandler;

            string html = _viewService.GenerateHtml(_viewModel);
            View.LoadHtml(html);
            View.NavigationCompleted += NavigationCompletedHandler;
        }

        private void NavigationCompletedHandler(object sender, EventArgs e)
        {
            View.NavigationCompleted -= NavigationCompletedHandler;

            VueJsDataBinding vueBinding = new VueJsDataBinding(
                _viewModel,
                View,
                new [] 
                {
                    new BindingDescription { PropertyName = nameof(_viewModel.FontSizeStep), Mode = HtmlViewBindingMode.TwoWayPlusOneTimeToView } 
                });
            vueBinding.StartListening();
        }

        private void SetEncryptionAlgorithmToViewmodel(string value)
        {
            _viewModel.SelectedEncryptionAlgorithm = _viewModel.EncryptionAlgorithms.Find(
                (item) => item.Value == value);
        }

        private void UnhandledViewBindingEventHandler(object sender, HtmlViewBindingNotifiedEventArgs e)
        {
            switch (e.BindingName)
            {
                case "SelectedThemePreview":
                    string themeId = e.Parameters["data-theme"];
                    _viewModel.SelectedTheme = _viewModel.Theme.Themes.Find(item => item.Id == themeId);
                    break;
                case "DefaultNoteColorPreview":
                    _viewModel.DefaultNoteColorHex = e.Parameters["data-notecolorhex"];
                    SetDefaultNoteColorToView(_viewModel.DefaultNoteColorHex);
                    break;
                case "FontSize":
                    _viewModel.FontSizeStep = e.Parameters["value"];
                    break;
            }
        }

        private void SetDefaultNoteColorToView(string colorHex)
        {
            string script = string.Format(
                "htmlViewBindingsSetCss('DefaultNoteColor', 'background-color', '{0}');", colorHex);
            View.ExecuteJavaScript(script);
        }
    }
}
