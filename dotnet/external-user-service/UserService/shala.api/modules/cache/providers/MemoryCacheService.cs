using Microsoft.Extensions.Caching.Memory;

namespace shala.api.modules.cache;

public class MemoryCacheService: ICacheService, IDisposable
{
    private TimeSpan _expirationMinutes = TimeSpan.FromMinutes(5);
    private readonly IMemoryCache _cache;
    private readonly List<string> _keyRegistry; // To store all cache keys

    public MemoryCacheService(IConfiguration configuration, IMemoryCache cache)
    {
        _cache = cache;
        _keyRegistry = new List<string>();
        var expirationMinutes = configuration.GetValue<int>("Cache:ExpirationMinutes");
        if (expirationMinutes > 0)
        {
            _expirationMinutes = TimeSpan.FromMinutes(expirationMinutes);
        }
    }

    public Task<T?> GetAsync<T>(string key)
    {
        return Task.FromResult(_cache.TryGetValue(key, out T? value) ? value : default);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        // Add the key to the registry
        if (!_keyRegistry.Contains(key))
        {
            _keyRegistry.Add(key);
        }
        if (expiration == null)
        {
            expiration = _expirationMinutes;
        }
        _cache.Set(key, value, expiration.Value);
        return Task.CompletedTask;
    }

    public Task<bool> HasAsync(string key)
    {
        return Task.FromResult(_cache.TryGetValue(key, out _));
    }

    public Task<long> SizeAsync()
    {
        var cacheSize = ((MemoryCache)_cache).Count;
        return Task.FromResult<long>((long)cacheSize);
    }

    public bool Invalidate(List<string> keys)
    {
        if (keys == null || keys.Count == 0)
        {
            return false;
        }
        foreach (var key in keys)
        {
            _cache.Remove(key);
            _keyRegistry.Remove(key); // Remove the key from the registry as well
        }
        return true;
    }

    public bool Remove(string key)
    {
        _keyRegistry.Remove(key);
        _cache.Remove(key);
        return true;
    }

    public List<string> FindAndClear<T>(string searchPattern)
    {
        var matchingKeys = _keyRegistry.Where(k => k.Contains(searchPattern)).ToList();
        foreach (var key in matchingKeys)
        {
            _cache.Remove(key);
            _keyRegistry.Remove(key); // Remove the key from the registry as well
        }
        return matchingKeys;
    }

    public void Dispose()
    {
        _cache.Dispose();
    }
}

