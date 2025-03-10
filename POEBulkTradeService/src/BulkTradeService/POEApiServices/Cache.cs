using System;
using System.Collections.Concurrent;
using System.Threading;
using Newtonsoft.Json;

public class Cache
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly int _minTtl = 300;

    public T Get<T>(object key)
    {
        var hashedKey = HashKey(key);
        return _cache.TryGetValue(hashedKey, out var entry) && !IsExpired(entry)
            ? (T)entry.Value
            : default;
    }

    public void Set<T>(object key, T value, int ttl)
    {
        var hashedKey = HashKey(key);
        var expiration = DateTime.UtcNow.AddSeconds(Math.Max(_minTtl, ttl));

        _cache.AddOrUpdate(hashedKey,
            new CacheEntry(value, expiration),
            (_, __) => new CacheEntry(value, expiration));

        // Create a timer to remove the cached item after the TTL expires
        TimerCallback timerCallback = state =>
        {
            _cache.TryRemove(hashedKey, out _);
        };

        var timer = new Timer(timerCallback, null, (int)TimeSpan.FromSeconds(ttl).TotalMilliseconds, Timeout.Infinite);

        // Ensure the timer is disposed when the cache entry is removed
        _cache[hashedKey].Timer = timer;
    }

    private static string HashKey(object key) =>
        BitConverter.ToString(System.Security.Cryptography.SHA1.Create()
            .ComputeHash(System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(key))));

    private static bool IsExpired(CacheEntry entry) => DateTime.UtcNow >= entry.Expiration;

    private class CacheEntry
    {
        public object Value { get; }
        public DateTime Expiration { get; }
        public Timer Timer { get; set; }

        public CacheEntry(object value, DateTime expiration)
        {
            Value = value;
            Expiration = expiration;
        }
    }
}