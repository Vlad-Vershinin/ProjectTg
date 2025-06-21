using ReactiveUI;
using System.Reactive;
using System;
using ReactiveUI.Fody.Helpers;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Client.ViewModels.DialogModels;

public class CreateChatViewModel : ReactiveObject
{
    private readonly ChatsViewModel _parentVm;

    // Поля для приватного чата
    [Reactive] public string Username { get; set; }

    // Поля для группового чата
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

                // Логика создания приватного чата
                Debug.WriteLine($"Создаем приватный чат с {Username}");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(GroupName))
                    throw new Exception("Введите название группы");

                // Логика создания группового чата
                Debug.WriteLine($"Создаем {(IsPublic ? "публичный" : "приватный")} чат: {GroupName}");
            }

            //this.Close(true); // Закрываем окно с результатом
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Ошибка: {ex.Message}");
            // Можно показать MessageBox с ошибкой
        }
    }
}