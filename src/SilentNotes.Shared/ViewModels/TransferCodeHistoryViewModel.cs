// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Collections.Generic;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SilentNotes.Controllers;
using SilentNotes.HtmlView;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the current transfer code to the user.
    /// </summary>
    public class TransferCodeHistoryViewModel : ViewModelBase
    {
        private bool _showTransfercodeHistoryVisible;
        private bool _showTransfercodeHistory;
        private List<string> _transferCodeHistory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransferCodeHistoryViewModel"/> class.
        /// </summary>
        public TransferCodeHistoryViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IThemeService themeService,
            IBaseUrlService webviewBaseUrl,
            ISettingsService settingsService)
            : base(navigationService, languageService, svgIconService, themeService, webviewBaseUrl)
        {
            Model = settingsService.LoadSettingsOrDefault();

            ShowTransfercodeHistoryVisible = TransferCodeHistory.Count > 0;
            TransfercodeHistoryVisible = false;

            // Initialize commands
            GoBackCommand = new RelayCommand(GoBack);
            ShowTransfercodeHistoryCommand = new RelayCommand(ShowTransfercodeHistory);
        }

        /// <summary>
        /// Gets the formatted transfer code.
        /// </summary>
        public string TransferCodeFmt
        {
            get { return TransferCode.FormatTransferCodeForDisplay(Model.TransferCode); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the show history button is visible.
        /// </summary>
        [VueDataBinding(VueBindingMode.OneWayToView)]
        public bool ShowTransfercodeHistoryVisible
        {
            get { return _showTransfercodeHistoryVisible; }
            set { SetProperty(ref _showTransfercodeHistoryVisible, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the transfercode history is visible.
        /// </summary>
        [VueDataBinding(VueBindingMode.OneWayToView)]
        public bool TransfercodeHistoryVisible
        {
            get { return _showTransfercodeHistory; }
            set { SetProperty(ref _showTransfercodeHistory, value); }
        }

        /// <summary>
        /// Gets the history of old transfer codes.
        /// </summary>
        public List<string> TransferCodeHistory
        {
            get
            {
                if (_transferCodeHistory == null)
                {
                    _transferCodeHistory = new List<string>();
                    foreach (string transferCode in Model.TransferCodeHistory)
                    {
                        if (transferCode != Model.TransferCode)
                            _transferCodeHistory.Add(TransferCode.FormatTransferCodeForDisplay(transferCode));
                    }
                }
                return _transferCodeHistory;
            }
        }

        /// <summary>
        /// Gets the command to go back to the note overview.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand GoBackCommand { get; private set; }

        private void GoBack()
        {
            _navigationService.Navigate(new Navigation(ControllerNames.NoteRepository));
        }

        /// <inheritdoc/>
        public override void OnGoBackPressed(out bool handled)
        {
            handled = true;
            GoBack();
        }

        /// <summary>
        /// Gets the command to show the history.
        /// </summary>
        [VueDataBinding(VueBindingMode.Command)]
        public ICommand ShowTransfercodeHistoryCommand { get; private set; }

        private void ShowTransfercodeHistory()
        {
            TransfercodeHistoryVisible = true;
        }

        /// <summary>
        /// Gets the wrapped model.
        /// </summary>
        internal SettingsModel Model { get; private set; }
    }
}
