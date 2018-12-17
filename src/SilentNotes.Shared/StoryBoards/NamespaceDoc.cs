using System;

namespace SilentNotes.StoryBoards
{
    /// <summary>
    /// The app can have 0-1 active storyboard which is accessible by the <see cref="IStoryBoardService"/>.
    /// Storyboards describe long-term actions with several steps involved, see the <see cref="IStoryBoard"/>.
    /// <para>
    /// The most important implementation can be found in the <see cref="SynchronizationStoryBoard"/>
    /// class, it handles the synchronization of the notes with the cloud repository and covers all
    /// the many possible ways through the task.
    /// </para>
    /// </summary>
    internal class NamespaceDoc
    {
    }
}
