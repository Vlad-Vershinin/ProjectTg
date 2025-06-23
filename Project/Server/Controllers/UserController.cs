using Microsoft.AspNetCore.Mvc;
using Server.Data;
using Microsoft.EntityFrameworkCore;
using Server.Models;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly DataContext _db;

    public UsersController(DataContext db)
    {
        _db = db;
    }

    [HttpGet("find")]
    public async Task<IActionResult> FindUserByUsername(string Login)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Login == Login);

        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpGet("messages/{id}")]
    public async Task<ActionResult<User>> GetUser(Guid id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }
}