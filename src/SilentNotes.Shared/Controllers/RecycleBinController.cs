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
                Ioc.GetOrCreate<INavigationService>(),
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<ISvgIconService>(),
                Ioc.GetOrCreate<IThemeService>(),
                Ioc.GetOrCreate<IBaseUrlService>(),
                Ioc.GetOrCreate<IFeedbackService>(),
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<ICryptoRandomService>(),
                Ioc.GetOrCreate<IRepositoryStorageService>());

            Bindings.BindCommand("GoBack", _viewModel.GoBackCommand);
            Bindings.BindCommand("EmptyRecycleBin", _viewModel.EmptyRecycleBinCommand);
            Bindings.BindGeneric<object>(
                NotesChangedEventHandler,
                null,
                null,
                null,
                new HtmlViewBindingViewmodelNotifier(_viewModel, "Notes"),
                HtmlViewBindingMode.OneWayToViewPlusOneTimeToView);
            Bindings.UnhandledViewBindingEvent += BindingsEventHandler;

            string html = _viewService.GenerateHtml(_viewModel);
            htmlView.LoadHtml(html);
        }

        private void NotesChangedEventHandler(object obj)
        {
            // Update the note list in the (HTML) view.
            string html = _viewContentService.GenerateHtml(_viewModel);
            View.ReplaceNode("recycled-notes", html);
        }

        private void BindingsEventHandler(object sender, HtmlViewBindingNotifiedEventArgs e)
        {
            if (e.BindingName == "restore_note")
            {
                Guid noteId = new Guid(e.Parameters["data-note"]);
                _viewModel.RestoreNoteCommand.Execute(noteId);
            }
        }
    }
}
