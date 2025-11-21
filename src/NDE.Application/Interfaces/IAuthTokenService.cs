using NDE.Domain.Utils;

namespace NDE.Application.Interfaces
{
  public interface IAuthTokenService
  {
    Task<string> GenerateAccessUrlAsync(Dictionary<string, string> resourceParams, int expirationDays = 7, Dictionary<string, string>? metadata = null);
    Task<TokenAccessData?> ValidateTokenAsync(string token, Dictionary<string, string>? resourceParams = null);
    Task<TokenAccessData?> ValidateFromQueryStringAsync(string? queryString);
    Task<bool> RevokeTokenAsync(string token);
    Task<bool> RefreshTokenAsync(string token, int expirationDays = 7);
  }
}
