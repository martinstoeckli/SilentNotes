// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using SilentNotes.HtmlView;
using SilentNotes.Services;
using SilentNotes.ViewModels;
using SilentNotes.Workers;

namespace SilentNotes.Controllers
{
    /// <summary>
    /// Controller to show a <see cref="NoteRepositoryViewModel"/> in an (Html)view.
    /// </summary>
    public class NoteRepositoryController : ControllerBase
    {
        private readonly IRazorViewService _viewContentService;
        private readonly IRazorViewService _viewStop;
        private NoteRepositoryViewModel _viewModel;
        private StopViewModel _stopViewModel;
        private string _scrollToNote;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteRepositoryController"/> class.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1611:ElementParametersMustBeDocumented", Justification = "Dependency injection")]
        public NoteRepositoryController(
            IRazorViewService viewService,
            IRazorViewService viewContentService,
            IRazorViewService stopService)
            : base(viewService)
        {
            _viewContentService = viewContentService;
            _viewStop = stopService;
        }

        /// <inheritdoc/>
        protected override void OverrideableDispose()
        {
            _viewModel.PropertyChanged -= ViewmodelPropertyChangedEventHandler;
            View.NavigationCompleted -= NavigationCompletedEventHandler;
            base.OverrideableDispose();
        }

        /// <inheritdoc/>
        protected override IViewModel GetViewModel()
        {
            if (_viewModel != null)
                return _viewModel;
            else
                return _stopViewModel;
        }

        /// <inheritdoc/>
        public override void ShowInView(IHtmlView htmlView, KeyValueList<string, string> variables, Navigation redirectedFrom)
        {
            base.ShowInView(htmlView, variables,redirectedFrom);
            View.NavigationCompleted += NavigationCompletedEventHandler;
            IRepositoryStorageService repositoryService = Ioc.GetOrCreate<IRepositoryStorageService>();
            _scrollToNote = variables?.GetValueOrDefault(ControllerParameters.NoteId);

            RepositoryStorageLoadResult loadResult = repositoryService.LoadRepositoryOrDefault(out _);
            if (loadResult != RepositoryStorageLoadResult.InvalidRepository)
            {
                _viewModel = new NoteRepositoryViewModel(
                    Ioc.GetOrCreate<INavigationService>(),
                    Ioc.GetOrCreate<ILanguageService>(),
                    Ioc.GetOrCreate<ISvgIconService>(),
                    Ioc.GetOrCreate<IThemeService>(),
                    Ioc.GetOrCreate<IBaseUrlService>(),
                    Ioc.GetOrCreate<IStoryBoardService>(),
                    Ioc.GetOrCreate<IFeedbackService>(),
                    Ioc.GetOrCreate<ISettingsService>(),
                    Ioc.GetOrCreate<IEnvironmentService>(),
                    Ioc.GetOrCreate<ICryptoRandomService>(),
                    repositoryService);

                VueBindingShortcut[] shortcuts = new[]
                {
                    new VueBindingShortcut("s", nameof(_viewModel.SynchronizeCommand)) { Ctrl = true },
                    new VueBindingShortcut("n", nameof(_viewModel.NewNoteCommand)) { Ctrl = true },
                    new VueBindingShortcut("l", nameof(_viewModel.NewChecklistCommand)) { Ctrl = true },
                    new VueBindingShortcut("r", nameof(_viewModel.ShowRecycleBinCommand)) { Ctrl = true },
                    new VueBindingShortcut("i", nameof(_viewModel.ShowInfoCommand)) { Ctrl = true },
                };
                VueBindings = new VueDataBinding(_viewModel, View, shortcuts);
                _viewModel.VueDataBindingScript = VueBindings.BuildVueScript();
                VueBindings.UnhandledViewBindingEvent += UnhandledViewBindingEventHandler;
                VueBindings.StartListening();

                _viewModel.PropertyChanged += ViewmodelPropertyChangedEventHandler;

                // Load html page and content (notes)
                string html = _viewService.GenerateHtml(_viewModel);
                string htmlNotes = _viewContentService.GenerateHtml(_viewModel);
                html = html.Replace("<ul id=\"note-repository\"></ul>", htmlNotes); // Replace node "note-repository" with content
                View.LoadHtml(html);
            }
            else
            {
                // Show error message and stop loading the application
                _stopViewModel = new StopViewModel(
                    Ioc.GetOrCreate<INavigationService>(),
                    Ioc.GetOrCreate<ILanguageService>(),
                    Ioc.GetOrCreate<ISvgIconService>(),
                    Ioc.GetOrCreate<IThemeService>(),
                    Ioc.GetOrCreate<IBaseUrlService>());
                string html = _viewStop.GenerateHtml(_stopViewModel);
                View.LoadHtml(html);
            }
        }

        private void ViewmodelPropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Notes")
            {
                // Update the note list in the (HTML) view.
                string html = _viewContentService.GenerateHtml(_viewModel);
                View.ReplaceNode("note-repository", html);
                View.ExecuteJavaScript("makeSortable();");
            }
            else if (e.PropertyName == "ClearFilter")
            {
                View.ExecuteJavaScript("vm.Filter = '';");
            }
        }

        /// <inheritdoc/>
        protected override void SetHtmlViewBackgroundColor(IHtmlView htmlView)
        {
            IThemeService themeService = Ioc.GetOrCreate<IThemeService>();
            htmlView.SetBackgroundColor(ColorExtensions.HexToColor(themeService.SelectedTheme.ImageTint));
        }

        private void NavigationCompletedEventHandler(object sender, EventArgs e)
        {
            View.NavigationCompleted -= NavigationCompletedEventHandler;

            string scrollToNoteScript = BuildScrollToNoteScript(_scrollToNote);
            View.ExecuteJavaScript("makeSortable();" + scrollToNoteScript);
        }

        private static string BuildScrollToNoteScript(string noteId)
        {
            if (string.IsNullOrEmpty(noteId))
                return null;
            else
                return string.Format("vueFindFirst('[data-note=\"{0}\"]').scrollIntoView();", noteId);
        }

        private void SetVisibilityAddRemoveTresor(Guid noteId, bool isInSafe)
        {
            string bindingVisible = isInSafe ? "RemoveFromSafe" : "AddToSafe";
            string bindingInvisible = isInSafe ? "AddToSafe" : "RemoveFromSafe";
            string script = string.Format(
                "$(\"[data-note='{2}']\").children(\"[data-binding='{0}']\").removeClass('hidden'); $(\"[data-note='{2}']\").children(\"[data-binding='{1}']\").addClass('hidden');",
                bindingVisible,
                bindingInvisible,
                noteId.ToString());
            View.ExecuteJavaScript(script);
        }

        private void UnhandledViewBindingEventHandler(object sender, VueBindingUnhandledViewBindingEventArgs e)
        {
            Guid noteId;
            switch (e.PropertyName)
            {
                case "AddToSafeCommand":
                    noteId = new Guid(e.Value);
                    _viewModel.AddNoteToSafe(noteId);
                    SetVisibilityAddRemoveTresor(noteId, true);
                    break;
                case "RemoveFromSafeCommand":
                    noteId = new Guid(e.Value);
                    _viewModel.RemoveNoteFromSafe(noteId);
                    SetVisibilityAddRemoveTresor(noteId, false);
                    break;
                case "OrderChangedCommand":
                    int oldIndex = int.Parse(e.Parameters["oldIndex"]);
                    int newIndex = int.Parse(e.Parameters["newIndex"]);
                    _viewModel.MoveNote(oldIndex, newIndex);
                    break;
            }
        }
    }
}
