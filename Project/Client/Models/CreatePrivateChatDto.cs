using System;

namespace Client.Models;

public class CreatePrivateChatDto
{
    public Guid Id { get; set; }
    public Guid User1Id { get; set; }
    public Guid User2Id { get; set; }
}
