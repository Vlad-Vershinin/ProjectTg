using Avalonia.Media.Imaging;
using Microsoft.AspNetCore.StaticAssets;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Client.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Login { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    [StringLength(20, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [StringLength(120)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string ImagePath { get; set; } = "/Assets/profile.png";

    [JsonIgnore]
    [Reactive] public Bitmap Avatar { get; set; }

    [JsonIgnore]
    public ObservableCollection<Chat> Chats { get; set; } = new ObservableCollection<Chat>();
}
