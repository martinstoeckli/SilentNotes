using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MudBlazor;
using SilentNotes.Services;

namespace SilentNotesAvalonia.Services
{
    public class FeedbackService : IFeedbackService
    {
        public Task<MessageBoxResult> ShowMessageAsync(string message, string title, MessageBoxButtons buttons, bool conservativeDefault)
        {
            return Task.FromResult(MessageBoxResult.Ok);
        }

        public void ShowToast(string message, Severity severity = Severity.Normal)
        {
        }
    }
}
