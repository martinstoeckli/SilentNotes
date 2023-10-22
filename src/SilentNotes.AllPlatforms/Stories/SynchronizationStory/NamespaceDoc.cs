using System;

namespace SilentNotes.Stories.SynchronizationStory
{
    /// <summary>
    /// <para>
    /// The "SynchronizationStory" namespace contains all the necessary steps to do a
    /// synchronization of the notes with the cloud storage. This can include UI interaction (e.g.
    /// to ask for login credentials), passing control to an external browser (for OAuth2
    /// authentication) or running silently in a background task.
    /// </para>
    /// <para>
    /// The possible starting points are:
    /// - The <see cref="IsCloudServiceSetStep"/> for a manually triggered synchronization.
    /// - The <see cref="ShowCloudStorageChoiceStep"/> when triggered from the settings dialog.
    /// - The <see cref="ExistsCloudRepositoryStep"/> for an auto triggered synchronization.
    /// </para>
    /// <para>
    /// Have a look at the implementation of <see cref="SilentNotes.Services.ISynchronizationService"/>
    /// for more information.
    /// </para>
    /// </summary>
    internal static class NamespaceDoc
    {
    }
}
