using System;
using System.Text.Json;
using System.Threading.Tasks;
using Entities.Interfaces;
using StackExchange.Redis;

namespace Services;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly JsonSerializerOptions _jsonOptions;

    public RedisCacheService(IConnectionMultiplexer redisConnection)
    {
        _redisConnection = redisConnection;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true
        };
    }

    private IDatabase Database => _redisConnection.GetDatabase();

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var value = await Database.StringGetAsync(key);
            if (value.IsNullOrEmpty)
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
        }
        catch
        {
            // Fail safe on cache errors
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? timeToLive = null)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            if (timeToLive.HasValue)
            {
                await Database.StringSetAsync(key, json, timeToLive.Value);
            }
            else
            {
                await Database.StringSetAsync(key, json);
            }
        }
        catch
        {
            // Fail safe on cache errors
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await Database.KeyDeleteAsync(key);
        }
        catch
        {
            // Fail safe on cache errors
        }
    }
}
