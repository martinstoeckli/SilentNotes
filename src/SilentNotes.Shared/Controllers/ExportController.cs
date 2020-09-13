// Copyright © 2020 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using SilentNotes.HtmlView;
using SilentNotes.Services;
using SilentNotes.ViewModels;

namespace SilentNotes.Controllers
{
    public class ExportController : ControllerBase
    {
        private ExportViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoController"/> class.
        /// </summary>
        /// <param name="viewService">Razor view service which can generate the HTML view.</param>
        public ExportController(IRazorViewService viewService)
            : base(viewService)
        {
        }

        /// <inheritdoc/>
        public override void ShowInView(IHtmlView htmlView, KeyValueList<string, string> variables, Navigation redirectedFrom)
        {
            base.ShowInView(htmlView, variables, redirectedFrom);
            _viewModel = new ExportViewModel(
                Ioc.GetOrCreate<INavigationService>(),
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<ISvgIconService>(),
                Ioc.GetOrCreate<IThemeService>(),
                Ioc.GetOrCreate<IBaseUrlService>(),
                Ioc.GetOrCreate<IFeedbackService>(),
                Ioc.GetOrCreate<ICryptoRandomService>(),
                Ioc.GetOrCreate<IRepositoryStorageService>(),
                Ioc.GetOrCreate<IFolderPickerService>());

            VueBindingShortcut[] shortcuts = new[]
            {
                new VueBindingShortcut(VueBindingShortcut.KeyEscape, nameof(InfoViewModel.GoBackCommand)),
            };
            VueBindings = new VueDataBinding(_viewModel, View, shortcuts);
            _viewModel.VueDataBindingScript = VueBindings.BuildVueScript();
            VueBindings.StartListening();

            string html = _viewService.GenerateHtml(_viewModel);
            View.LoadHtml(html);
        }

        /// <inheritdoc/>
        protected override IViewModel GetViewModel()
        {
            return _viewModel;
        }
    }
}
