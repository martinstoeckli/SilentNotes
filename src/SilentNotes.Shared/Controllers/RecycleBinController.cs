// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using CommunityToolkit.Mvvm.DependencyInjection;
using SilentNotes.HtmlView;
using SilentNotes.Services;
using SilentNotes.ViewModels;
using SilentNotes.Workers;

namespace SilentNotes.Controllers
{
    /// <summary>
    /// Controller to show a <see cref="InfoViewModel"/> in an (Html)view.
    /// </summary>
    public class RecycleBinController : ControllerBase
    {
        private readonly IRazorViewService _viewContentService;
        private RecycleBinViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecycleBinController"/> class.
        /// </summary>
        /// <param name="viewService">Razor view service which can generate the HTML view.</param>
        /// <param name="viewContentService">Razor view service which can generate the HTML of
        /// the notes.</param>
        public RecycleBinController(IRazorViewService viewService, IRazorViewService viewContentService)
            : base(viewService)
        {
            _viewContentService = viewContentService;
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
            _viewModel = new RecycleBinViewModel(
                Ioc.Default.GetService<INavigationService>(),
                Ioc.Default.GetService<ILanguageService>(),
                Ioc.Default.GetService<ISvgIconService>(),
                Ioc.Default.GetService<IThemeService>(),
                Ioc.Default.GetService<IBaseUrlService>(),
                Ioc.Default.GetService<IFeedbackService>(),
                Ioc.Default.GetService<ISettingsService>(),
                Ioc.Default.GetService<ICryptoRandomService>(),
                Ioc.Default.GetService<IRepositoryStorageService>());

            VueBindingShortcut[] shortcuts = new[]
            {
                new VueBindingShortcut(VueBindingShortcut.KeyEscape, nameof(RecycleBinViewModel.GoBackCommand)),
            };
            VueBindings = new VueDataBinding(_viewModel, View, shortcuts);
            _viewModel.VueDataBindingScript = VueBindings.BuildVueScript();
            VueBindings.StartListening();

            string html = _viewService.GenerateHtml(_viewModel);
            string htmlNotes = _viewContentService.GenerateHtml(_viewModel);
            html = html.Replace("<ul id=\"recycled-notes\"></ul>", htmlNotes);
            htmlView.LoadHtml(html);
        }

        /// <inheritdoc/>
        protected override void SetHtmlViewBackgroundColor(IHtmlView htmlView)
        {
            htmlView.SetBackgroundColor(ColorExtensions.HexToColor("#323232"));
        }
    }
}
