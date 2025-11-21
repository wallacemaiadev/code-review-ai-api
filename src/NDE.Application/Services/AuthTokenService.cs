using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NDE.Application.Interfaces;
using NDE.Domain.Utils;

namespace NDE.Application.Services
{
  public class AuthTokenService : IAuthTokenService
  {
    private const string TOKEN_PREFIX = "AuthToken";
    private const int TOKEN_LENGTH = 32;

    private readonly IDistributedCache _cache;
    private readonly AppSettings _settings;
    private readonly ILogger<AuthTokenService> _logger;

    public AuthTokenService(IDistributedCache cache, IOptions<AppSettings> settings, ILogger<AuthTokenService> logger)
    {
      _cache = cache;
      _settings = settings.Value;
      _logger = logger;
    }

    public async Task<string> GenerateAccessUrlAsync(Dictionary<string, string> resourceParams, int expirationDays = 7, Dictionary<string, string>? metadata = null)
    {
      _logger.LogInformation("Gerando URL de portal...");

      if (string.IsNullOrWhiteSpace(_settings.Portal))
        throw new ArgumentException("BaseUrl não pode ser vazio", nameof(_settings.Portal));

      if (resourceParams == null || !resourceParams.Any())
        throw new ArgumentException("ResourceParams deve conter ao menos um parâmetro", nameof(resourceParams));

      if (expirationDays < 1 || expirationDays > 7)
        throw new ArgumentException("Expiração deve ser entre 1 e 7 dias", nameof(expirationDays));

      var token = GenerateSecureToken();
      var cacheKey = BuildCacheKey(token);

      var tokenData = new TokenAccessData
      {
        ResourceParams = resourceParams,
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddDays(expirationDays),
        Metadata = metadata ?? new Dictionary<string, string>()
      };

      var jsonData = JsonSerializer.Serialize(tokenData);
      var dataBytes = Encoding.UTF8.GetBytes(jsonData);

      var options = new DistributedCacheEntryOptions()
          .SetAbsoluteExpiration(TimeSpan.FromDays(expirationDays));

      await _cache.SetAsync(cacheKey, dataBytes, options);

      return BuildAccessUrl(_settings.Portal, resourceParams, token);
    }

    public async Task<TokenAccessData?> ValidateTokenAsync(string token, Dictionary<string, string>? resourceParams = null)
    {
      if (string.IsNullOrWhiteSpace(token))
        return null;

      var cacheKey = BuildCacheKey(token);
      var dataBytes = await _cache.GetAsync(cacheKey);

      if (dataBytes == null || dataBytes.Length == 0)
        return null;

      try
      {
        var jsonData = Encoding.UTF8.GetString(dataBytes);
        var tokenData = JsonSerializer.Deserialize<TokenAccessData>(jsonData);

        if (tokenData == null)
          return null;

        if (tokenData.ExpiresAt < DateTime.UtcNow)
        {
          await RevokeTokenAsync(token);
          return null;
        }

        if (resourceParams != null && !ValidateResourceParams(tokenData.ResourceParams, resourceParams))
          return null;

        return tokenData;
      }
      catch
      {
        return null;
      }
    }

    public async Task<TokenAccessData?> ValidateFromQueryStringAsync(string? queryString)
    {
      var queryParams = ParseQueryString(queryString);

      if (!queryParams.TryGetValue("authtoken", out var token))
        return null;

      queryParams.Remove("authtoken");

      return await ValidateTokenAsync(token, queryParams.Any() ? queryParams : null);
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
      if (string.IsNullOrWhiteSpace(token))
        return false;

      var cacheKey = BuildCacheKey(token);
      await _cache.RemoveAsync(cacheKey);
      return true;
    }

    public async Task<bool> RefreshTokenAsync(string token, int expirationDays = 7)
    {
      var tokenData = await ValidateTokenAsync(token);

      if (tokenData == null)
        return false;

      tokenData.ExpiresAt = DateTime.UtcNow.AddDays(expirationDays);

      var cacheKey = BuildCacheKey(token);
      var jsonData = JsonSerializer.Serialize(tokenData);
      var dataBytes = Encoding.UTF8.GetBytes(jsonData);

      var options = new DistributedCacheEntryOptions()
          .SetAbsoluteExpiration(TimeSpan.FromDays(expirationDays));

      await _cache.SetAsync(cacheKey, dataBytes, options);
      return true;
    }

    private string GenerateSecureToken()
    {
      var randomBytes = new byte[TOKEN_LENGTH];
      using (var rng = RandomNumberGenerator.Create())
      {
        rng.GetBytes(randomBytes);
      }
      return Convert.ToBase64String(randomBytes)
          .Replace("+", "-")
          .Replace("/", "_")
          .Replace("=", "");
    }

    private string BuildAccessUrl(string baseUrl, Dictionary<string, string> resourceParams, string token)
    {
      var uriBuilder = new UriBuilder(baseUrl);
      var query = HttpUtility.ParseQueryString(uriBuilder.Query);

      foreach (var param in resourceParams)
      {
        query[param.Key] = param.Value;
      }

      query["token"] = token;

      uriBuilder.Query = query.ToString();
      return uriBuilder.ToString();
    }

    private string BuildCacheKey(string token)
    {
      return $"{TOKEN_PREFIX}:{token}";
    }

    private bool ValidateResourceParams(
        Dictionary<string, string> storedParams,
        Dictionary<string, string> providedParams)
    {
      foreach (var param in storedParams)
      {
        if (!providedParams.TryGetValue(param.Key, out var value) || value != param.Value)
          return false;
      }
      return true;
    }

    private Dictionary<string, string> ParseQueryString(string? queryString)
    {
      var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

      if (string.IsNullOrWhiteSpace(queryString))
        return result;

      queryString = queryString.TrimStart('?');
      var pairs = queryString.Split('&');

      foreach (var pair in pairs)
      {
        var parts = pair.Split('=');
        if (parts.Length == 2)
        {
          var key = HttpUtility.UrlDecode(parts[0]);
          var value = HttpUtility.UrlDecode(parts[1]);
          result[key] = value;
        }
      }

      return result;
    }
  }
}