using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Client.ViewModels;

class LoginViewModel : ReactiveObject
{
    private readonly MainViewModel mainViewModel_;
    private readonly HttpClient httpClient_ = new();
    private const string baseURL_ = "https://localhost:7068/api";

    [Reactive] public char PasswordChar { get; set; } = '•';

    [Reactive] public string Login {  get; set; }
    [Reactive] public string Password {  get; set; }
    [Reactive] public bool IsLoading { get; set; } = true;

    public ReactiveCommand<Unit, Unit> GoToRegisterCommand {  get; set; }
    public ReactiveCommand<Unit, Unit> ShowPasswordCommand {  get; set; }
    public ReactiveCommand<Unit, Unit> LoginCommand { get; set; }

    public LoginViewModel(MainViewModel mainVM)
    {
        mainViewModel_ = mainVM;

        GoToRegisterCommand = ReactiveCommand.CreateFromTask(GoToRegister);
        ShowPasswordCommand = ReactiveCommand.Create(ShowPassword);
        LoginCommand = ReactiveCommand.CreateFromTask(TryLogin);
    }

    private async Task GoToRegister()
    {
        await mainViewModel_.NavigateToRegister();
    }

    private void ShowPassword()
    {
        if (PasswordChar == '•')
        {
            PasswordChar = '\0';
        }
        else
        {
            PasswordChar = '•';
        }
    }

    private async Task TryLogin()
    {
        try
        {
            IsLoading = false;

            var response = await httpClient_.PostAsJsonAsync($"{baseURL_}/auth/login", new
            {
                Login,
                Password
            });

            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (authResponse?.Token == null)
            {
                Debug.WriteLine("Неудалось получить токен");
                return;
            }

            Debug.WriteLine($"{authResponse?.Token}");
        }
        catch(HttpRequestException ex) {
            Debug.WriteLine($"Ошибка сети: {ex.Message}");
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Ошибка: {ex.Message}");
        }
        finally
        {
            IsLoading = true;
        }
        
    }

}

public class AuthResponse()
{
    public string Token { get; set; }
}