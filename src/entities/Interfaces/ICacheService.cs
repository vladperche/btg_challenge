using System;
using System.Threading.Tasks;

namespace Entities.Interfaces;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? timeToLive = null);
    Task RemoveAsync(string key);
}
