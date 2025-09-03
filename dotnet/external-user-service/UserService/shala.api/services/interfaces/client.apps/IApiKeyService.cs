using shala.api.domain.types;

namespace shala.api.services;

public interface IApiKeyService
{
    Task<ApiKey?> GetByIdAsync(Guid id);
    Task<ApiKey?> GetByKeyAsync(string key);
    Task<ClientApp?> ValidateAsync(string key, string secret);
    Task<ApiKey?> CreateAsync(ApiKeyCreateModel model);
    Task<List<ApiKey>> GetByClientAppIdAsync(Guid clientAppId);
    Task<ClientApp?> GetClientAppByKeyAsync(string key);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteByClientAppIdAsync(Guid clientAppId);
    Task<SearchResults<ApiKey>> SearchAsync(ApiKeySearchFilters filters);

}
