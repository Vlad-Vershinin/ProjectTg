using Microsoft.AspNetCore.Mvc;
using Server.Data;
using Microsoft.EntityFrameworkCore;
using Server.Models;
using Microsoft.AspNetCore.Http;

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

    [HttpGet("finduser")]
    public async Task<ActionResult<User>> GetUser(string Id)
    {
        var id = Guid.Parse(Id);
        var user = await _db.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("getuser/{userId}")]
    public async Task<IActionResult> GetUserWithAvatar(Guid userId)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Select(u => new
            {
                u.Id,
                u.Login,
                u.UserName,
                u.Email,
                u.Description,
                AvatarUrl = !string.IsNullOrEmpty(u.ImagePath)
                    ? $"{Request.Scheme}://{Request.Host}/{u.ImagePath.TrimStart('/')}"
                    : $"{Request.Scheme}://{Request.Host}/Assets/profile.png"
            })
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound("Пользователь не найден");
        }

        return Ok(user);
    }

    [HttpPost("saveuser")]
    public async Task<IActionResult> UpdateUser([FromForm] UserUpdateDto userDto)
    {
        var user = await _db.Users.FindAsync(userDto.Id);
        if (user == null)
            return NotFound();

        // Обновляем основные данные
        user.UserName = userDto.UserName;
        user.Description = userDto.Description;
        user.Email = userDto.Email;

        // Обрабатываем аватар, если он был загружен
        if (userDto.Image != null)
        {
            var uploadsFolder = Path.Combine("Images");
            var fullUploadPath = Path.Combine(Directory.GetCurrentDirectory(), uploadsFolder);

            if (!Directory.Exists(fullUploadPath))
                Directory.CreateDirectory(fullUploadPath);

            var uniqueFileName = $"{user.Id}_{userDto.Image.FileName}";
            var filePath = Path.Combine(fullUploadPath, uniqueFileName);

            await using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await userDto.Image.CopyToAsync(fileStream);
            }

            // Сохраняем относительный путь
            user.ImagePath = Path.Combine(uploadsFolder, uniqueFileName);
        }

        _db.Users.Update(user);
        await _db.SaveChangesAsync();

        return Ok(user);
    }
}

public class UserUpdateDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string? Description { get; set; }
    public string Email { get; set; }
    public IFormFile? Image { get; set; }
}