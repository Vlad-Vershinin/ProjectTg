﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Data;
using Server.Models;
using Server.Models.Dtos;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly DataContext db_;

    public AuthController(DataContext data)
    {
        db_ = data;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var user = await db_.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Login == loginDto.Login);

        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            return BadRequest("Неверный логин или пароль");
        }

        var token = GenerateJwtToken(user);
        return Ok(token);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        if (await db_.Users.AnyAsync(u => u.Login == registerDto.Login))
            return BadRequest("Пользователь уже существует");

        var user = new User
        {
            Id = registerDto.Id,
            Login = registerDto.Login,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            Email = registerDto.Email,
            UserName = registerDto.Login,
            ImagePath = "avatar"
        };

        db_.Users.Add(user);
        await db_.SaveChangesAsync();

        var token = GenerateJwtToken(user);
        return Ok(token);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("DTEp86T1AB7YJVcs1OHecLNRPxHPFYf7JB27N9rqSGY="));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: "https://localhost:7068",
            audience: "Client",
            claims: claims,
            expires: DateTime.Now.AddDays(2),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
