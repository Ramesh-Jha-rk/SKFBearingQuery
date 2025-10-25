using Microsoft.Extensions.Configuration;
using Products.Domain.Interfaces;
using StackExchange.Redis;

namespace Products.Infrastructure.Caching;

/// <summary>
/// Redis implementation of cache service
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDatabase? _db;
    private readonly IServer? _server;

    public RedisCacheService(IConfiguration configuration)
    {
    var conn = configuration["Redis:Connection"];
        if (!string.IsNullOrEmpty(conn))
        {
    try
       {
          var mux = ConnectionMultiplexer.Connect(conn);
      _db = mux.GetDatabase();
   _server = mux.GetServer(mux.GetEndPoints().First());
     }
   catch
         {
     // Gracefully handle Redis connection failures
          _db = null;
         _server = null;
  }
        }
    }

    public async Task<string?> GetAsync(string key)
    {
        if (_db == null) return null;
   try
        {
 return await _db.StringGetAsync(key);
        }
        catch
        {
     return null;
        }
    }

    public async Task SetAsync(string key, string value, TimeSpan? ttl = null)
    {
        if (_db == null) return;
        try
        {
     await _db.StringSetAsync(key, value, ttl);
        }
        catch
      {
            // Silently fail if cache unavailable
        }
    }

    public async Task DeleteAsync(string key)
    {
        if (_db == null) return;
        try
        {
   await _db.KeyDeleteAsync(key);
        }
        catch
        {
     // Silently fail if cache unavailable
        }
    }

    public async Task ClearAsync(string? pattern = null)
    {
        if (_db == null || _server == null) return;
    try
        {
      var keys = _server.Keys(pattern: pattern ?? "query:*").ToArray();
     if (keys.Length > 0)
  {
      await _db.KeyDeleteAsync(keys);
            }
        }
   catch
        {
    // Silently fail if cache unavailable
        }
    }
}
