using System.Text.Json;
using StackExchange.Redis;

namespace shala.api.modules.cache;

public class RedisCacheService: ICacheService, IDisposable
{
    private readonly ConnectionMultiplexer _redis;
    private readonly IDatabase _db;
    private readonly IServer _server;
    private readonly TimeSpan _expirationMinutes = TimeSpan.FromMinutes(5);

    public RedisCacheService(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Cache:Redis:ConnectionString");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception("Redis connection string not found");
        }
        _redis = ConnectionMultiplexer.Connect(connectionString);
        _db = _redis.GetDatabase();
        _server = _redis.GetServer(_redis.GetEndPoints().First());
        var expirationMinutes = configuration.GetValue<int>("Cache:ExpirationMinutes");
        if (expirationMinutes > 0)
        {
            _expirationMinutes = TimeSpan.FromMinutes(expirationMinutes);
        }
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty || value.IsNull)
        {
            return default;
        }

        #pragma warning disable CS8604 // Possible null reference argument.
        return JsonSerializer.Deserialize<T>(value);
        #pragma warning restore CS8604 // Possible null reference argument.
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        if (expiration == null)
        {
            expiration = _expirationMinutes;
        }
        var serializedValue = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, serializedValue, expiration);
    }

    public async Task<bool> HasAsync(string key)
    {
        return await _db.KeyExistsAsync(key);
    }

    public bool Invalidate(List<string> keys)
    {
        if (keys == null || keys.Count == 0)
        {
            return false;
        }
        return _db.KeyDelete(keys.Select(x => (RedisKey)x).ToArray()) > 0;
    }

    public bool Remove(string key)
    {
        return _db.KeyDelete(key);
    }

    public List<string> FindAndClear<T>(string searchPattern)
    {
        var matchingKeys = new List<string>();

        // Use SCAN to safely search for keys matching the pattern
        var cursor = 0;
        do
        {
            var result = _server.Keys(_db.Database, $"*{searchPattern}*", cursor: cursor);
            foreach (var key in result)
            {
                matchingKeys.Add(key.ToString());

                // Delete the key
                _db.KeyDelete(key);
            }
            cursor++;
        } while (cursor != 0);

        return matchingKeys;
    }

    public async Task<long> SizeAsync()
    {
        return await _server.DatabaseSizeAsync();
    }

    public void Dispose()
    {
        _redis.Dispose();
    }

    public async Task ClearAllAsync()
    {
        await _server.FlushDatabaseAsync();
    }

}




