using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net.Http;
using System.Reactive;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Client.ViewModels;

class RegisterViewModel : ReactiveObject
{
    private readonly MainViewModel mainViewModel_;
    private readonly HttpClient httpClient_ = new();
    private const string baseURL_ = "https://localhost:7068/api";

    [Reactive] public char PasswordChar { get; set; } = '•';

    [Reactive] public string Login { get; set; }
    [EmailAddress]
    [Reactive] public string Email { get; set; }
    [MinLength(8)]
    [Reactive] public string Password { get; set; }

    public ReactiveCommand<Unit, Unit> GoToLoginCommand { get; set; }
    public ReactiveCommand<Unit, Unit> ShowPasswordCommand { get; set; }
    public ReactiveCommand<Unit, Unit> RegisterCommand {  get; set; }

    public RegisterViewModel(MainViewModel mainVM)
    {
        mainViewModel_ = mainVM;

        GoToLoginCommand = ReactiveCommand.CreateFromTask(GoToLogin);
        ShowPasswordCommand = ReactiveCommand.Create(ShowPassword);
        RegisterCommand = ReactiveCommand.CreateFromTask(Register);
    }

    private async Task GoToLogin()
    {
        await mainViewModel_.NavigateToLogin();
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

    private async Task Register()
    {
        try
        {
            Guid id = Guid.NewGuid();
            mainViewModel_.UserId = id;

            var json = JsonSerializer.Serialize(new
            {
                Id = id,
                Login,
                Password,
                Email
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient_.PostAsync($"{baseURL_}/auth/register", content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Статус код: {response.StatusCode}");
                return;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var token = await response.Content.ReadAsStringAsync();

            //var token = JsonSerializer.Deserialize<string>(responseContent, options);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка регистрации: {ex}");
        }
        
    }
}
