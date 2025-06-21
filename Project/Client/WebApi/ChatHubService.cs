using Microsoft.AspNetCore.SignalR;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Threading.Tasks;
using System;
using Client.Models;
//using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNetCore.SignalR.Client;

namespace Client.WebApi;

public class ChatHubService : ReactiveObject
{
    private readonly string _hubUrl;
    private HubConnection _hubConnection;

    [Reactive] public bool IsConnected { get; private set; }
    [Reactive] public string ConnectionStatus { get; private set; } = "Disconnected";

    public event Action<Message> MessageReceived;
    public event Action<PrivateChat> ChatCreated;

    public ChatHubService(string hubUrl)
    {
        _hubUrl = hubUrl;
    }

    public async Task InitializeAsync(string hubUrl, string accessToken = null)
    {
        try
        {
            var builder = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        options.AccessTokenProvider = () => Task.FromResult(accessToken);
                    }
                })
                .WithAutomaticReconnect();

            _hubConnection = builder.Build();

            // Настройка обработчиков сообщений...
            await _hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR connection error: {ex.Message}");
        }
    }

    private void SetupHubHandlers()
    {
        _hubConnection.On<Message>("ReceiveMessage", message =>
        {
            MessageReceived?.Invoke(message);
        });

        _hubConnection.On<PrivateChat>("ChatCreated", chat =>
        {
            ChatCreated?.Invoke(chat);
        });

        _hubConnection.Reconnected += connectionId =>
        {
            ConnectionStatus = "Reconnected";
            IsConnected = true;
            return Task.CompletedTask;
        };

        _hubConnection.Reconnecting += error =>
        {
            ConnectionStatus = "Reconnecting...";
            IsConnected = false;
            return Task.CompletedTask;
        };

        _hubConnection.Closed += error =>
        {
            ConnectionStatus = "Disconnected";
            IsConnected = false;
            return Task.CompletedTask;
        };
    }

    public async Task StartConnectionAsync()
    {
        if (_hubConnection == null || _hubConnection.State == HubConnectionState.Connected)
            return;

        try
        {
            ConnectionStatus = "Connecting...";
            await _hubConnection.StartAsync();
            ConnectionStatus = "Connected";
            IsConnected = true;
        }
        catch (Exception ex)
        {
            ConnectionStatus = $"Connection error: {ex.Message}";
        }
    }

    public async Task StopConnectionAsync()
    {
        if (_hubConnection == null)
            return;

        try
        {
            await _hubConnection.StopAsync();
            ConnectionStatus = "Disconnected";
            IsConnected = false;
        }
        catch (Exception ex)
        {
            ConnectionStatus = $"Disconnection error: {ex.Message}";
        }
    }

    public async Task JoinChat(Guid chatId)
    {
        if (!IsConnected) return;

        try
        {
            await _hubConnection.InvokeAsync("JoinChat", chatId);
        }
        catch (Exception ex)
        {
            // Обработка ошибок
        }
    }

    public async Task LeaveChat(Guid chatId)
    {
        if (!IsConnected) return;

        try
        {
            await _hubConnection.InvokeAsync("LeaveChat", chatId);
        }
        catch (Exception ex)
        {
            // Обработка ошибок
        }
    }

    public async Task SendMessage(Guid chatId, string text)
    {
        if (!IsConnected) return;

        try
        {
            await _hubConnection.InvokeAsync("SendMessage", chatId, text);
        }
        catch (Exception ex)
        {
            // Обработка ошибок
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}