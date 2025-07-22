using Microsoft.Maui.Controls;

namespace SilentNotes
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var mainPage = new MainPage();
            return new Window(mainPage) { Title = "SilentNotes" };
        }
    }
}