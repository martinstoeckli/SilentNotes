using SilentNotes.Crypto;
using SilentNotes.Models;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotes.ViewModels
{
    /// <summary>
    /// This is an optimized viewmodel of a note with truncated content. It should be used as a
    /// readonly representation of the note, but very long notes are truncated, so they can be
    /// processed faster by the HTML view in an overview of notes.
    /// The searchable content is the same though.
    /// </summary>
    public class ShortenedNoteViewModel : NoteViewModel
    {
        /// <inheritdoc/>
        public ShortenedNoteViewModel(
            INavigationService navigationService,
            ILanguageService languageService,
            ISvgIconService svgIconService,
            IThemeService themeService,
            IBaseUrlService webviewBaseUrl,
            SearchableHtmlConverter searchableTextConverter,
            IRepositoryStorageService repositoryService,
            IFeedbackService feedbackService,
            ISettingsService settingsService,
            ICryptor cryptor,
            SafeListModel safes,
            NoteModel noteFromRepository)
            : base(navigationService, languageService, svgIconService, themeService, webviewBaseUrl, searchableTextConverter, repositoryService, feedbackService, settingsService, cryptor, safes, noteFromRepository)
        {
            if (_unlockedContent != null)
            {
                // Create a short version for large notes, with only the first part of the note.
                // This is a performance improvement if there are large notes in the repository.
                HtmlShortener shortener = new HtmlShortener();
                shortener.WantedLength = 600; // Should be enough even for settings with
                shortener.WantedTagNumber = 20; // small font and very height notes.

                string shortenedContent = shortener.Shorten(_unlockedContent);
                if (shortenedContent.Length != _unlockedContent.Length)
                {
                    // Because the note will be truncated, we have to build the searchable content
                    // first, before overwriting the original content.
                    string dummy = SearchableContent;
                    _unlockedContent = shortenedContent;
                }
            }
        }
    }
}
