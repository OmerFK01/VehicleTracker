using VehicleTracker.Services;
using VehicleTracker.Hubs;
using StackExchange.Redis;


var builder = WebApplication.CreateBuilder(args);

// Redis connection string (appsettings locally, ConnectionStrings__Redis in Docker)
string redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? throw new InvalidOperationException("Redis connection string is missing");

var redisOptions = ConfigurationOptions.Parse(redisConnectionString);
redisOptions.AbortOnConnectFail = false; //keep retrying instead of crashing

// SignalR with Redis backplane (same Redis address as the cache)
builder.Services.AddSignalR()
    .AddStackExchangeRedis(redisConnectionString, o => o.Configuration.AbortOnConnectFail = false);

//var redisHost = builder.Configuration["Redis:Host"] ?? "localhost:6379";
//var redisCs = redisHost.Contains("abortConnect") ? redisHost : redisHost + ",abortConnect=false";

//builder.Services.AddSignalR()
//    .AddStackExchangeRedis(redisCs); 

//Register a single shared IConnectionMultiplexer instance
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    //try
    //{
        //Connect with recommended settings
        var multiplexer = ConnectionMultiplexer.Connect(redisOptions);
        
        //Optional: log connection status
        Console.WriteLine($"Connected to Redis: {multiplexer.Configuration}");
        return multiplexer;

    //}
    //catch(RedisConnectionException ex) 
    //{
    //    Console.Error.WriteLine($"Redis connection failed: {ex.Message}");
    //    //throw; //Fail fast if Redis is critical
    //}
});

if (builder.Configuration.GetValue<bool>("RUN_SIMULATOR", true))
    builder.Services.AddHostedService<VehicleSimulator>();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHub<VehicleHub>("/hub/vehicles");

//Example usage: resolve and use Redis
app.MapGet("/redis-test", (IConnectionMultiplexer redis) =>
{
    var db = redis.GetDatabase();
    db.StringSet("test-key", "Hello Redis!");
    return db.StringGet("test-key");
});

app.MapGet("/health", async (IConnectionMultiplexer mux) =>
{
    try { 
        await mux.GetDatabase().PingAsync(); 
        return Results.Ok("Redis OK"); 
    }
    catch (Exception ex) { 
        return Results.Problem($"Redis unavailable: {ex.Message}"); 
    }
});

app.Run();
