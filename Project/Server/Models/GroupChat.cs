using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Server.Models;

public class GroupChat : Chat
{
    public Guid AdminId { get; set; }

    [JsonIgnore]
    public User? Admin { get; set; }

    [JsonIgnore]
    public List<User> Members { get; set; } = new();
}
