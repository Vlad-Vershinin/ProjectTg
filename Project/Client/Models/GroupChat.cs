using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using System;

namespace Client.Models;

public class GroupChat : Chat
{
    public Guid AdminId { get; set; }

    [JsonIgnore]
    public User? Admin { get; set; }

    [JsonIgnore]
    public ObservableCollection<User> Members { get; set; } = new();
}