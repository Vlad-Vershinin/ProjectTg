using Client.Models;
using Client.Views.DialogViews;
using Client.ViewModels.DialogModels;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using System.Net.Http;
using System;
using System.Net.Http.Json;
using Avalonia;
using Microsoft.AspNetCore.SignalR.Client;
using ReactiveUI.Fody.Helpers;
using Client.Views;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Logging;

namespace Client.ViewModels;

public class ChatsViewModel : ReactiveObject
{
    private readonly MainViewModel _mainViewModel;
    private readonly string _apiUrl = "https://localhost:7068/api";
    private readonly HttpClient httpClient_ = new();
    [Reactive] public Guid currentUserId_ { get; set; }
    private HubConnection hubConnection_;
    [Reactive] public Control SelectedChat { get; set; } = new ContentControl();

    [Reactive] public ObservableCollection<PrivateChat> Chats { get; set; } = new();

    public ReactiveCommand<Unit, Unit> CreateChatCommand { get; set; }
    public ReactiveCommand<PrivateChat, Unit> OpenChatCommand { get; set; }

    public ChatsViewModel(MainViewModel mainVm)
    {
        _mainViewModel = mainVm;

        CreateChatCommand = ReactiveCommand.CreateFromTask(CreateChat);
        OpenChatCommand = ReactiveCommand.Create<PrivateChat>(OpenChat);
        
        currentUserId_ = _mainViewModel.UserId;

        InitializerSignalR();
        //LoadChats();
    }

    private void OpenChat(PrivateChat chat)
    {
        var chatViewModel = new ChatViewModel(chat, currentUserId_, hubConnection_);
        var chatView = new ChatView { DataContext = chatViewModel };

        SelectedChat = chatView;
        chatViewModel.LoadMessages().ConfigureAwait(false);
    }

    private async Task InitializerSignalR()
    {
        if (_mainViewModel.JwtToken == null || string.IsNullOrEmpty(_mainViewModel.JwtToken))
        {
            throw new Exception("Jwt токен пустой");
        }

        //hubConnection_ = new HubConnectionBuilder()
        //    .WithUrl("https://localhost:7068/chat", options =>
        //    {
        //        options.AccessTokenProvider = () => Task.FromResult(_mainViewModel.JwtToken);
        //    })
        //    .WithAutomaticReconnect(new[] {
        //        TimeSpan.Zero, // Первая попытка сразу
        //        TimeSpan.FromSeconds(1),
        //        TimeSpan.FromSeconds(5),
        //        TimeSpan.FromSeconds(10)
        //    })
        //    .Build();

        hubConnection_ = new HubConnectionBuilder().WithUrl("https://localhost:7068/chat", options =>
        {
            options.SkipNegotiation = true;
            options.Transports = HttpTransportType.WebSockets;
            options.AccessTokenProvider = () => Task.FromResult(_mainViewModel?.JwtToken);
        })
            .ConfigureLogging(log => log.AddConsole())
            .WithAutomaticReconnect()
            .Build();

        // Обработчики событий
        hubConnection_.Closed += async (error) =>
        {
            Debug.WriteLine($"Соединение закрыто: {error?.Message}");
            await Task.Delay(1000);
            await hubConnection_.StartAsync();
        };

        hubConnection_.Reconnected += (connectionId) =>
        {
            Debug.WriteLine($"Соединение восстановлено: {connectionId}");
            return Task.CompletedTask;
        };

        hubConnection_.On<Guid, Message>("ReceiveMessage", (chatId, message) =>
        {
            var chat = Chats.FirstOrDefault(c => c.Id == chatId);
            if (chat != null)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    // Если чат открыт - добавляем сообщение
                    if (SelectedChat is ChatView chatView &&
                        chatView.DataContext is ChatViewModel chatVm &&
                        chatVm._chat.Id == chatId)
                    {
                        chatVm.Messages.Add(message);
                    }
                    // Обновляем список чатов
                    LoadChats().ConfigureAwait(false);
                });
            }
        });

        hubConnection_.On<IEnumerable<object>>("UpdateChatList", (chatsObj) =>
        {
            // Обновляем список чатов при изменениях
            Debug.WriteLine($"=-=-=-=-=-=-=-=-=-=-=-=-=--=-=-=-=-==-=--=-=");
            Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
            {
                await LoadChats();
            });
        });

        try
        {
            await hubConnection_.StartAsync();
        }
        catch (Exception ex)
        {
            // Обработка ошибок подключения
            Debug.WriteLine($"SignalR connection error: {ex.Message}");
        }
    }

    private async Task CreateChat()
    {
        //currentUserId_ = _mainViewModel.UserId;

        var dialogView = new CreateChatView();
        var viewModel = new CreateChatViewModel(this);
        dialogView.DataContext = viewModel;

        var result = await dialogView.ShowDialog<bool>(GetMainWindow());
        if (result)
        {
            await LoadChats();
        }
    }

    private async Task LoadChats()
    {
        try
        {
            var chats = await httpClient_.GetFromJsonAsync<PrivateChat[]>(
                $"{_apiUrl}/chats/user/{currentUserId_}");

            Chats.Clear();
            foreach (var chat in chats)
            {
                Chats.Add(chat);
            }
        }
        catch (Exception ex)
        {
            // Обработка ошибок
        }
    }

    public async Task CreatePrivateChat(Guid user1Id, Guid user2Id)
    {
        try
        {
            var response = await httpClient_.PostAsJsonAsync(
                $"{_apiUrl}/chats/private",
                new CreatePrivateChatDto { Id = Guid.NewGuid(), User1Id = currentUserId_, User2Id = user2Id });

            if (response.IsSuccessStatusCode)
            {
                var newChat = await response.Content.ReadFromJsonAsync<PrivateChat>();
                Chats.Add(newChat);
            }
        }
        catch (Exception ex)
        {
            // Обработка ошибок
        }
    }

    private Window GetMainWindow()
    {
        return (Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow;
    }
}
