using ReactiveUI;
using System.Reactive;
using System;
using ReactiveUI.Fody.Helpers;
using System.Diagnostics;
using System.Threading.Tasks;
using Client.Models;
using System.Net.Http;
using System.Text.Json;
using Avalonia.Controls;

namespace Client.ViewModels.DialogModels;

public class CreateChatViewModel : ReactiveObject
{
    private readonly ChatsViewModel _parentVm;
    private readonly HttpClient _httpClient = new();
    private readonly string _apiUrl = "https://localhost:7068/api";

    [Reactive] public string Username { get; set; }
    [Reactive] public string GroupName { get; set; }

    public ReactiveCommand<Unit, Unit> CreateCommand { get; }
    public ReactiveCommand<Unit, Unit> CloseCommand { get; }

    public CreateChatViewModel(ChatsViewModel parentVm)
    {
        _parentVm = parentVm;
        CreateCommand = ReactiveCommand.CreateFromTask(CreateChat);
        CloseCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            CloseDialog(true);
        });
    }

    private async Task CreateChat()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Username))
                throw new Exception("Введите логин пользователя");

            var response = await _httpClient.GetAsync($"{_apiUrl}/users/find?Login={Username}");

            if (!response.IsSuccessStatusCode)
                throw new Exception("Пользователь не найден");

            var userJson = await response.Content.ReadAsStringAsync();
            var foundUser = JsonSerializer.Deserialize<User>(userJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            await _parentVm.CreatePrivateChat(
                _parentVm.currentUserId_,
                foundUser.Id            
            );

            CloseDialog(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private void CloseDialog(bool result)
    {
        var view = ViewLocator.Current.ResolveView(this);
        if (view is Window dialogWindow)
        {
            dialogWindow.Close(result);
        }
    }
}