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
using Microsoft.AspNet.SignalR.Client.Http;
using System.Text.Json;

namespace Client.ViewModels;

public class ChatViewModel : ReactiveObject
{
    private readonly PrivateChat _chat;
    private readonly HttpClient _httpClient = new();
    private readonly string _apiUrl = "https://localhost:7068/api";
    private Guid _currentUserId;

    [Reactive] public string ChatName { get; set; }
    [Reactive] public ObservableCollection<Message> Messages { get; set; } = new();
    [Reactive] public string NewMessageText { get; set; }

    public ReactiveCommand<Unit, Unit> SendMessageCommand { get; }

    public ChatViewModel(PrivateChat chat, Guid userId)
    {
        _chat = chat;
        ChatName = chat.Name;

        _currentUserId = userId;

        SendMessageCommand = ReactiveCommand.CreateFromTask(SendMessage);
        LoadMessages();
    }
    
    private async Task LoadMessages()
    {
        try
        {
            var messages = await _httpClient.GetFromJsonAsync<Message[]>(
               $"{_apiUrl}/messages/{_chat.Id}");

            Debug.WriteLine(_currentUserId);
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

                Debug.WriteLine($"Логин отправителя: {message.Sender.Login}");
            }
            Debug.WriteLine(Messages.Count);
        }
        catch (Exception ex)
        {
            // Обработка ошибок
        }
    }

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(NewMessageText))
            return;

        await _httpClient.PostAsJsonAsync(
            $"{_apiUrl}/chats/{_chat.Id}/messages",
            new { Text = NewMessageText, SenderId = _currentUserId });
        NewMessageText = string.Empty;
        await LoadMessages(); // Обновляем сообщения
    }
}