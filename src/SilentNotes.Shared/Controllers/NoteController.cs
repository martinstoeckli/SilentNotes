// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using SilentNotes.HtmlView;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.ViewModels;

namespace SilentNotes.Controllers
{
    /// <summary>
    /// Controller to show a <see cref="NoteViewModel"/> in an (Html)view.
    /// </summary>
    public class NoteController : ControllerBase
    {
        private readonly IRepositoryStorageService _repositoryService;
        private NoteViewModel _viewModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteController"/> class.
        /// </summary>
        /// <param name="repositoryService">A service which can provide a note repository.</param>
        /// <param name="viewService">Razor view service which can generate the HTML view.</param>
        public NoteController(IRepositoryStorageService repositoryService, IRazorViewService viewService)
            : base(viewService)
        {
            _repositoryService = repositoryService;
        }

        /// <inheritdoc/>
        protected override IViewModel GetViewModel()
        {
            return _viewModel;
        }

        /// <inheritdoc/>
        public override void ShowInView(IHtmlView htmlView, KeyValueList<string, string> variables)
        {
            Guid noteId = new Guid(variables["id"]);
            base.ShowInView(htmlView, variables);

            _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);
            NoteModel note = noteRepository.Notes.Find(item => noteId == item.Id);

            _viewModel = new NoteViewModel(
                Ioc.GetOrCreate<INavigationService>(),
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<ISvgIconService>(),
                Ioc.GetOrCreate<IBaseUrlService>(),
                null,
                _repositoryService,
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<IEnvironmentService>(),
                note);

            Bindings.BindCommand("GoBack", _viewModel.GoBackCommand);
            Bindings.UnhandledViewBindingEvent += UnhandledViewBindingEventHandler;

            string html = _viewService.GenerateHtml(_viewModel);
            View.LoadHtml(html);
        }

        private async void UnhandledViewBindingEventHandler(object sender, HtmlViewBindingNotifiedEventArgs e)
        {
            switch (e.BindingName?.ToLowerInvariant())
            {
                case "backgroundcolorhex":
                    _viewModel.BackgroundColorHex = e.Parameters["data-backgroundcolorhex"];
                    SetBackgroundColorToView(_viewModel.BackgroundColorHex);
                    break;
                case "quill":
                    string content = await View.ExecuteJavaScriptReturnString("getNoteHtmlContent();");
                    _viewModel.HtmlContent = content;
                    break;
            }
        }

        private void SetBackgroundColorToView(string backgroundColorHex)
        {
            string script = string.Format(
                "htmlViewBindingsSetCss('Content', 'background-color', '{0}');",
                backgroundColorHex);
            string darkClass = _viewModel.GetDarkClass();
            if (!string.IsNullOrEmpty(darkClass))
                script += "htmlViewBindingsAddClass('quill', 'dark');";
            else
                script += "htmlViewBindingsRemoveClass('quill', 'dark');";
            View.ExecuteJavaScript(script);
        }
    }
}
