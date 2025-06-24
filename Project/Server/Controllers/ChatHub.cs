using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Server.Data;
using Server.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace Server.Controllers;

[Authorize]
public class ChatHub : Hub
{
    private readonly DataContext _context;
    private static readonly ConcurrentDictionary<Guid, string> _userConnections = new();

    public ChatHub(DataContext context)
    {
        _context = context;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Guid.Parse(Context.UserIdentifier);
        _userConnections[userId] = Context.ConnectionId;

        await SendUpdatedChatList(userId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Guid.Parse(Context.UserIdentifier);
        _userConnections.TryRemove(userId, out _);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(Guid chatId, string text)
    {
        var userId = Guid.Parse(Context.UserIdentifier);
        var user = await _context.Users.FindAsync(userId);

        var chat = await _context.PrivateChats
            .Include(pc => pc.User1)
            .Include(pc => pc.User2)
            .FirstOrDefaultAsync(pc => pc.Id == chatId &&
                (pc.User1Id == userId || pc.User2Id == userId));

        if (chat == null)
        {
            throw new HubException("Чат не найден или нет доступа");
        }

        var message = new Message
        {
            Text = text,
            SentAt = DateTime.UtcNow,
            ChatId = chatId,
            SenderId = userId
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        // Получаем собеседника
        var otherUserId = chat.User1Id == userId ? chat.User2Id : chat.User1Id;

        // Отправляем сообщение обоим пользователям с полной информацией
        await Clients.Users(userId.ToString(), otherUserId.ToString())
            .SendAsync("ReceiveMessage", new
            {
                ChatId = chatId,
                Message = new
                {
                    message.Id,
                    message.Text,
                    message.SentAt,
                    message.ChatId,
                    message.SenderId,
                    Sender = new
                    {
                        user.Id,
                        user.Login,
                        user.Username
                    }
                }
            });

        // Обновляем списки чатов
        await SendUpdatedChatList(userId);
        await SendUpdatedChatList(otherUserId);
    }

    private async Task SendUpdatedChatList(Guid userId)
    {
        var chats = await _context.PrivateChats
            .Include(pc => pc.User1)
            .Include(pc => pc.User2)
            .Include(pc => pc.Messages)
            .Where(pc => pc.User1Id == userId || pc.User2Id == userId)
            .ToListAsync();

        var sortedChats = chats
            .Select(pc => new
            {
                Chat = pc,
                LastMessageDate = pc.Messages.Any()
                    ? pc.Messages.Max(m => m.SentAt)
                    : DateTime.MinValue
            })
            .OrderByDescending(x => x.LastMessageDate)
            .Select(x => new
            {
                x.Chat.Id,
                x.Chat.Name,
                OtherUserId = x.Chat.User1Id == userId ? x.Chat.User2Id : x.Chat.User1Id,
                OtherUserName = x.Chat.User1Id == userId ? x.Chat.User2.Username : x.Chat.User1.Username,
                LastMessage = x.Chat.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault(),
                UnreadCount = x.Chat.Messages.Count(m => m.SenderId != userId && !m.IsRead)
            })
            .ToList();

        if (_userConnections.TryGetValue(userId, out var connectionId))
        {
            await Clients.Client(connectionId).SendAsync("UpdateChatList", sortedChats);
        }
    }

    public async Task MarkAsRead(Guid chatId, Guid messageId)
    {
        var userId = Guid.Parse(Context.UserIdentifier);

        var message = await _context.Messages
            .FirstOrDefaultAsync(m => m.Id == messageId && m.ChatId == chatId);

        if (message != null && message.SenderId != userId)
        {
            message.IsRead = true;
            await _context.SaveChangesAsync();

            await SendUpdatedChatList(userId);
        }
    }
}