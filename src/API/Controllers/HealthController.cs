using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Repositories.Context;
using StackExchange.Redis;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly MongoDbContext _mongoDbContext;
    private readonly IConnectionMultiplexer _redisConnection;

    public HealthController(MongoDbContext mongoDbContext, IConnectionMultiplexer redisConnection)
    {
        _mongoDbContext = mongoDbContext;
        _redisConnection = redisConnection;
    }

    /// <summary>
    /// Health check endpoint to verify presentation layer and persistence connections.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CheckHealth()
    {
        var redisStatus = "Disconnected";
        var mongoStatus = "Disconnected";
        var isHealthy = true;

        // Check Redis
        try
        {
            var db = _redisConnection.GetDatabase();
            var pingResult = await db.PingAsync();
            redisStatus = $"Connected ({pingResult.TotalMilliseconds}ms)";
        }
        catch (Exception ex)
        {
            redisStatus = $"Error: {ex.Message}";
            isHealthy = false;
        }

        // Check MongoDB
        try
        {
            var canConnect = await _mongoDbContext.Database.CanConnectAsync();
            mongoStatus = canConnect ? "Connected" : "Unable to connect";
            if (!canConnect) isHealthy = false;
        }
        catch (Exception ex)
        {
            mongoStatus = $"Error: {ex.Message}";
            isHealthy = false;
        }

        var response = new
        {
            Status = isHealthy ? "Healthy" : "Unhealthy",
            Timestamp = DateTime.UtcNow,
            Components = new
            {
                PresentationApi = "Healthy",
                RedisCache = redisStatus,
                MongoDbPersistence = mongoStatus
            }
        };

        if (isHealthy)
        {
            return Ok(response);
        }

        return StatusCode(503, response);
    }
}
