using Avalonia.Controls;
using Client.Models;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Text.Json;
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

            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();

                var res = await httpClient_.GetAsync($"{baseURL_}/users/find?Login={Login}");

                if (res.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };
                    var responseContent = await res.Content.ReadAsStringAsync();

                    var user = JsonSerializer.Deserialize<User>(responseContent, options);

                    mainViewModel_.UserId = user.Id;

                    Debug.WriteLine($"{mainViewModel_.UserId}\n\n\n");

                    if (mainViewModel_.UserId == Guid.Empty)
                    {
                        Debug.WriteLine("Id не получен");
                    }

                    await mainViewModel_?.NavigateToChats();
                }
            }

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