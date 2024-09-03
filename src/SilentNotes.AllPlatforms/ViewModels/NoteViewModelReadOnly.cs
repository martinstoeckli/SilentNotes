// Copyright © 2023 Martin Stoeckli.
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using SilentNotes.Crypto;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// A simplified version of <see cref="NoteViewModel"/> which can be used to display notes
    /// (e.g. in the repository overview), without the full blown edit options.
    /// </summary>
    public class NoteViewModelReadOnly : ViewModelBase
    {
        protected readonly IThemeService _themeService;
        protected readonly ISettingsService _settingsService;
        protected readonly ISafeKeyService _keyService;
        private readonly ICryptor _cryptor;
        private readonly SafeListModel _safes;
        private SearchableHtmlConverter _searchableTextConverter;
        protected string _unlockedContent;
        private string _searchableContent;

        /// <summary>
        /// Initializes a new instance of the <see cref="NoteViewModelReadOnly"/> class.
        /// </summary>
        public NoteViewModelReadOnly(
            NoteModel model,
            SearchableHtmlConverter searchableTextConverter,
            IThemeService themeService,
            ISettingsService settingsService,
            ISafeKeyService safeKeyService,
            ICryptor cryptor,
            SafeListModel safes)
        {
            Model = model ?? NoteModel.NotFound;
            _themeService = themeService;
            _settingsService = settingsService;
            _keyService = safeKeyService;
            _searchableTextConverter = searchableTextConverter;
            _cryptor = cryptor;
            _safes = safes;

            MarkSearchableContentAsDirty();
            _unlockedContent = IsInSafe ? UnlockIfSafeOpen(Model.HtmlContent) : Model.HtmlContent;
        }

        /// <summary>
        /// Gets the unique id of the note.
        /// </summary>
        public Guid Id
        {
            get { return Model.Id; }
        }

        /// <summary>
        /// Gets the type of the note as css class.
        /// </summary>
        public string CssClassNoteType
        {
            get { return Model.NoteType.ToString().ToLowerInvariant(); }
        }

        /// <summary>
        /// Gets a searchable representation of the <see cref="UnlockedHtmlContent"/>. This searchable
        /// text is generated on demand only, to mark it as dirty use <see cref="MarkSearchableContentAsDirty"/>.
        /// </summary>
        public string SearchableContent
        {
            get
            {
                bool searchableContentIsDirty = _searchableContent == null;
                if (searchableContentIsDirty && UnlockedHtmlContent != null)
                {
                    SearchableHtmlConverter converter = _searchableTextConverter ?? (_searchableTextConverter = new SearchableHtmlConverter());
                    converter.TryConvertHtml(UnlockedHtmlContent, out _searchableContent);
                }
                return _searchableContent;
            }
        }

        /// <summary>
        /// Marks the <see cref="SearchableContent"/> as dirty, so it can be recreated the next
        /// time it is used.
        /// </summary>
        protected void MarkSearchableContentAsDirty()
        {
            _searchableContent = null;
        }

        /// <summary>
        /// Gets or sets the Html content of the note.
        /// </summary>
        public string UnlockedHtmlContent
        {
            get { return _unlockedContent; }
        }

        /// <summary>
        /// Gets the <see cref="UnlockedHtmlContent"/>, but very long notes are truncated, so they
        /// can be processed faster by the HTML view in an overview of notes.
        /// </summary>
        /// <returns>Truncated html content of the note.</returns>
        public string GetShortenedUnlockedHtmlContent()
        {
            string result = _unlockedContent;
            if (result != null)
            {
                // Create a short version for large notes, with only the first part of the note.
                // This is a performance improvement if there are large notes in the repository.
                var shortener = new HtmlShortener();
                shortener.WantedLength = 600; // Should be enough even for settings with
                shortener.WantedTagNumber = 20; // small font and very height notes.

                result = shortener.Shorten(result);
                result = shortener.DisableLinks(result);
            }
            return result;
        }

        /// <summary>
        /// Gets a list of tags (labels) associated with this note.
        /// </summary>
        public List<string> Tags
        {
            get { return Model.Tags; }
        }

        /// <summary>
        /// Gets or sets the background color as hex string, e.g. #ff0000
        /// </summary>
        public string BackgroundColorHex
        {
            get
            {
                string result = Model.BackgroundColorHex;
                if (_themeService.IsDarkMode)
                {
                    SettingsModel settings = _settingsService.LoadSettingsOrDefault();
                    if (settings.UseColorForAllNotesInDarkMode)
                        result = settings.ColorForAllNotesInDarkModeHex;
                }
                return result;
            }
        }

        /// <summary>
        /// Gets a class attribute for a given background color, "note-light" for bright background
        /// colors, "note-dark" for dark background colors.
        /// </summary>
        /// <param name="backgroundColorHex">Background color of the note. If this parameter is
        /// null, the <see cref="BackgroundColorHex"/> of the note itself is used.</param>
        /// <returns>Html class "note-dark" if the background color is dark, otherwise "note-light".</returns>
        public string LightOrDarkClass(string backgroundColorHex = null)
        {
            if (string.IsNullOrEmpty(backgroundColorHex))
                backgroundColorHex = BackgroundColorHex;
            System.Drawing.Color backgroundColor = ColorExtensions.HexToColor(backgroundColorHex);
            if (backgroundColor.IsDark())
                return "note-dark";
            else
                return "note-light";
        }

        /// <summary>
        /// Gets or sets a value indicating whether the note is deleted and is part of the
        /// recycling bin.
        /// </summary>
        public bool InRecyclingBin
        {
            get { return Model.InRecyclingBin; }

            set
            {
                if (SetProperty(Model.InRecyclingBin, value, (bool v) => Model.InRecyclingBin = v))
                    Model.RefreshModifiedAt();
            }
        }

        /// <summary>
        /// Gets a value indicating whether the note is in a safe. Notes in a safe can be locked
        /// or unlocked.
        /// </summary>
        public bool IsInSafe
        {
            get { return Model.SafeId != null; }
        }

        /// <summary>
        /// Gets a value indicating whether the note is locked. A locked note is part of a safe and
        /// is not yet decrypted.
        /// </summary>
        public bool IsLocked
        {
            get { return Model.SafeId.HasValue && !_keyService.IsSafeOpen(Model.SafeId.Value); }
        }

        /// <summary>
        /// Decrypts the note, if the belonging safe is open.
        /// </summary>
        /// <returns>Decrypted note content, or null if the safe is closed.</returns>
        protected string UnlockIfSafeOpen(string lockedContent)
        {
            if (_keyService.TryGetKey(Model.SafeId, out byte[] safeKey))
            {
                byte[] binaryContent = CryptoUtils.Base64StringToBytes(lockedContent);
                byte[] unlockedContent = _cryptor.Decrypt(binaryContent, safeKey);
                return CryptoUtils.BytesToString(unlockedContent);
            }
            return null;
        }

        /// <summary>
        /// Gets a value indicating whether the note is unlocked. An unlocked note is part of a safe
        /// and is decrypted.
        /// </summary>
        protected bool IsUnlocked
        {
            get { return IsInSafe && (UnlockedHtmlContent != null); }
        }

        /// <summary>
        /// Encrpyts the unlocked content with the key of the assigned safe.
        /// </summary>
        /// <param name="unlockedContent">Content to encrypt.</param>
        /// <returns>Encrypted content.</returns>
        public string Lock(string unlockedContent)
        {
            string encryptionAlgorithm = _settingsService.LoadSettingsOrDefault().SelectedEncryptionAlgorithm;
            if (_keyService.TryGetKey(Model.SafeId, out byte[] key))
            {
                byte[] binaryContent = CryptoUtils.StringToBytes(unlockedContent);
                byte[] lockedContent = _cryptor.Encrypt(binaryContent, key, encryptionAlgorithm, null);
                return CryptoUtils.BytesToBase64String(lockedContent);
            }
            else
            {
                throw new Exception("Could not find key of the safe.");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the the note is pinned.
        /// <see cref="NoteModel.IsPinned"/>
        /// This property does not call <see cref="NoteModel.RefreshModifiedAt"/>, because it is
        /// not seen as changed content, so switching should not overwrite other recent changes.
        /// </summary>
        public bool IsPinned
        {
            get { return Model.IsPinned; }
            set { SetProperty(Model.IsPinned, value, (v) => Model.IsPinned = v); }
        }

        /// <summary>
        /// Gets the navigation route of the note. In case that the note is locked, the opensafe
        /// route is returned with a target route.
        /// </summary>
        public string Route
        {
            get
            {
                string routeForUnlockedNote = RouteNames.Combine(Model.NoteType.GetRouteName(), Id);
                if (IsLocked)
                    return RouteNames.Combine(RouteNames.OpenSafe, "target", CryptoUtils.StringToBase64String(routeForUnlockedNote));
                else
                    return routeForUnlockedNote;
            }
        }

        /// <summary>
        /// Gets the wrapped model.
        /// </summary>
        internal NoteModel Model { get; private set; }
    }
}
