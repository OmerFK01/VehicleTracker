using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using VehicleTracker.Models;
using System.Text.Json;

namespace VehicleTracker.Hubs;

public class VehicleHub(IConnectionMultiplexer redis) : Hub
{
    public override async Task OnConnectedAsync()
    {
        var db = redis.GetDatabase();
        var server = redis.GetServer(redis.GetEndPoints()[0]);
        foreach (var key in server.Keys(pattern: "vehicle:*"))
        {
            var json = await db.StringGetAsync(key);
            if (json.HasValue)
            {
                await Clients.Caller.SendAsync("positionUpdate", JsonSerializer.Deserialize<VehicleUpdate>(json.ToString()));
            }
            await base.OnConnectedAsync();
        }
    }

}