using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Models;

namespace Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class MessagesController : ControllerBase
{
    private readonly DataContext _context;

    public MessagesController(DataContext context)
    {
        _context = context;
    }

    [HttpGet("{chatId}")]
    public async Task<ActionResult<IEnumerable<Message>>> GetMessages(Guid chatId)
    {
        var messages = await _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        return Ok(messages);
    }
}