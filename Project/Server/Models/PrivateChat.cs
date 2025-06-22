using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Server.Models;

[Table("PrivateChats")]
public class PrivateChat : Chat
{
    public Guid User1Id { get; set; }
    public Guid User2Id { get; set; }

    [JsonIgnore]
    public List<Message> Messages { get; set; } = new();

    [JsonIgnore]
    public User User1 { get; set; }
    [JsonIgnore]
    public User User2 { get; set; }
}