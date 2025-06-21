using Microsoft.AspNetCore.SignalR;
using Server.Models;

namespace Server.Controllers;

public class ChatHub : Hub
{
    public async Task Send(Message message)
    {
        await this.Clients.All.SendAsync("Receive", message);
    }
}
