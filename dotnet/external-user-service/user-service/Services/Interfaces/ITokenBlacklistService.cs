namespace AspNetJwtAuth.Services.Interfaces
{
    public interface ITokenBlacklistService
    {
        Task<bool> IsTokenBlacklistedAsync(string jti);
        Task BlacklistTokenAsync(string jti, TimeSpan expiry);
        Task<int> GetBlacklistedTokenCountAsync();
    }
}
