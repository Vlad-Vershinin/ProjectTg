using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Server.Models;

public class Chat
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }

    [JsonIgnore]
    public ObservableCollection<Message> Messages { get; set; } = new();
}
