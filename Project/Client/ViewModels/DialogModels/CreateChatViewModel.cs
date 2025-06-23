using ReactiveUI;
using System.Reactive;
using System;
using ReactiveUI.Fody.Helpers;
using System.Diagnostics;
using System.Threading.Tasks;
using Client.Models;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia;
using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace Client.ViewModels.DialogModels;

public class CreateChatViewModel : ReactiveObject
{
    private readonly ChatsViewModel _parentVm;
    private readonly HttpClient _httpClient = new();
    private readonly string _apiUrl = "https://localhost:7068/api";

    [Reactive] public string Username { get; set; }
    [Reactive] public string GroupName { get; set; }
    [Reactive] public bool IsPublic { get; set; } = true;
    [Reactive] public bool IsPrivateSelected { get; set; } = true;

    public ReactiveCommand<Unit, Unit> CreateCommand { get; }

    public CreateChatViewModel(ChatsViewModel parentVm)
    {
        _parentVm = parentVm;
        CreateCommand = ReactiveCommand.CreateFromTask(CreateChat);
    }

    private async Task CreateChat()
    {
        try
        {
            if (IsPrivateSelected)
            {
                if (string.IsNullOrWhiteSpace(Username))
                    throw new Exception("Введите логин пользователя");

                // Поиск пользователя по логину
                var response = await _httpClient.GetAsync($"{_apiUrl}/users/find?Login={Username}");

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Пользователь не найден");

                var userJson = await response.Content.ReadAsStringAsync();
                var foundUser = JsonSerializer.Deserialize<User>(userJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Создание приватного чата
                await _parentVm.CreatePrivateChat(
                    _parentVm.currentUserId_, // ID текущего пользователя
                    foundUser.Id            // ID найденного пользователя
                );
            }
            else
            {
                if (string.IsNullOrWhiteSpace(GroupName))
                    throw new Exception("Введите название группы");

                Debug.WriteLine($"Создаем {(IsPublic ? "публичный" : "приватный")} чат: {GroupName}");
            }

            CloseDialog(true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private void CloseDialog(bool result)
    {
        // Закрытие диалога
    }
}