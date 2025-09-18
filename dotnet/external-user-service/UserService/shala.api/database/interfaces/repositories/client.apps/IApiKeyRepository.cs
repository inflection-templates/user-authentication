using shala.api.database.interfaces.models;
using shala.api.domain.types;

namespace shala.api.database.interfaces;

public interface IApiKeyRepository
  : ICRUDRepository<
    ApiKey,
    ApiKeyCreateModel,
    ApiKeyUpdateModel,
    ApiKeySearchFilters,
    IDbModel>
{
    Task<bool> DeleteByClientAppIdAsync(Guid clientAppId);
    Task<List<ApiKey>> GetByClientAppIdAsync(Guid clientAppId);
    Task<ApiKey?> GetByKeyAsync(string key);
    Task<string?> GetSecretHashAsync(string key);

}
