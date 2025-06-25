using System.Text.Json.Serialization;

namespace Server.Models;

public class User
{
    public Guid Id { get; set; }
    public string Login { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    public string? Description { get; set; }
    public string ImagePath { get; set; }

    //[JsonIgnore]
    //public List<Chat> Chats { get; set; }

    [JsonIgnore]
    public List<PrivateChat> PrivateChatsAsUser1 { get; set; } = new();

    [JsonIgnore]
    public List<PrivateChat> PrivateChatsAsUser2 { get; set; } = new();
}
