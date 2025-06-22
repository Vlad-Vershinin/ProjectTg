using System.Text.Json.Serialization;

namespace Server.Models;

public class Message
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; } = false;

    public Guid ChatId { get; set; }
    public Guid SenderId { get; set; }

    [JsonIgnore]
    public Chat? Chat { get; set; }
    [JsonIgnore]
    public User? Sender { get; set; }
}