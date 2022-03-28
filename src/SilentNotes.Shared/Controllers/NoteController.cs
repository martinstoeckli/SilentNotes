// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using SilentNotes.Crypto;
using SilentNotes.HtmlView;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.ViewModels;
using SilentNotes.Workers;

namespace SilentNotes.Controllers
{
    /// <summary>
    /// Controller to show a <see cref="NoteViewModel"/> in an (Html)view.
    /// </summary>
    public class NoteController : ControllerBase
    {
        private readonly IRepositoryStorageService _repositoryService;
        private NoteViewModel _viewModel;
        private string _startingSearchFilter;

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
        protected override void OverrideableDispose()
        {
            if (View != null)
            {
                View.Navigating -= NavigatingEventHandler;
            }
            if (VueBindings != null)
            {
                VueBindings.UnhandledViewBindingEvent -= UnhandledViewBindingEventHandler;
                VueBindings.ViewLoadedEvent -= ViewLoadedEventHandler;
            }
            base.OverrideableDispose();
        }

        /// <inheritdoc/>
        protected override IViewModel GetViewModel()
        {
            return _viewModel;
        }

        /// <inheritdoc/>
        public override bool NeedsNavigationRedirect(Navigation original, out Navigation redirectTo)
        {
            if (original.Variables.TryGetValue(ControllerParameters.NoteId, out string noteId))
            {
                // Get the note from the repository
                _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);
                NoteModel note = noteRepository.Notes.FindById(new Guid(noteId));

                // Find its safe and check if it needs to be opened
                SafeModel safe = noteRepository.Safes.FindById(note?.SafeId);
                if ((safe != null) && (!safe.IsOpen))
                {
                    redirectTo = new Navigation(ControllerNames.OpenSafe, ControllerParameters.NoteId, noteId);
                    return true;
                }
            }
            redirectTo = null;
            return false;
        }

        /// <inheritdoc/>
        public override void ShowInView(IHtmlView htmlView, KeyValueList<string, string> variables, Navigation redirectedFrom)
        {
            base.ShowInView(htmlView, variables, redirectedFrom);
            ISettingsService settingsService = Ioc.GetOrCreate<ISettingsService>();
            _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);

            variables.TryGetValue(ControllerParameters.SearchFilter, out _startingSearchFilter);

            // Get the note from the repository
            Guid noteId = new Guid(variables[ControllerParameters.NoteId]);
            NoteModel note = noteRepository.Notes.FindById(noteId);

            ICryptor cryptor = new Cryptor(NoteModel.CryptorPackageName, Ioc.GetOrCreate<ICryptoRandomService>());
            _viewModel = new NoteViewModel(
                Ioc.GetOrCreate<INavigationService>(),
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<ISvgIconService>(),
                Ioc.GetOrCreate<IThemeService>(),
                Ioc.GetOrCreate<IBaseUrlService>(),
                null,
                _repositoryService,
                Ioc.GetOrCreate<IFeedbackService>(),
                settingsService,
                cryptor,
                noteRepository.Safes,
                noteRepository.CollectActiveTags(),
                note);
            SetHtmlViewBackgroundColor(htmlView);

            VueBindingShortcut[] shortcuts = new[]
            {
                new VueBindingShortcut("f", "ToggleSearchDialogCommand") { Ctrl = true },
                new VueBindingShortcut(VueBindingShortcut.KeyEscape, "CloseSearchDialogCommand"),
                new VueBindingShortcut(VueBindingShortcut.KeyHome, "ScrollToTop") { Ctrl = true },
                new VueBindingShortcut(VueBindingShortcut.KeyEnd, "ScrollToBottom") { Ctrl = true },
                new VueBindingShortcut("l", "ShowLinkDialog") { Ctrl = true },
            };
            VueBindings = new VueDataBinding(_viewModel, View, shortcuts);
            VueBindings.DeclareAdditionalVueData("PrettyTimeAgoVisible", "true");
            VueBindings.DeclareAdditionalVueData("Header1Active", "false");
            VueBindings.DeclareAdditionalVueData("Header2Active", "false");
            VueBindings.DeclareAdditionalVueData("Header3Active", "false");
            VueBindings.DeclareAdditionalVueData("BoldActive", "false");
            VueBindings.DeclareAdditionalVueData("ItalicActive", "false");
            VueBindings.DeclareAdditionalVueData("ListOrderedActive", "false");
            VueBindings.DeclareAdditionalVueData("ListBulletActive", "false");
            VueBindings.DeclareAdditionalVueData("CodeActive", "false");
            VueBindings.DeclareAdditionalVueData("QuoteActive", "false");
            VueBindings.DeclareAdditionalVueData("UnderlineActive", "false");
            VueBindings.DeclareAdditionalVueData("StrikeActive", "false");
            VueBindings.DeclareAdditionalVueData("OldLinkUrl", "''");
            VueBindings.DeclareAdditionalVueData("NewLinkUrl", "''");
            VueBindings.DeclareAdditionalVueMethod("ToggleSearchDialogCommand", "toggleSearchDialog();");
            VueBindings.DeclareAdditionalVueMethod("CloseSearchDialogCommand", "showSearchDialog(false);");
            VueBindings.DeclareAdditionalVueMethod("ScrollToTop", "scrollToTop();");
            VueBindings.DeclareAdditionalVueMethod("ScrollToBottom", "scrollToBottom();");
            VueBindings.DeclareAdditionalVueMethod("ShowLinkDialog", "showLinkDialog();");
            VueBindings.UnhandledViewBindingEvent += UnhandledViewBindingEventHandler;
            VueBindings.ViewLoadedEvent += ViewLoadedEventHandler;
            _viewModel.VueDataBindingScript = VueBindings.BuildVueScript();
            VueBindings.StartListening();

            string html = _viewService.GenerateHtml(_viewModel);
            View.LoadHtml(html);
        }

        /// <inheritdoc/>
        protected override void SetHtmlViewBackgroundColor(IHtmlView htmlView)
        {
            if (_viewModel != null)
                htmlView.SetBackgroundColor(ColorExtensions.HexToColor(_viewModel.BackgroundColorHex));
        }

        private void NavigatingEventHandler(object sender, string uri)
        {
            if (IsExternalLink(uri))
            {
                INativeBrowserService nativeBrowser = Ioc.GetOrCreate<INativeBrowserService>();
                nativeBrowser.OpenWebsite(uri);
            }
        }

        private static bool IsExternalLink(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                return false;

            string[] acceptedProtocols = { "http:", "https:", "mailto:" };
            foreach (string acceptedProtocol in acceptedProtocols)
            {
                if (uri.StartsWith(acceptedProtocol, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        private async void UnhandledViewBindingEventHandler(object sender, VueBindingUnhandledViewBindingEventArgs e)
        {
            if (string.Equals(e.PropertyName, nameof(_viewModel.UnlockedHtmlContent)))
            {
                string content = await View.ExecuteJavaScriptReturnString("getNoteHtmlContent();");
                _viewModel.UnlockedHtmlContent = content;
            }
        }

        private void ViewLoadedEventHandler(object sender, EventArgs e)
        {
            VueBindings.ViewLoadedEvent -= ViewLoadedEventHandler;
            View.Navigating += NavigatingEventHandler;

            if (!string.IsNullOrEmpty(_startingSearchFilter))
            {
                string encodedSearchFilter = WebviewUtils.EscapeJavaScriptString(_startingSearchFilter);
                string script = string.Format("setStartingSearchFilter('{0}');", encodedSearchFilter);
                View.ExecuteJavaScript(script);
            }
        }
    }
}
