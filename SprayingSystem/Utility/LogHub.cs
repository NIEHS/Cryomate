using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class LogHub : Hub
{
    public async Task SendLog(string log)
    {
        await Clients.All.SendAsync("ReceiveLog", log);
    }
}
