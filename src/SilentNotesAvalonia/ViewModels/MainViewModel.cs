using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SilentNotes.Services;

namespace SilentNotesAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IFeedbackService _feedbackService;

    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    public MainViewModel(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
        ShowToastCommand = new RelayCommand(ShowToast);
    }

    public ICommand ShowToastCommand { get; }

    private void ShowToast()
    {
        _feedbackService.ShowToast("Sugus toast");
    }
}
