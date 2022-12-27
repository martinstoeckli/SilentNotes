// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using CommunityToolkit.Mvvm.DependencyInjection;
using SilentNotes.HtmlView;
using SilentNotes.Services;
using SilentNotes.ViewModels;

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
                Ioc.Default.GetService<INavigationService>(),
                Ioc.Default.GetService<ILanguageService>(),
                Ioc.Default.GetService<ISvgIconService>(),
                Ioc.Default.GetService<IThemeService>(),
                Ioc.Default.GetService<IBaseUrlService>(),
                Ioc.Default.GetService<IFeedbackService>(),
                Ioc.Default.GetService<ICryptoRandomService>(),
                Ioc.Default.GetService<ISettingsService>(),
                Ioc.Default.GetService<IRepositoryStorageService>(),
                RedirectedFrom);

            VueBindingShortcut[] shortcuts = new[]
            {
                new VueBindingShortcut(VueBindingShortcut.KeyEscape, nameof(OpenSafeViewModel.GoBackCommand)),
                new VueBindingShortcut(VueBindingShortcut.KeyEnter, nameof(OpenSafeViewModel.OkCommand)),
            };
            VueBindings = new VueDataBinding(_viewModel, View, shortcuts);
            _viewModel.VueDataBindingScript = VueBindings.BuildVueScript();
            VueBindings.StartListening();

            string html = _viewService.GenerateHtml(_viewModel);
            View.LoadHtml(html);
        }
    }
}
