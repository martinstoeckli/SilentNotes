using System;

namespace SilentNotes.HtmlView
{
    /// <summary>
    /// The duplicate finder tries to detect javascript events, which where sent more than once,
    /// and should be ignored. The webview controls do sometimes send duplicates.
    /// </summary>
    public class HtmlViewDuplicateEventFinder
    {
        private readonly TimeSpan _maxTimeSpanForDuplicate;
        private DateTime _lastEventTime;
        private string _lastUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlViewDuplicateEventFinder"/> class.
        /// </summary>
        public HtmlViewDuplicateEventFinder()
        {
            _maxTimeSpanForDuplicate = TimeSpan.FromSeconds(0.1);
        }

        /// <summary>
        /// Checks whether a javascript event from the HtmlView was sent more than once, so the
        /// subsequent events can be ignored.
        /// </summary>
        /// <param name="url">The navigation url of the event.</param>
        /// <returns>Returns true if the event is a duplicate, otherwise false.</returns>
        public bool IsDuplicateEvent(string url)
        {
            if (url == null)
                url = string.Empty;

            DateTime newEventTime = DateTime.Now;
            bool result = IsInsideTimeSpanForDuplicate(_lastEventTime, newEventTime)
                && string.Equals(url, _lastUrl)
                && IsCandidateForDuplicate(url);

            // Remember new event
            _lastEventTime = newEventTime;
            _lastUrl = url;
            return result;
        }

        private bool IsInsideTimeSpanForDuplicate(DateTime lastEventTime, DateTime newEventTime)
        {
            return (newEventTime - _lastEventTime) <= _maxTimeSpanForDuplicate;
        }

        private bool IsCandidateForDuplicate(string url)
        {
            // Exclude keyboard typing events
            return !url.Contains("event-type=text-change") // see NoteRazorView
                && (!url.Contains("event-type=input")); 
        }
    }
}
