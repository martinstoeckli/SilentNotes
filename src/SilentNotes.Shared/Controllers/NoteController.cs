// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteController"/> class.
        /// </summary>
        /// <param name="repositoryService">A service which can provide a note repository.</param>
        /// <param name="viewService">Razor view service which can generate the HTML view.</param>
        public NoteController(IRepositoryStorageService repositoryService, IRazorViewService viewService)
            : base(viewService)
        {
            _repositoryService = repositoryService;
            WeakReferenceMessenger.Default.Register<SynchronizationAtStartupFinishedMessage>(this, SynchronizationAtStartupHandler);
        }

        /// <inheritdoc/>
        protected override void OverrideableDispose()
        {
            WeakReferenceMessenger.Default.Unregister<SynchronizationAtStartupFinishedMessage>(this);
            if (View != null)
            {
                View.HtmlView.Navigating -= NavigatingEventHandler;
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
        public override void ShowInView(IHtmlViewService htmlViewService, KeyValueList<string, string> variables, Navigation redirectedFrom)
        {
            base.ShowInView(htmlViewService, variables, redirectedFrom);
            ISettingsService settingsService = Ioc.Default.GetService<ISettingsService>();
            _repositoryService.LoadRepositoryOrDefault(out NoteRepositoryModel noteRepository);

            variables.TryGetValue(ControllerParameters.SearchFilter, out string startingSearchFilter);

            // Get the note from the repository
            Guid noteId = new Guid(variables[ControllerParameters.NoteId]);
            NoteModel note = noteRepository.Notes.FindById(noteId);

            ICryptor cryptor = new Cryptor(NoteModel.CryptorPackageName, Ioc.Default.GetService<ICryptoRandomService>());
            _viewModel = new NoteViewModel(
                Ioc.Default.GetService<INavigationService>(),
                Ioc.Default.GetService<ILanguageService>(),
                Ioc.Default.GetService<ISvgIconService>(),
                Ioc.Default.GetService<IThemeService>(),
                Ioc.Default.GetService<IBaseUrlService>(),
                null,
                _repositoryService,
                Ioc.Default.GetService<IFeedbackService>(),
                settingsService,
                Ioc.Default.GetService<IEnvironmentService>(),
                cryptor,
                noteRepository.Safes,
                noteRepository.CollectActiveTags(),
                note);
            _viewModel.AutoSynchronizationRunning = true; // Always start with controls in readonly mode.

            SetHtmlViewBackgroundColor(View.HtmlView);

            VueBindingShortcut[] shortcuts = new[]
            {
                new VueBindingShortcut("f", "ToggleSearchDialogCommand") { Ctrl = true },
                new VueBindingShortcut(VueBindingShortcut.KeyEscape, "CloseSearchDialogCommand"),
                new VueBindingShortcut("l", "ShowLinkDialog") { Ctrl = true },
                new VueBindingShortcut("F3", "FindNextCommand"),
                new VueBindingShortcut("F3", "FindPreviousCommand") { Shift = true },
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
            VueBindings.DeclareAdditionalVueData("SearchPattern", String.Format("'{0}'", startingSearchFilter));
            VueBindings.DeclareAdditionalVueMethod("ToggleSearchDialogCommand", "toggleSearchDialog();");
            VueBindings.DeclareAdditionalVueMethod("CloseSearchDialogCommand", "showSearchDialog(false);");
            VueBindings.DeclareAdditionalVueMethod("ShowLinkDialog", "showLinkDialog();");
            VueBindings.DeclareAdditionalVueMethod("FindNextCommand", "ProseMirrorBundle.selectNext(editor);");
            VueBindings.DeclareAdditionalVueMethod("FindPreviousCommand", "ProseMirrorBundle.selectPrevious(editor);");
            VueBindings.UnhandledViewBindingEvent += UnhandledViewBindingEventHandler;
            VueBindings.ViewLoadedEvent += ViewLoadedEventHandler;
            _viewModel.VueDataBindingScript = VueBindings.BuildVueScript();
            VueBindings.StartListening();

            string html = _viewService.GenerateHtml(_viewModel);
            View.HtmlView.LoadHtml(html);
        }

        private async void SynchronizationAtStartupHandler(object recipient, SynchronizationAtStartupFinishedMessage message)
        {
            if (_viewModel == null)
                return;

            await View.HtmlView.ExecuteJavaScriptReturnString("activateEditor();");
            _viewModel.AutoSynchronizationRunning = false;
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
                INativeBrowserService nativeBrowser = Ioc.Default.GetService<INativeBrowserService>();
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
                string content = await View.HtmlView.ExecuteJavaScriptReturnString("getNoteHtmlContent();");
                _viewModel.UnlockedHtmlContent = content;
            }
        }

        private async void ViewLoadedEventHandler(object sender, EventArgs e)
        {
            VueBindings.ViewLoadedEvent -= ViewLoadedEventHandler;
            View.HtmlView.Navigating += NavigatingEventHandler;

            // Remove readonly mode when no auto synchronization is running. If a synchronization
            // is still running it will send a message at the end of the synchronization.
            IAutoSynchronizationService autoSynchronizationService = Ioc.Default.GetService<IAutoSynchronizationService>();
            bool noSynchronizationRunning = !autoSynchronizationService.IsRunning;

            // To load the content in the view, the javascript would have to be written into the page,
            // which would have to be interpreted by the WebView and would increase the size of the content.
            // Loading it here avoids this performance issue.
            string escapedContent = _viewModel.GetEscapedUnlockedHtmlContent();
            StringBuilder script = new StringBuilder(escapedContent.Length + 90);
            script.Append("initializeNoteContent('");
            script.Append(escapedContent);
            script.Append("');");
            bool isNewNote = _viewModel.SearchableContent.Trim().Length == 0;
            if (isNewNote)
                script.Append("toggleFormat('heading', 1);");
            script.Append("startSendingViewModelUpdates();");
            if (noSynchronizationRunning)
                script.Append("activateEditor();");

            await View.HtmlView.ExecuteJavaScriptReturnString(script.ToString());
            if (noSynchronizationRunning)
                _viewModel.AutoSynchronizationRunning = false;
        }
    }
}
