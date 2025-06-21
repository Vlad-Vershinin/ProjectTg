namespace Server.Models.Dtos;

public class CreateGroupChatDto
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public Guid AdminId { get; set; }
}
