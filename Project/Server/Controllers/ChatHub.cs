using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Server.Data;
using Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Server.Controllers;

[Authorize]
public class ChatHub : Hub
{
    private readonly DataContext _context;

    public ChatHub(DataContext context)
    {
        _context = context;
    }

    public async Task SendMessage(Guid chatId, string text)
    {
        var userId = Guid.Parse(Context.UserIdentifier);

        var chat = await _context.PrivateChats
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

        var otherUserId = chat.User1Id == userId ? chat.User2Id : chat.User1Id;

        await Clients.Users(userId.ToString(), otherUserId.ToString())
            .SendAsync("ReceiveMessage", message);
    }

    public async Task JoinChat(Guid chatId)
    {
        var userId = Guid.Parse(Context.UserIdentifier);

        var chat = await _context.PrivateChats
            .FirstOrDefaultAsync(pc => pc.Id == chatId &&
                (pc.User1Id == userId || pc.User2Id == userId));

        if (chat != null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());
        }
    }

    public async Task LeaveChat(Guid chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
    }
}