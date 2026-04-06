using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using AvaloniaCrossTest.Services;
using Microsoft.Extensions.DependencyInjection;
using SilentNotesAvalonia.ViewModels;
using SilentNotesAvalonia.Views;

namespace SilentNotesAvalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>
    /// Gets the service provider for resolving application services.
    /// </summary>
    public IServiceProvider Services { get; private set; }

    /// <summary>
    /// Sets the <see cref="Services"/> property.
    /// </summary>
    /// <remarks>
    /// This method should be called once at the startup of the application, from the platform
    /// specific part of the code. It is necessary because Avalonia requires a parameterless
    /// default constructor the the <see cref="App"/> class on Android.
    /// </remarks>
    /// <param name="services">The service provider to use.</param>
    public void InitializeServices(IServiceProvider services) => Services = services;

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            var mainWindow = new MainWindow();

            var mainWindowProvider = Services.GetRequiredService<IMainWindowProvider>();
            mainWindowProvider.MainWindow = mainWindow;

            var mainViewModel = new MainViewModel();
            mainWindow.DataContext = mainViewModel;
            desktop.MainWindow = mainWindow;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            var mainView = new MainView();
            var mainViewModel = new MainViewModel();
            mainView.DataContext = mainViewModel;
            singleViewPlatform.MainView = mainView;
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}