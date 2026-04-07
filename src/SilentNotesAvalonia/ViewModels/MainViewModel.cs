using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SilentNotes.Services;

namespace SilentNotesAvalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IFeedbackService _feedbackService;
    private readonly IDataProtectionService _dataProtectionService;

    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";

    [ObservableProperty]
    private string _protectedData;

    public MainViewModel(
        IFeedbackService feedbackService,
        IDataProtectionService dataProtectionService)
    {
        _feedbackService = feedbackService;
        _dataProtectionService = dataProtectionService;
        ShowToastCommand = new RelayCommand(ShowToast);
        ProtectDataCommand = new RelayCommand(ProtectData);
    }

    public ICommand ShowToastCommand { get; }

    private void ShowToast()
    {
        _feedbackService.ShowToast("Sugus toast");
    }

    public ICommand ProtectDataCommand { get; }

    private void ProtectData()
    {
        byte[] data = new byte[] { 25, 26, 27 };
        string protectedData = _dataProtectionService.Protect(data);
        ProtectedData = protectedData;
        byte[] unprotectedData = _dataProtectionService.Unprotect(protectedData);
    }
}
