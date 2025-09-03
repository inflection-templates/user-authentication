namespace shala.api.integration.tests;

public static class TestDataCache
{
    private static Dictionary<string, object> _cache = new Dictionary<string, object>();

    public static void Set(string key, object value)
    {
        _cache[key] = value;
    }

    public static T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        return default(T);
    }

    public static void Clear()
    {
        _cache.Clear();
    }
}
