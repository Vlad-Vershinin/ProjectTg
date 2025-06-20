using Avalonia.Controls;
using Client.Views;
using ReactiveUI.Fody.Helpers;
using System.Threading.Tasks;

namespace Client.ViewModels;

public class MainViewModel : ViewModelBase
{
    [Reactive] public Control? CurrentContent { get; set; }

    private LoginView LoginView { get; set; }
    private RegisterView RegisterView { get; set; }

    public MainViewModel()
    {
        LoginView = new LoginView { DataContext = new LoginViewModel(this) };
        RegisterView = new RegisterView { DataContext = new RegisterViewModel(this) };

        NavigateToLogin();
    }

    public async Task NavigateToLogin() =>
        CurrentContent = LoginView;

    public async Task NavigateToRegister() => 
        CurrentContent = RegisterView;
}
