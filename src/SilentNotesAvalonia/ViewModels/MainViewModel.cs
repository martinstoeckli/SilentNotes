using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SilentNotes.Services;

namespace SilentNotesAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    public MainViewModel()
    {
        SugusCommand = new RelayCommand(Sugus);
    }

    public ICommand SugusCommand { get; }

    private void Sugus()
    {
    }
}
