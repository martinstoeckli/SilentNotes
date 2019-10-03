// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Web;
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
        private bool _startedWithSendToSilentnotes;
        private string _sendToSilentnotesText;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteController"/> class.
        /// </summary>
        /// <param name="repositoryService">A service which can provide a note repository.</param>
        /// <param name="viewService">Razor view service which can generate the HTML view.</param>
        public NoteController(IRepositoryStorageService repositoryService, IRazorViewService viewService)
            : base(viewService)
        {
            _repositoryService = repositoryService;
            _startedWithSendToSilentnotes = false;
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
            ISettingsService settingsService = Ioc.GetOrCreate<ISettingsService>();
            View.NavigationCompleted += NavigationCompletedEventHandler;
            _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);

            NoteModel note;
            _startedWithSendToSilentnotes = variables.ContainsKey(ControllerParameters.SendToSilentnotesText);
            if (_startedWithSendToSilentnotes)
            {
                // Create new note and update repository
                note = new NoteModel();
                note.BackgroundColorHex = settingsService.LoadSettingsOrDefault().DefaultNoteColorHex;
                noteRepository.Notes.Insert(0, note);
                _sendToSilentnotesText = variables[ControllerParameters.SendToSilentnotesText];
            }
            else
            {
                // Get the note from the repository
                Guid noteId = new Guid(variables[ControllerParameters.NoteId]);
                note = noteRepository.Notes.Find(item => noteId == item.Id);
            }

            _viewModel = new NoteViewModel(
                Ioc.GetOrCreate<INavigationService>(),
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<ISvgIconService>(),
                Ioc.GetOrCreate<IBaseUrlService>(),
                null,
                _repositoryService,
                Ioc.GetOrCreate<IFeedbackService>(),
                settingsService,
                note);

            Bindings.BindCommand("PullNoteFromOnlineStorage", _viewModel.PullNoteFromOnlineStorageCommand);
            Bindings.BindCommand("PushNoteToOnlineStorage", _viewModel.PushNoteToOnlineStorageCommand);
            Bindings.BindCommand("GoBack", _viewModel.GoBackCommand);
            Bindings.UnhandledViewBindingEvent += UnhandledViewBindingEventHandler;

            string html = _viewService.GenerateHtml(_viewModel);
            View.LoadHtml(html);
        }

        private void NavigationCompletedEventHandler(object sender, EventArgs e)
        {
            View.NavigationCompleted -= NavigationCompletedEventHandler;
            if (_startedWithSendToSilentnotes)
            {
                // Let quill do the text import, so it can convert it safely to HTML and trigger
                // the "quill" event which eventually sets the modified property.
                string encodedNewText = HttpUtility.JavaScriptStringEncode(_sendToSilentnotesText, false);
                string script = string.Format("setNoteHtmlContent('{0}');", encodedNewText);
                View.ExecuteJavaScript(script);
            }
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

        private void SetBackgroundColorToView(string colorHex)
        {
            string script = string.Format(
                "htmlViewBindingsSetCss('Content', 'background-color', '{0}');", colorHex);
            string darkClass = _viewModel.GetDarkClass();
            if (!string.IsNullOrEmpty(darkClass))
                script += "htmlViewBindingsAddClass('quill', 'dark');";
            else
                script += "htmlViewBindingsRemoveClass('quill', 'dark');";
            View.ExecuteJavaScript(script);
        }
    }
}
