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
    public ReactiveCommand<Unit, Unit> OpenProfileCommand { get; set; }
    public ReactiveCommand<Unit, Unit> LeaveCommand { get; set; }
    public ReactiveCommand<PrivateChat, Unit> OpenChatCommand { get; set; }

    public ChatsViewModel(MainViewModel mainVm)
    {
        _mainViewModel = mainVm;

        CreateChatCommand = ReactiveCommand.CreateFromTask(CreateChat);
        OpenChatCommand = ReactiveCommand.Create<PrivateChat>(OpenChat);
        OpenProfileCommand = ReactiveCommand.CreateFromTask(OpenProfile);
        LeaveCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            await hubConnection_.StopAsync();
            await hubConnection_.DisposeAsync();
            hubConnection_ = null;
            await mainVm.NavigateToLogin();
        });
        currentUserId_ = _mainViewModel.UserId;

        InitializerSignalR();
    }

    private void OpenChat(PrivateChat chat)
    {
        var chatViewModel = new ChatViewModel(chat, currentUserId_, hubConnection_);
        var chatView = new ChatView { DataContext = chatViewModel };

        SelectedChat = chatView;
        chatViewModel.LoadMessages().ConfigureAwait(false);
    }

    private async Task OpenProfile()
    {
        var progileVM = new ProfileWindowViewModel(currentUserId_);
        var profileView = new ProfileWIndowView();
        profileView.DataContext = progileVM;

        var result = await profileView.ShowDialog<bool>(GetMainWindow());
    }

    private async Task InitializerSignalR()
    {
        if (_mainViewModel.JwtToken == null || string.IsNullOrEmpty(_mainViewModel.JwtToken))
        {
            throw new Exception("Jwt токен пустой");
        }

        hubConnection_ = new HubConnectionBuilder().WithUrl("https://localhost:7068/chat", options =>
        {
            options.SkipNegotiation = true;
            options.Transports = HttpTransportType.WebSockets;
            options.AccessTokenProvider = () => Task.FromResult(_mainViewModel?.JwtToken);
        })
            .ConfigureLogging(log => log.AddConsole())
            .WithAutomaticReconnect()
            .Build();

        hubConnection_.Reconnected += (connectionId) =>
        {
            return Task.CompletedTask;
        };

        hubConnection_.On<IEnumerable<object>>("UpdateChatList", (chatsObj) =>
        {
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
            Debug.WriteLine($"SignalR connection error: {ex.Message}");
        }
    }

    private async Task CreateChat()
    {
        var viewModel = new CreateChatViewModel(this);
        var dialogView = new CreateChatView();
        dialogView.DataContext = viewModel;

        var result = await dialogView.ShowDialog<bool>(GetMainWindow());
        if (result)
        {
            await LoadChats();
        }
    }

    public async Task LoadChats()
    {
        try
        {
            var chats = await httpClient_.GetFromJsonAsync<PrivateChat[]>(
                $"{_apiUrl}/chats/user/{currentUserId_}");

            Chats.Clear();
            foreach (var chat in chats)
            {
                chat.CurrentUserId = currentUserId_;
                var otherUserId = chat.User1Id == currentUserId_ ? chat.User2Id : chat.User1Id;
                chat.OtherUserAvatarUrl = $"{_apiUrl}/users/getuser/{otherUserId}";
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
