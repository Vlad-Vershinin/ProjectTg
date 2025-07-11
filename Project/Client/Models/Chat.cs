﻿using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace Client.Models;

public abstract class Chat
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    [JsonIgnore]
    [Reactive] public string LastMessage { get; set; }

    [JsonIgnore]
    public ObservableCollection<Message> Messages { get; set; } = new();
}