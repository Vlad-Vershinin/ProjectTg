namespace Server.Models.Dtos;

public class RegisterDto
{
    public Guid Id { get; set; }
    public string Login { get; set; }
    public string Password { get; set; }
    public string Email { get; set; }
}
