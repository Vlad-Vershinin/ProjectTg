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

namespace Client.ViewModels;

public class ChatsViewModel : ReactiveObject
{
    private readonly MainViewModel _mainViewModel;
    private readonly string _apiUrl = "https://localhost:7068/api";
    private readonly HttpClient httpClient_ = new();
    [Reactive] public Guid currentUserId_ { get; set; }
    private HubConnection hubConnection_;
    [Reactive] public Control SelectedChat { get; set; } = new ContentControl();

    public ObservableCollection<PrivateChat> Chats { get; } = new();

    public ReactiveCommand<Unit, Unit> CreateChatCommand { get; }
    public ReactiveCommand<PrivateChat, Unit> OpenChatCommand { get; }

    public ChatsViewModel(MainViewModel mainVm)
    {
        _mainViewModel = mainVm;

        CreateChatCommand = ReactiveCommand.CreateFromTask(CreateChat);
        OpenChatCommand = ReactiveCommand.Create<PrivateChat>(OpenChat);
        
        currentUserId_ = _mainViewModel.UserId;

        InitializerSignalR();
        LoadChats();
    }

    private void OpenChat(PrivateChat chat)
    {
        var chatViewModel = new ChatViewModel(chat, currentUserId_);
        var chatView = new ChatView { DataContext = chatViewModel };

        SelectedChat = chatView;
    }

    private async Task InitializerSignalR()
    {
        hubConnection_ = new HubConnectionBuilder()
            .WithUrl("https://localhost:7068/chat")
            .Build();

        hubConnection_.On<Message>("ReceiveMessage", message =>
        {
            // Обновляем UI с новым сообщением
        });

        try
        {
            await hubConnection_.StartAsync();
        }
        catch (Exception ex)
        {
            // Обработка ошибок подключения
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

    public async Task SendMessage(Guid chatId, string text)
    {
        try
        {
            var response = await httpClient_.PostAsJsonAsync(
                $"{_apiUrl}/chats/{chatId}/messages",
                new { Text = text, SenderId = currentUserId_ });

            if (!response.IsSuccessStatusCode)
            {
                // Обработка ошибок
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
