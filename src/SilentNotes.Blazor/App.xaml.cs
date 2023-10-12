using Microsoft.Maui.Controls;

namespace SilentNotes
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            MainPage = new MainPage();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            Window window = base.CreateWindow(activationState);
            window.Title = "SilentNotes";
#if DEBUG
            window.Title = "SilentNotes - dev";
#endif
            return window;
        }
    }
}