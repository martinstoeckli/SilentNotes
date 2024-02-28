using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;

namespace SilentNotes
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            try
            {
                InitializeComponent();
                MainPage = new MainPage();
            }
            catch (Exception ex)
            {
                Ioc.Instance.GetService<ILogger>()?.LogError(ex, null, null);
                throw;
            }
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            try
            {
                Window window = base.CreateWindow(activationState);
                window.Title = "SilentNotes";
                return window;
            }
            catch (Exception ex)
            {
                Ioc.Instance.GetService<ILogger>()?.LogError(ex, null, null);
                throw;
            }
        }
    }
}