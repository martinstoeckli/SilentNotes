using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using SilentNotes.Services;
using SilentNotes.Workers;

namespace SilentNotes.StoryBoards.PullPushStory
{
    /// <summary>
    /// Enumeration of all available step ids of the <see cref="PullPushStoryBoard"/>.
    /// </summary>
    public enum PullPushStoryStepId
    {
        ExistsCloudRepository,
        DownloadCloudRepository,
        DecryptCloudRepository,
        IsSameRepository,
        StoreMergedRepositoryAndQuit,
    }

    /// <summary>
    /// Enumeration of all available session keys of the <see cref="PullPushStoryBoard"/>.
    /// </summary>
    public enum PullPushStorySessionKey
    {
        BinaryCloudRepository,
        CloudRepository,
    }

    /// <summary>
    /// Story for manual synchronization of a single note with the cloud.
    /// This story can only be triggered by the user and has no Gui input.
    /// </summary>
    public class PullPushStoryBoard : StoryBoardBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PullPushStoryBoard"/> class.
        /// </summary>
        public PullPushStoryBoard()
            : base(StoryBoardMode.GuiAndToasts)
        {
            RegisterStep(new ExistsCloudRepositoryStep(
                PullPushStoryStepId.ExistsCloudRepository.ToInt(),
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<IFeedbackService>(),
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<ICloudStorageClientFactory>()));
            RegisterStep(new DownloadCloudRepositoryStep(
                PullPushStoryStepId.DownloadCloudRepository.ToInt(),
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<IFeedbackService>(),
                Ioc.GetOrCreate<ICloudStorageClientFactory>(),
                Ioc.GetOrCreate<ISettingsService>()));
            RegisterStep(new DecryptCloudRepositoryStep(
                PullPushStoryStepId.DecryptCloudRepository.ToInt(),
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<IFeedbackService>(),
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<INoteRepositoryUpdater>()));
            RegisterStep(new IsSameRepositoryStep(
                PullPushStoryStepId.IsSameRepository.ToInt(),
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<IFeedbackService>(),
                Ioc.GetOrCreate<IRepositoryStorageService>()));
            RegisterStep(new StoreMergedRepositoryAndQuitStep(
                PullPushStoryStepId.StoreMergedRepositoryAndQuit.ToInt(),
                this,
                Ioc.GetOrCreate<ILanguageService>(),
                Ioc.GetOrCreate<IFeedbackService>(),
                Ioc.GetOrCreate<ISettingsService>(),
                Ioc.GetOrCreate<ICryptoRandomService>(),
                Ioc.GetOrCreate<IRepositoryStorageService>(),
                Ioc.GetOrCreate<ICloudStorageClientFactory>()));
        }
    }

    /// <summary>Extension methods for the enumeration.</summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Extension methods.")]
    public static class PullPushStoryStepIdExtensions
    {
        /// <summary>Conversion from enum to int.</summary>
        /// <param name="step">The step.</param>
        /// <returns>Integer of the step.</returns>
        [DebuggerStepThrough]
        public static int ToInt(this PullPushStoryStepId step)
        {
            return (int)step;
        }
    }

    /// <summary>Extension methods for the enumeration.</summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Extension methods.")]
    public static class PullPushStorySessionKeyExtensions
    {
        /// <summary>Conversion from enum to int.</summary>
        /// <param name="step">The step.</param>
        /// <returns>Integer of the step.</returns>
        [DebuggerStepThrough]
        public static int ToInt(this PullPushStorySessionKey step)
        {
            return (int)step;
        }
    }
}
