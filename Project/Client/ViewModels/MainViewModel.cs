using Avalonia.Controls;
using Client.Views;
using ReactiveUI.Fody.Helpers;
using System;
using System.Threading.Tasks;

namespace Client.ViewModels;

public class MainViewModel : ViewModelBase
{
    [Reactive] public Guid UserId { get; set; }

    [Reactive] public Control? CurrentContent { get; set; }

    private LoginView LoginView { get; set; }
    private RegisterView RegisterView { get; set; }
    private ChatsView ChatsView { get; set; }
    public MainWindow MainWindow { get; set; }

    public MainViewModel(MainWindow mainWindow)
    {
        MainWindow = mainWindow;

        LoginView = new LoginView { DataContext = new LoginViewModel(this) };
        RegisterView = new RegisterView { DataContext = new RegisterViewModel(this) };
        //ChatsView = new ChatsView { DataContext = new ChatsViewModel(this) };

        NavigateToLogin();
    }

    public async Task NavigateToLogin() =>
        CurrentContent = LoginView;

    public async Task NavigateToRegister() => 
        CurrentContent = RegisterView;

    public async Task NavigateToChats() =>
        CurrentContent = new ChatsView { DataContext = new ChatsViewModel(this) };
}
