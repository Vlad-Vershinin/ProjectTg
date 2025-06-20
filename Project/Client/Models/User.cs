using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Client.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [StringLength(20, MinimumLength = 3)]
    public string UserName { get; set; } = string.Empty;

    [JsonIgnore]
    public ObservableCollection<Chat> Chats { get; set; } = new ObservableCollection<Chat>();
}
