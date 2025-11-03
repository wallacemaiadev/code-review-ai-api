using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.Extensions.Caching.Distributed;

namespace NDE.Domain.Caching;

public static class DistributedCacheExtensions
{
  public static Task SetDefaultAsync<T>(this IDistributedCache cache, string key, T value) => cache.SetAsync(key, value, SetDefaultExpiration());
  public static Task SetAsync<T>(this IDistributedCache cache, string key, T value) => cache.SetAsync(key, value, new DistributedCacheEntryOptions());
  public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options)
  {
    var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value, GetJsonSerializerOptions()));
    return cache.SetAsync(key, bytes, options);
  }
  public static bool TryGetValue<T>(this IDistributedCache cache, string key, out T? value)
  {
    var cacheValue = cache.Get(key);
    value = default;

    if (cacheValue == null)
      return false;

    value = JsonSerializer.Deserialize<T>(cacheValue, GetJsonSerializerOptions());

    return true;
  }

  private static JsonSerializerOptions GetJsonSerializerOptions()
  {
    return new JsonSerializerOptions()
    {
      PropertyNamingPolicy = null,
      WriteIndented = true,
      AllowTrailingCommas = true,
      DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
  }

  private static DistributedCacheEntryOptions SetDefaultExpiration()
  {
    return new DistributedCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
        .SetSlidingExpiration(TimeSpan.FromMinutes(5));
  }
}