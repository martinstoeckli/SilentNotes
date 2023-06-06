// Copyright © 2018 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using SilentNotes.Models;
using SilentNotes.Workers;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// View model to present the current transfer code to the user.
    /// </summary>
    public class TransferCodeHistoryViewModel : ViewModelBase
    {
        private List<string> _transferCodeHistory;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransferCodeHistoryViewModel"/> class.
        /// </summary>
        public TransferCodeHistoryViewModel(
            SettingsModel model)
        {
            Model = model;
            IsTransfercodeHistoryVisible = TransferCodeHistory.Count > 0;

            // Initialize commands
            CopyTransferCodeCommand = new AsyncRelayCommand(CopyTransferCode);
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
        public bool IsTransfercodeHistoryVisible { get; }

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
        /// Gets the command to copy the active transfer code.
        /// </summary>
        public ICommand CopyTransferCodeCommand { get; private set; }

        private async Task CopyTransferCode()
        {
            await Clipboard.SetTextAsync(TransferCodeFmt);
        }

        /// <summary>
        /// Gets the wrapped model.
        /// </summary>
        internal SettingsModel Model { get; }
    }
}
