// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
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
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewmodelPropertyChangedEventHandler;
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
            if (_viewModel != null)
                return _viewModel;
            else
                return _stopViewModel;
        }

        /// <inheritdoc/>
        public override void ShowInView(IHtmlViewService htmlViewService, KeyValueList<string, string> variables, Navigation redirectedFrom)
        {
            base.ShowInView(htmlViewService, variables, redirectedFrom);
            IRepositoryStorageService repositoryService = Ioc.Default.GetService<IRepositoryStorageService>();
            _scrollToNote = variables?.GetValueOrDefault(ControllerParameters.NoteId);

            RepositoryStorageLoadResult loadResult = repositoryService.LoadRepositoryOrDefault(out _);
            if (loadResult != RepositoryStorageLoadResult.InvalidRepository)
            {
                _viewModel = new NoteRepositoryViewModel(
                    Ioc.Default.GetService<INavigationService>(),
                    Ioc.Default.GetService<ILanguageService>(),
                    Ioc.Default.GetService<ISvgIconService>(),
                    Ioc.Default.GetService<IThemeService>(),
                    Ioc.Default.GetService<IBaseUrlService>(),
                    Ioc.Default.GetService<IStoryBoardService>(),
                    Ioc.Default.GetService<IFeedbackService>(),
                    Ioc.Default.GetService<ISettingsService>(),
                    Ioc.Default.GetService<IEnvironmentService>(),
                    Ioc.Default.GetService<IAutoSynchronizationService>(),
                    Ioc.Default.GetService<ICryptoRandomService>(),
                    repositoryService);

                VueBindingShortcut[] shortcuts = new[]
                {
                    new VueBindingShortcut("s", nameof(_viewModel.SynchronizeCommand)) { Ctrl = true },
                    new VueBindingShortcut("n", nameof(_viewModel.NewNoteCommand)) { Ctrl = true },
                    new VueBindingShortcut("l", nameof(_viewModel.NewChecklistCommand)) { Ctrl = true },
                    new VueBindingShortcut("r", nameof(_viewModel.ShowRecycleBinCommand)) { Ctrl = true },
                    new VueBindingShortcut("i", nameof(_viewModel.ShowInfoCommand)) { Ctrl = true },
                    new VueBindingShortcut(VueBindingShortcut.KeyHome, "ScrollToTop") { Ctrl = true },
                    new VueBindingShortcut(VueBindingShortcut.KeyEnd, "ScrollToBottom") { Ctrl = true },
                };
                VueBindings = new VueDataBinding(_viewModel, View, shortcuts);
                VueBindings.DeclareAdditionalVueMethod("ScrollToTop", "scrollToTop();");
                VueBindings.DeclareAdditionalVueMethod("ScrollToBottom", "scrollToBottom();");
                _viewModel.VueDataBindingScript = VueBindings.BuildVueScript();
                VueBindings.UnhandledViewBindingEvent += UnhandledViewBindingEventHandler;
                VueBindings.ViewLoadedEvent += ViewLoadedEventHandler;
                VueBindings.StartListening();

                _viewModel.PropertyChanged += ViewmodelPropertyChangedEventHandler;

                string html = _viewService.GenerateHtml(_viewModel);
                View.HtmlView.LoadHtml(html);
            }
            else
            {
                // Show error message and stop loading the application
                _stopViewModel = new StopViewModel(
                    Ioc.Default.GetService<INavigationService>(),
                    Ioc.Default.GetService<ILanguageService>(),
                    Ioc.Default.GetService<ISvgIconService>(),
                    Ioc.Default.GetService<IThemeService>(),
                    Ioc.Default.GetService<IBaseUrlService>(),
                    Ioc.Default.GetService<IVersionService>(),
                    repositoryService,
                    Ioc.Default.GetService<IFolderPickerService>());

                VueBindings = new VueDataBinding(_stopViewModel, View, null);
                _stopViewModel.VueDataBindingScript = VueBindings.BuildVueScript();
                VueBindings.StartListening();

                string html = _viewStop.GenerateHtml(_stopViewModel);
                View.HtmlView.LoadHtml(html);
            }
        }

        private void ViewmodelPropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Notes")
            {
                // Update the note list in the (HTML) view.
                string html = _viewContentService.GenerateHtml(_viewModel);
                View.HtmlView.ReplaceNode("note-repository", html);
                View.HtmlView.ExecuteJavaScript("makeSortable();");
            }
            else if (e.PropertyName == "ClearFilter")
            {
                // Unfortunately on Android, vue v-model does not report each key press, instead it
                // waits on keyboard composition. Thus we have to handle the binding with v-on:input
                // which does not automatically update the input field when the vue property changes.
                View.HtmlView.ExecuteJavaScript("document.getElementById('Filter').value = '';");
            }
        }

        /// <inheritdoc/>
        protected override void SetHtmlViewBackgroundColor(IHtmlView htmlView)
        {
            htmlView.SetBackgroundColor(ColorExtensions.HexToColor("#323232"));
        }

        private void ViewLoadedEventHandler(object sender, EventArgs e)
        {
            VueBindings.ViewLoadedEvent -= ViewLoadedEventHandler;

            // The content div is excluded from Vue.js interpretation (v-pre), so it would theoretically
            // be faster to replace the notes content directly when building the HTML in ShowInView().
            // By showing the page immediately and delaying the notes until here, loading of the
            // overview feels more responsive though.
            ViewmodelPropertyChangedEventHandler(this, new PropertyChangedEventArgs("Notes"));

            if (!string.IsNullOrEmpty(_scrollToNote))
            {
                string scrollToNoteScript = string.Format("var note = document.querySelector('[data-note=\"{0}\"]'); if (note) note.scrollIntoView();", _scrollToNote);
                View.HtmlView.ExecuteJavaScript(scrollToNoteScript);
            }
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
            View.HtmlView.ExecuteJavaScript(script);
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
                    _viewModel.CheckPinStatusAtPosition(newIndex);
                    break;
            }
        }
    }
}