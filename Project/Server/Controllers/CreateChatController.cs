using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models.Dtos;
using System.Collections.ObjectModel;
using Server.Models;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreateChatController : ControllerBase
{
    private readonly DataContext _db;

    public CreateChatController(DataContext db)
    {
        _db = db;
    }

    [HttpPost("private")]
    public async Task<IActionResult> CreatePrivateChat([FromBody] CreatePrivateChatDto chatDto)
    {
        var user1 = await _db.Users.FindAsync(chatDto.User1Id);
        var user2 = await _db.Users.FindAsync(chatDto.User2Id);

        if (user1 == null || user2 == null)
        {
            return BadRequest("Один из пользователей не найден");
        }

        var existingChat = await _db.PrivateChats
            .FirstOrDefaultAsync(pc =>
                (pc.User1Id == chatDto.User1Id && pc.User2Id == chatDto.User2Id) ||
                (pc.User1Id == chatDto.User2Id && pc.User2Id == chatDto.User1Id));

        if (existingChat != null)
        {
            return BadRequest("Чат между этими пользователями уже существует");
        }

        var newChat = new PrivateChat
        {
            Id = Guid.NewGuid(),
            User1Id = chatDto.User1Id,
            User2Id = chatDto.User2Id,
            Name = $"{user1.Username} и {user2.Username}",
            Messages = new ObservableCollection<Message>()
        };

        _db.PrivateChats.Add(newChat);
        await _db.SaveChangesAsync();

        return Ok(newChat);
    }
}