using CommunityToolkit.Mvvm.Messaging;
using SilentNotes.Services;

namespace SilentNotes;

public partial class App : Microsoft.Maui.Controls.Application
{
    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    /// <param name="serviceProvider">Sets the <see cref="Ioc"/> property.</param>
	public App(IServiceProvider serviceProvider)
	{
		InitializeComponent();
        Ioc.Instance.Initialize(serviceProvider);
        MainPage = new MainPage();

#if ANDROID
        // Workaround: Android soft keyboard hides the lower part of the content
        Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.Application.UseWindowSoftInputModeAdjust(
            Current.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>(),
            Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.WindowSoftInputModeAdjust.Resize);
#endif
    }

#if WINDOWS
    protected override Window CreateWindow(IActivationState activationState)
    {
        var window = base.CreateWindow(activationState);
        window.Destroying += OnDestroying;
        return window;
    }

    private void OnDestroying(object sender, EventArgs e)
    {
        WeakReferenceMessenger.Default.Send<StoreUnsavedDataMessage>(new StoreUnsavedDataMessage());

        // Start auto synchronization
        IAutoSynchronizationService syncService = Ioc.Instance.GetService<IAutoSynchronizationService>();
        syncService.SynchronizeAtShutdown().GetAwaiter().GetResult();
    }
#endif
}
