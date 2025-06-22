using Microsoft.AspNetCore.Mvc;
using Server.Data;
using Server.Models.Dtos;
using Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ChatsController : ControllerBase
{
    private readonly DataContext _context;

    public ChatsController(DataContext context)
    {
        _context = context;
    }

    [HttpPost("private")]
    public async Task<ActionResult<PrivateChat>> CreatePrivateChat([FromBody] CreatePrivateChatDto dto)
    {
        // Проверяем существование пользователей
        var user1 = await _context.Users.FindAsync(dto.User1Id);
        var user2 = await _context.Users.FindAsync(dto.User2Id);

        if (user1 == null || user2 == null)
        {
            return NotFound("Один из пользователей не найден");
        }

        // Проверяем, не существует ли уже чат между этими пользователями
        var existingChat = await _context.PrivateChats
            .FirstOrDefaultAsync(pc =>
                (pc.User1Id == dto.User1Id && pc.User2Id == dto.User2Id) ||
                (pc.User1Id == dto.User2Id && pc.User2Id == dto.User1Id));

        if (existingChat != null)
        {
            return Ok(existingChat); // Возвращаем существующий чат
        }

        // Создаем новый чат
        var chat = new PrivateChat
        {
            Id = dto.Id,
            User1Id = dto.User1Id,
            User2Id = dto.User2Id,
            Name = $"{user1.Username} и {user2.Username}"
        };

        _context.PrivateChats.Add(chat);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetChat), new { id = chat.Id }, chat);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PrivateChat>> GetChat(Guid id)
    {
        var chat = await _context.PrivateChats
            .Include(pc => pc.User1)
            .Include(pc => pc.User2)
            .Include(pc => pc.Messages)
                .ThenInclude(m => m.Sender)
            .FirstOrDefaultAsync(pc => pc.Id == id);

        if (chat == null)
        {
            return NotFound();
        }

        return Ok(chat);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<PrivateChat>>> GetUserChats(Guid userId)
    {
        var chats = await _context.PrivateChats
            .Include(pc => pc.User1)
            .Include(pc => pc.User2)
            .Include(pc => pc.Messages)
            .Where(pc => pc.User1Id == userId || pc.User2Id == userId)
            .ToListAsync();

        // Сортировка в памяти
        var sortedChats = chats
            .Select(pc => new
            {
                Chat = pc,
                LastMessageDate = pc.Messages.Any()
                    ? pc.Messages.Max(m => m.SentAt)
                    : DateTime.MinValue
            })
            .OrderByDescending(x => x.LastMessageDate)
            .Select(x => x.Chat)
            .ToList();

        return Ok(sortedChats);
    }

    [HttpPost("{chatId}/messages")]
    public async Task<ActionResult<Message>> AddMessage(Guid chatId, [FromBody] MessageDto dto)
    {
        var chat = await _context.PrivateChats.FindAsync(chatId);
        if (chat == null)
        {
            return NotFound("Чат не найден");
        }

        var user = await _context.Users.FindAsync(dto.SenderId);
        if (user == null)
        {
            return NotFound("Пользователь не найден");
        }

        var message = new Message
        {
            Text = dto.Text,
            SentAt = DateTime.UtcNow,
            ChatId = chatId,
            SenderId = dto.SenderId
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetMessage), new { id = message.Id }, message);
    }

    [HttpGet("messages/{id}")]
    public async Task<ActionResult<Message>> GetMessage(Guid id)
    {
        var message = await _context.Messages
            .Include(m => m.Sender)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (message == null)
        {
            return NotFound();
        }

        return Ok(message);
    }
}

public class MessageDto
{
    public string Text { get; set; }
    public Guid SenderId { get; set; }
}