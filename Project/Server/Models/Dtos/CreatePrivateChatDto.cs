namespace Server.Models.Dtos;

public class CreatePrivateChatDto
{
    public Guid Id { get; set; }
    public Guid User1Id { get; set; }
    public Guid User2Id { get; set; }
}
