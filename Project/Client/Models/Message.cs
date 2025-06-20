using System.Text.Json.Serialization;
using System;

namespace Client.Models;

public class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Text { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public Guid ChatId { get; set; }
    public Guid SenderId { get; set; }

    [JsonIgnore]
    public Chat? Chat { get; set; }

    [JsonIgnore]
    public User? Sender { get; set; }
}