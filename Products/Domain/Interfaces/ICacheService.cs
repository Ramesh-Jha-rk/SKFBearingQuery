namespace Products.Domain.Interfaces;

/// <summary>
/// Service interface for caching
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Get value from cache
    /// </summary>
    Task<string?> GetAsync(string key);

    /// <summary>
    /// Set value in cache with optional TTL
    /// </summary>
    Task SetAsync(string key, string value, TimeSpan? ttl = null);

    /// <summary>
    /// Delete a specific key from cache
    /// </summary>
    Task DeleteAsync(string key);

    /// <summary>
    /// Clear all cache entries matching a pattern
    /// </summary>
    Task ClearAsync(string? pattern = null);
}
