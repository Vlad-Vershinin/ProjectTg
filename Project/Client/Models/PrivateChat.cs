﻿using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace Client.Models;

public class PrivateChat : Chat
{
    public Guid User1Id { get; set; }
    public Guid User2Id { get; set; }

    [JsonIgnore]
    public User? User1 { get; set; }

    [JsonIgnore]
    public User? User2 { get; set; }

    [JsonIgnore]
    public Guid? CurrentUserId { get; set; }
    [JsonIgnore]
    public string OtherUserAvatarUrl { get; set; }
}