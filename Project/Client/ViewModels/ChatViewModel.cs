using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using Client.Models;
using System.Net.Http.Json;
using System.Diagnostics;
using System;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;
using System.Collections.Generic;
using System.Linq;

namespace Client.ViewModels;

public class ChatViewModel : ReactiveObject
{
    public readonly PrivateChat _chat;
    private readonly HttpClient _httpClient = new();
    private readonly string _apiUrl = "https://localhost:7068/api";
    private Guid _currentUserId;
    private readonly HubConnection _hubConnection;

    [Reactive] public string ChatName { get; set; }
    [Reactive] public ObservableCollection<Message> Messages { get; set; } = new();
    [Reactive] public string NewMessageText { get; set; } = string.Empty;

    public ReactiveCommand<Unit, Unit> SendMessageCommand { get; }

    public ChatViewModel(PrivateChat chat, Guid userId, HubConnection hubConnection)
    {
        _chat = chat;
        ChatName = chat.Name;
        _currentUserId = userId;
        _hubConnection = hubConnection;

        _hubConnection.On<Guid, Message>("ReceiveMessage", (receivedChatId, message) =>
        {
            if (receivedChatId == _chat.Id)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    if (!Messages.Any(m => m.Id == message.Id))
                    {
                        Messages.Add(message);
                    }
                });
            }
        });

        SendMessageCommand = ReactiveCommand.CreateFromTask(SendMessage);
        //LoadMessages();
    }

    private void InitializeHubConnection()
    {
        _hubConnection.StartAsync().ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                Debug.WriteLine("Ошибка подключения к Hub");
            }
        });

        _hubConnection.Reconnected += _ => Task.Run(LoadMessages);
    }

    public async Task LoadMessages()
    {
        try
        {
            var messages = await _httpClient.GetFromJsonAsync<Message[]>(
               $"{_apiUrl}/messages/{_chat.Id}");

            Messages.Clear();
            foreach (var message in messages)
            {
                if (message.Sender == null)
                {
                    var Sender = await _httpClient.GetAsync($"{_apiUrl}/users/finduser?Id={message.SenderId}");
                    var senderResponse = await Sender.Content.ReadAsStringAsync();
                    var user = JsonSerializer.Deserialize<User>(senderResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    message.Sender = user;
                }

                Messages.Add(message);
            }
        }
        catch (Exception ex)
        {
            // Обработка ошибок
        }
    }

    private async Task SendMessage()
    {
        try
        {
            if (_hubConnection.State != HubConnectionState.Connected)
            {
                await _hubConnection.StartAsync();

                if (_hubConnection.State != HubConnectionState.Connected)
                {
                    throw new Exception("SignalR connection is not active");
                }
            }

            await _hubConnection.InvokeAsync("SendMessage", _chat.Id, NewMessageText);
            NewMessageText = string.Empty;
        }
        catch (Exception ex)
        {
            await _httpClient.PostAsJsonAsync(
                $"{_apiUrl}/chats/{_chat.Id}/messages",
                new { Text = NewMessageText, SenderId = _currentUserId });
            await LoadMessages();
        }
    }

    public void Dispose()
    {
        _hubConnection?.DisposeAsync();
    }
}