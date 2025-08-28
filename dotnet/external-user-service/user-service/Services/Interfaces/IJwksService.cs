using Microsoft.IdentityModel.Tokens;

namespace AspNetJwtAuth.Services.Interfaces
{
    public interface IJwksService
    {
        Task<IEnumerable<SecurityKey>> GetSigningKeysAsync(string? kid = null);
        Task RefreshKeysAsync();
        Task<int> GetCachedKeyCountAsync();
    }
}
