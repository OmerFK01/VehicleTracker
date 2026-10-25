using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using VehicleTracker.Hubs;
using VehicleTracker.Models;

namespace VehicleTracker.Services;

public class VehicleSimulator(IHubContext<VehicleHub> hub, IConnectionMultiplexer redis) : BackgroundService
{
    private readonly Random _rng = new();

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var vehicles = Enumerable.Range(1, 10).Select(i => new VehicleUpdate(
        $"GO-{i:00}", i % 2 == 0 ? "Lakeshore West" : "Kitchener",
        43.645 + _rng.NextDouble() * 0.05, -79.38 + _rng.NextDouble() * 0.05,
        0, DateTime.UtcNow)).ToList();

        while (!cancellationToken.IsCancellationRequested)
        {
            for (int i = 0; i < vehicles.Count; i++)
            {
                vehicles[i] = vehicles[i] with
                {
                    Lat = vehicles[i].Lat + (_rng.NextDouble() - 0.5) * 0.002,
                    Lng = vehicles[i].Lng + (_rng.NextDouble() - 0.5) * 0.002,
                    Speed = _rng.Next(0, 100),
                    TimeStamp = DateTime.UtcNow
                };

                await redis.GetDatabase().StringSetAsync(
                     $"vehicle:{vehicles[i].Id}",
                     System.Text.Json.JsonSerializer.Serialize(vehicles[i]),
                     TimeSpan.FromMinutes(5));

                await hub.Clients.All.SendAsync("positionUpdate", vehicles[i], cancellationToken);
            }

            await Task.Delay(1000, cancellationToken);
        }

    }


}

