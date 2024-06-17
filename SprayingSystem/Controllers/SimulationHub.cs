using Microsoft.AspNetCore.SignalR;

public class SimulationHub : Hub
{
    public async Task SendSimulationData(string data)
    {
        await Clients.All.SendAsync("ReceiveMessage", data);
    }
}