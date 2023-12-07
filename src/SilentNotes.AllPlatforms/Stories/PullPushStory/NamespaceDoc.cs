using System;

namespace SilentNotes.Stories.PullPushStory
{
    /// <summary>
    /// <para>
    /// The "PushPullStory" namespace contains all the necessary steps to do a forced
    /// synchronization of a single note. This story is active only in the mode "manual sync only"
    /// in the settings and is a derivation of the "SynchronizationStory".
    /// </para>
    /// <para>
    /// The possible starting points is:
    /// - The <see cref="SilentNotes.Stories.PullPushStory.ExistsCloudRepositoryStep"/>.
    /// </para>
    /// <para>
    /// This story allows to overwrite a local note with the same note from the online service, or
    /// vive versa, even if the other note has a newer modification date and would normally
    /// overwrite the note.
    /// </para>
    /// </summary>
    internal static class NamespaceDoc
    {
    }
}
