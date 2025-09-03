
namespace shala.api.modules.cache;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task<bool> HasAsync(string key);
    Task<long> SizeAsync();
    bool Remove(string key);
    bool Invalidate(List<string> keys);
    List<string> FindAndClear<T>(string searchPattern);
}
